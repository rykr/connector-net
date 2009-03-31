﻿using Microsoft.VisualStudio.Package;
using System.Collections.Generic;

namespace MySql.Data.VisualStudio
{
    /// <summary>
    /// We don't actually need this class but we have to return a scanner object
    /// from our language service or the colorizing doesn't work.
    /// All the colorizing work happens in MySqlColorizer.
    /// </summary>
    class MySqlScanner : IScanner
    {
        List<string> keywords;
        Tokenizer tokenizer = new Tokenizer();
        List<string> lines = new List<string>();

        public MySqlScanner()
        {
            tokenizer.ReturnComments = true;
            Initialize();
        }

        #region IScanner Members

        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            tokenizer.BlockComment = state == 1 ? true : false;

            string token = tokenizer.NextToken();
            if (token == null) return false;

            tokenInfo.StartIndex = tokenizer.StartIndex;
            tokenInfo.EndIndex = tokenizer.StopIndex;
            tokenInfo.Type = GetTokenType(token);
            tokenInfo.Color = GetTokenColor(tokenInfo);
            if (state == 1 && token == "*/")
                state = 0;
            else
                state = tokenizer.BlockComment ? 1 : 0;
            return true;
        }

        void IScanner.SetSource(string source, int offset)
        {
            tokenizer.Text = source;
        }

        #endregion

        #region Private methods

        private TokenColor GetTokenColor(TokenInfo ti)
        {
            switch (ti.Type)
            {
                case TokenType.Comment:
                case TokenType.LineComment:
                    return TokenColor.Comment;
                case TokenType.Literal:
                    return TokenColor.String;
                case TokenType.Keyword:
                    return TokenColor.Keyword;
                default:
                    return TokenColor.Text;
            }
        }

        private TokenType GetTokenType(string token)
        {
            if (tokenizer.LineComment) return TokenType.Comment;
            else if (tokenizer.BlockComment) return TokenType.Comment;
            else if (IsKeyword(token)) return TokenType.Keyword;
            else if (tokenizer.Quoted) return TokenType.Literal;
            return TokenType.Text;
        }

        private bool IsKeyword(string token)
        {
            return keywords.Contains(token.ToUpperInvariant());
        }

        private void Initialize()
        {
            if (keywords != null) return;
            keywords = new List<string>();

            // procedures and functions
            keywords.Add("CREATE");
            keywords.Add("ALTER");
            keywords.Add("PROCEDURE");
            keywords.Add("CALL");
            keywords.Add("RETURN");
            keywords.Add("FUNCTION");
            keywords.Add("RETURNS");
            keywords.Add("DECLARE");
            keywords.Add("DEFINER");
            keywords.Add("CURRENT_USER");
            keywords.Add("OUT");
            keywords.Add("INOUT");
            keywords.Add("IN");
            keywords.Add("BEGIN");
            keywords.Add("END");
            keywords.Add("VIEW");
            keywords.Add("AS");

            // update
            keywords.Add("UPDATE");
            keywords.Add("TABLE");

            // delete
            keywords.Add("DELETE");

            // select 
            keywords.Add("SELECT");
            keywords.Add("FROM");
            keywords.Add("WHERE");
            keywords.Add("GROUP");
            keywords.Add("BY");
            keywords.Add("ASC");
            keywords.Add("DESC");
            keywords.Add("WITH");
            keywords.Add("ROLLUP");
            keywords.Add("HAVING");
            keywords.Add("ORDER");
            keywords.Add("LIMIT");
            keywords.Add("OFFSET");
            keywords.Add("INTO");
            keywords.Add("OUTFILE");
            keywords.Add("DUMPFILE");
            keywords.Add("FOR");
            keywords.Add("LOCK");
            keywords.Add("SHARE");
            keywords.Add("MODE");
            keywords.Add("ALL");
            keywords.Add("DISTINCT");
            keywords.Add("DISTINCTROW");
            keywords.Add("HIGH_PRIORITY");
            keywords.Add("STRAIGHT_JOIN");
            keywords.Add("SQL_SMALL_RESULT");
            keywords.Add("SQL_BIG_RESULT");
            keywords.Add("SQL_BUFFER_RESULT");
            keywords.Add("SQL_CACHE");
            keywords.Add("SQL_NO_CACHE");
            keywords.Add("SQL_CALC_FOUND_ROWS");

            // misc
            keywords.Add("SHOW");
            keywords.Add("PROCESSLIST");
            keywords.Add("KILL");
            keywords.Add("STATUS");

            // data types
            keywords.Add("INT");
            keywords.Add("CHAR");
            keywords.Add("VARCHAR");

            // functions
            keywords.Add("COUNT");
            keywords.Add("REPLACE");
        }

        #endregion
    }
}