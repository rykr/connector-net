// Copyright (C) 2004-2006 MySQL AB
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Text;
using System.IO;

namespace MySql.Data.MySqlClient
{
    internal abstract class Statement
    {
        protected MySqlConnection connection;
        protected Driver driver;
        protected string commandText;
        private ArrayList buffers;
        protected MySqlParameterCollection parameters;
        protected string preCommand;
        protected string postCommand;

        private Statement(MySqlConnection connection)
        {
            this.connection = connection;
            this.driver = connection.driver;
            buffers = new ArrayList();
        }

        public Statement(MySqlConnection connection, string text) : this(connection)
        {
            commandText = text;
        }

        #region Properties

        public virtual string ProcessedCommandText
        {
            get { return commandText; }
        }

        public string PreCommand
        {
            get { return preCommand; }
            set { preCommand = value; }
        }

        public string PostCommand
        {
            get { return postCommand; }
            set { postCommand = value; }
        }

        #endregion

        public virtual void Close()
        {
        }

        private string GetCommandText()
        {
            StringBuilder sb = new StringBuilder(PreCommand);
            if (sb.Length > 0)
                sb.Append(";");
            sb.Append(ProcessedCommandText);
            if (PostCommand != null)
            {
                sb.Append(";");
                sb.Append(PostCommand);
            }
            return sb.ToString();
        }

        public virtual void Execute(MySqlParameterCollection parameters)
        {
            this.parameters = parameters;

            // we keep a reference to this until we are done
            BindParameters();
            ExecuteNext();
        }

        public virtual bool ExecuteNext()
        {
            if (buffers.Count == 0)
                return false;

            MemoryStream ms = (MemoryStream)buffers[0];
            driver.Query(ms.GetBuffer(), (int)ms.Length);
            buffers.RemoveAt(0);
            return true;
        }

        protected virtual void BindParameters()
        {
            // tokenize the sql
            ArrayList tokenArray = TokenizeSql(GetCommandText());

            MySqlStream stream = new MySqlStream(driver.Encoding);
            stream.Version = driver.Version;

            // make sure our token array ends with a ;
            string lastToken = (string)tokenArray[tokenArray.Count - 1];
            if (lastToken != ";")
                tokenArray.Add(";");

            foreach (String token in tokenArray)
            {
                if (token.Trim().Length == 0) 
                    continue;
                if (token == ";")
                {
                    buffers.Add(stream.InternalBuffer);
                    stream = new MySqlStream(driver.Encoding);
                    continue;
                }
                if (token[0] == parameters.ParameterMarker)
                {
                    if (SerializeParameter(parameters, stream, token))
                        continue;
                }
 
                // our fall through case is to write the token to the byte stream
                stream.WriteStringNoNull(token);
            }
        }

        /// <summary>
        /// Serializes the given parameter to the given memory stream
        /// </summary>
        /// <remarks>
        /// <para>This method is called by PrepareSqlBuffers to convert the given
        /// parameter to bytes and write those bytes to the given memory stream.
        /// </para>
        /// </remarks>
        /// <returns>True if the parameter was successfully serialized, false otherwise.</returns>
        private bool SerializeParameter(MySqlParameterCollection parameters, 
            MySqlStream stream, string parmName)
        {
            int index = parameters.IndexOf(parmName);
            if (index == -1)
            {
                // if we are using old syntax, we can't throw exceptions for parameters
                // not defined.
                if (connection.Settings.UseOldSyntax) 
                    return false;
                throw new MySqlException("Parameter '" + parmName + "' must be defined");
            }
            MySqlParameter parameter = parameters[index];
            parameter.Serialize(stream, false);
            return true;
        }

        /// <summary>
        /// Breaks the given SQL up into 'tokens' that are easier to output
        /// into another form (bytes, preparedText, etc).
        /// </summary>
        /// <param name="sql">SQL to be tokenized</param>
        /// <returns>Array of tokens</returns>
        /// <remarks>The SQL is tokenized at parameter markers ('?') and at 
        /// (';') sql end markers if the server doesn't support batching.
        /// </remarks>
        public ArrayList TokenizeSql(string sql)
        {
            bool batch = connection.Settings.AllowBatch & driver.SupportsBatch;
            char delim = Char.MinValue;
            StringBuilder sqlPart = new StringBuilder();
            bool escaped = false;
            bool inLineComment = false;
            bool inComment = false;
            ArrayList tokens = new ArrayList();

            sql = sql.TrimStart(';').TrimEnd(';');
            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];
                if (escaped)
                    escaped = !escaped;
                else if (c == delim)
                    delim = Char.MinValue;
                else if (c == '#' && delim == Char.MinValue)
                {
                    inLineComment = true;
                    continue;
                }
                else if (c == '\n' && inLineComment == true)
                {
                    inLineComment = false;
                    continue;
                }
                else if (c == '/')
                {
                    if (sql.Length > (i + 1) && sql[i + 1] == '*' && delim == Char.MinValue)
                    {
                        inComment = true; continue;
                    }
                    else if (inComment && delim == Char.MinValue && i != 0 && sql[i - 1] == '*')
                    {
                        inComment = false; continue;
                    }
                }
                else if (inLineComment || inComment)
                    continue;
                else if (c == ';' && !escaped && delim == Char.MinValue && !batch)
                {
                    tokens.Add(sqlPart.ToString());
                    tokens.Add(";");
                    sqlPart.Remove(0, sqlPart.Length);
                    continue;
                }
                else if ((c == '\'' || c == '\"' || c == '`') & !escaped & delim == Char.MinValue)
                    delim = c;
                else if (c == '\\')
                    escaped = !escaped;
                else if (c == connection.ParameterMarker && delim == Char.MinValue && !escaped)
                {
                    tokens.Add(sqlPart.ToString());
                    sqlPart.Remove(0, sqlPart.Length);
                }
                else if (sqlPart.Length > 0 && sqlPart[0] == connection.ParameterMarker &&
                    !Char.IsLetterOrDigit(c) && c != '_' && c != '.' && c != '$' &&
                    ((c != '@' && c != connection.ParameterMarker) &&
                     (c != '?' && c != connection.ParameterMarker)))
                {
                    tokens.Add(sqlPart.ToString());
                    sqlPart.Remove(0, sqlPart.Length);
                }

                sqlPart.Append(c);
            }
            tokens.Add(sqlPart.ToString());
            return tokens;
        }
    }
}
