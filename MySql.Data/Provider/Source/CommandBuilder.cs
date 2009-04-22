// Copyright (c) 2004-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc.
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
using System.ComponentModel;
using System.Data.Common;
using System.Data;
using System.Text;
using MySql.Data.Common;
using System.Collections;
using MySql.Data.Types;
using System.Globalization;
using MySql.Data.MySqlClient.Properties;

namespace MySql.Data.MySqlClient
{
    /// <include file='docs/MySqlCommandBuilder.xml' path='docs/class/*'/>
#if !CF
    [ToolboxItem(false)]
    [System.ComponentModel.DesignerCategory("Code")]
#endif
    public sealed class MySqlCommandBuilder : DbCommandBuilder
    {
        private string finalSelect;
        private bool returnGeneratedIds;

        #region Constructors

        /// <include file='docs/MySqlCommandBuilder.xml' path='docs/Ctor/*'/>
        public MySqlCommandBuilder()
        {
            QuotePrefix = QuoteSuffix = "`";
			ReturnGeneratedIdentifiers = true;
        }

        /// <include file='docs/MySqlCommandBuilder.xml' path='docs/Ctor2/*'/>
        public MySqlCommandBuilder(MySqlDataAdapter adapter)
            : this()
        {
            DataAdapter = adapter;
        }

        #endregion

        #region Properties

        /// <include file='docs/mysqlcommandBuilder.xml' path='docs/DataAdapter/*'/>
        public new MySqlDataAdapter DataAdapter
        {
            get { return (MySqlDataAdapter)base.DataAdapter; }
            set { base.DataAdapter = value; }
        }

        /// <summary>
        /// Indicates whether the command builder should generate a SELECT statement
        /// to populate any autogenerated fields.  We provide this property rather
        /// than rely on the MySqlCommand.UpdatedRowSource property since a user should
        /// still be able to write a custom insert command and not have our work interfere.
        /// </summary>
        public bool ReturnGeneratedIdentifiers
        {
            get { return returnGeneratedIds; }
            set { returnGeneratedIds = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves parameter information from the stored procedure specified 
        /// in the MySqlCommand and populates the Parameters collection of the 
        /// specified MySqlCommand object.
        /// This method is not currently supported since stored procedures are 
        /// not available in MySql.
        /// </summary>
        /// <param name="command">The MySqlCommand referencing the stored 
        /// procedure from which the parameter information is to be derived. 
        /// The derived parameters are added to the Parameters collection of the 
        /// MySqlCommand.</param>
        /// <exception cref="InvalidOperationException">The command text is not 
        /// a valid stored procedure name.</exception>
        public static void DeriveParameters(MySqlCommand command)
        {
            if (!command.Connection.driver.Version.isAtLeast(5, 0, 0))
                throw new MySqlException("DeriveParameters is not supported on MySQL versions " +
                    "prior to 5.0");

            // retrieve the proc definitino from the cache.
            string spName = command.CommandText;
            if (spName.IndexOf(".") == -1)
                spName = command.Connection.Database + "." + spName;
            DataSet ds = command.Connection.ProcedureCache.GetProcedure(command.Connection, spName);

            DataTable parameters = ds.Tables["Procedure Parameters"];
            DataTable procTable = ds.Tables["Procedures"];
            command.Parameters.Clear();
            foreach (DataRow row in parameters.Rows)
            {
                MySqlParameter p = new MySqlParameter();
                p.ParameterName = String.Format("@{0}", row["PARAMETER_NAME"]);
                if (row["ORDINAL_POSITION"].Equals(0) && p.ParameterName == "@")
                    p.ParameterName = "@RETURN_VALUE";
                p.Direction = GetDirection(row);
                bool unsigned = StoredProcedure.GetFlags(row["DTD_IDENTIFIER"].ToString()).IndexOf("UNSIGNED") != -1;
                bool real_as_float = procTable.Rows[0]["SQL_MODE"].ToString().IndexOf("REAL_AS_FLOAT") != -1;
                p.MySqlDbType = MetaData.NameToType(row["DATA_TYPE"].ToString(),
                    unsigned, real_as_float, command.Connection);
                if (!row["CHARACTER_MAXIMUM_LENGTH"].Equals(DBNull.Value))
                    p.Size = (int)row["CHARACTER_MAXIMUM_LENGTH"];
                if (!row["NUMERIC_PRECISION"].Equals(DBNull.Value))
                    p.Precision = Convert.ToByte(row["NUMERIC_PRECISION"]);
                if (!row["NUMERIC_SCALE"].Equals(DBNull.Value))
                    p.Scale = Convert.ToByte(row["NUMERIC_SCALE"]);
                command.Parameters.Add(p);
            }
        }

        private static ParameterDirection GetDirection(DataRow row)
        {
            string mode = row["PARAMETER_MODE"].ToString();
            int ordinal = Convert.ToInt32(row["ORDINAL_POSITION"]);

            if (0 == ordinal)
                return ParameterDirection.ReturnValue;
            else if (mode == "IN")
                return ParameterDirection.Input;
            else if (mode == "OUT")
                return ParameterDirection.Output;
            return ParameterDirection.InputOutput;
        }

        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// <returns></returns>
        public new MySqlCommand GetDeleteCommand()
        {
            return (MySqlCommand)base.GetDeleteCommand();
        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <returns></returns>
        public new MySqlCommand GetUpdateCommand()
        {
            return (MySqlCommand)base.GetUpdateCommand();
        }

        /// <summary>
        /// Gets the insert command.
        /// </summary>
        /// <returns></returns>
        public new MySqlCommand GetInsertCommand()
        {
            return (MySqlCommand)GetInsertCommand(false);
        }

        /// <include file='docs/MySqlCommandBuilder.xml' path='docs/RefreshSchema/*'/>
        public override void RefreshSchema()
        {
            base.RefreshSchema();
            finalSelect = null;
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            if (unquotedIdentifier == null) throw new
                ArgumentNullException("unquotedIdentifier");

            // don't quote again if it is already quoted
            if (unquotedIdentifier.StartsWith(QuotePrefix) &&
                unquotedIdentifier.EndsWith(QuoteSuffix))
                return unquotedIdentifier;

            unquotedIdentifier = unquotedIdentifier.Replace(QuotePrefix, QuotePrefix + QuotePrefix);

            return String.Format("{0}{1}{2}", QuotePrefix, unquotedIdentifier, QuoteSuffix);
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            if (quotedIdentifier == null) throw new
                ArgumentNullException("quotedIdentifier");

            // don't unquote again if it is already unquoted
            if (!quotedIdentifier.StartsWith(QuotePrefix) ||
                !quotedIdentifier.EndsWith(QuoteSuffix))
                return quotedIdentifier;

            if (quotedIdentifier.StartsWith(QuotePrefix))
                quotedIdentifier = quotedIdentifier.Substring(1);
            if (quotedIdentifier.EndsWith(QuoteSuffix))
                quotedIdentifier = quotedIdentifier.Substring(0, quotedIdentifier.Length - 1);

            quotedIdentifier = quotedIdentifier.Replace(QuotePrefix + QuotePrefix, QuotePrefix);

            return quotedIdentifier;
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        protected override string GetParameterName(string parameterName)
        {
            StringBuilder sb = new StringBuilder(parameterName);
            sb.Replace(" ", "");
            sb.Replace("/", "_per_");
            sb.Replace("-", "_");
            sb.Replace(")", "_cb_");
            sb.Replace("(", "_ob_");
            sb.Replace("%", "_pct_");
            sb.Replace("<", "_lt_");
            sb.Replace(">", "_gt_");
            sb.Replace(".", "_pt_");
            return String.Format("@{0}", sb.ToString());
        }

        protected override DbCommand InitializeCommand(DbCommand command)
        {
            return base.InitializeCommand(command);
        }


        protected override void ApplyParameterInfo(DbParameter parameter, DataRow row,
            StatementType statementType, bool whereClause)
        {
            ((MySqlParameter)parameter).MySqlDbType = (MySqlDbType)row["ProviderType"];
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            return String.Format("@p{0}", parameterOrdinal.ToString(CultureInfo.InvariantCulture));
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            return String.Format("@p{0}", parameterOrdinal.ToString(CultureInfo.InvariantCulture));
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            MySqlDataAdapter myAdapter = (adapter as MySqlDataAdapter);
            if (adapter != base.DataAdapter)
                myAdapter.RowUpdating += new MySqlRowUpdatingEventHandler(RowUpdating);
            else
                myAdapter.RowUpdating -= new MySqlRowUpdatingEventHandler(RowUpdating);
        }

        private void RowUpdating(object sender, MySqlRowUpdatingEventArgs args)
        {
            base.RowUpdatingHandler(args);

            if (args.StatementType != StatementType.Insert) return;

            if (ReturnGeneratedIdentifiers)
            {
                if (args.Command.UpdatedRowSource != UpdateRowSource.None)
                    throw new InvalidOperationException(
                        Resources.MixingUpdatedRowSource);
                args.Command.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
                if (finalSelect == null)
                    CreateFinalSelect();
            }

            if (finalSelect != null && finalSelect.Length > 0)
                args.Command.CommandText += finalSelect;
        }

        /// <summary>
        /// We only need to return the single auto generated column since the base
        /// ADO.Net classes will take care of mapping it onto the datarow for us.
        /// </summary>
        private void CreateFinalSelect()
        {
            StringBuilder select = new StringBuilder();

            DataTable dt = GetSchemaTable(DataAdapter.SelectCommand);

            foreach (DataRow row in dt.Rows)
            {
                if (!(bool)row["IsAutoIncrement"])
                    continue;

                select.AppendFormat(CultureInfo.InvariantCulture, 
                    "; SELECT last_insert_id() AS `{0}`", row["ColumnName"]);
                break;
            }

            finalSelect = select.ToString();
        }
    }
}
