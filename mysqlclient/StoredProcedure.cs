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
using System.Data;
using System.Text;
using MySql.Data.Common;
using System.Collections;
using System.Globalization;
using System.Diagnostics;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for StoredProcedure.
	/// </summary>
	internal class StoredProcedure
	{
		private string hash;
		private MySqlConnection connection;
		private string outSelect;

		public StoredProcedure(MySqlConnection conn)
		{
			uint code = (uint)DateTime.Now.GetHashCode();
			hash = code.ToString();
			connection = conn;
		}

		private MySqlParameter GetReturnParameter(MySqlCommand cmd)
		{
			foreach (MySqlParameter p in cmd.Parameters)
				if (p.Direction == ParameterDirection.ReturnValue)
					return p;
			return null;
		}

		private string GetRoutineType(ref string spName)
		{
			int dotIndex = spName.IndexOf('.');
			string schema = spName.Substring(0, dotIndex);
			if (schema == null || schema == String.Empty)
				schema = "database()";
			else
				schema = String.Format("'{0}'", schema);
			string name = spName.Substring(dotIndex + 1);

			MySqlCommand cmd = new MySqlCommand(String.Format("SELECT ROUTINE_SCHEMA, ROUTINE_TYPE FROM " +
				 "INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA={0} AND " +
				 "ROUTINE_NAME='{1}'", schema, name), connection);
			MySqlDataReader reader = null;
			try
			{
				reader = cmd.ExecuteReader();
				reader.Read();
				object oSchema = reader.GetValue(0);
				if (schema == "database()")
					spName = oSchema.ToString() + "." + name;
				return reader.GetString(1);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

		private string GetProcedureBody(string spName, out string sql_mode, out bool isFunc)
		{
			sql_mode = String.Empty;

			string type = GetRoutineType(ref spName);
			if (type == null)
				throw new MySqlException("Procedure or function '" + spName + "' does not exist.");

			MySqlDataReader reader = null;
			try
			{
				MySqlCommand cmd = new MySqlCommand(String.Format("SHOW CREATE " +
					 "{0} {1}", type, spName), connection);
				isFunc = type.ToLower(CultureInfo.InvariantCulture) == "function";
				cmd.CommandText = String.Format("SHOW CREATE {0} {1}", type, spName);
				reader = cmd.ExecuteReader();
				reader.Read();
				sql_mode = reader.GetString(1);
				return reader.GetString(2);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}

		private string CleanParameterName(string parameter, bool stripMarker)
		{
			char c = parameter[0];
			if (c == '`' || c == '\'' || c == '"')
				parameter = parameter.Substring(1, parameter.Length - 2);
			if (stripMarker && parameter[0] == connection.ParameterMarker)
				parameter = parameter.Substring(1);

			return parameter;
		}

		private int FindRightParen(string body, string quotePattern)
		{
			int pos = 0;
			bool left = false;
			char quote = Char.MinValue;

			foreach (char c in body)
			{
				if (c == ')')
				{
					if (left)
						left = false;
					else if (quote == Char.MinValue)
						break;
				}
				else if (c == '(' && quote == Char.MinValue)
					left = true;
				else
				{
					int quoteIndex = quotePattern.IndexOf(c);
					if (quoteIndex > -1)
						if (quote == Char.MinValue)
							quote = c;
						else if (quote == c)
							quote = Char.MinValue;
				}
				pos++;
			}
			return pos;
		}

		private ArrayList ParseBody(string body, string sqlMode)
		{
			bool ansiQuotes = sqlMode.IndexOf("ANSI_QUOTES") != -1;
			bool noBackslash = sqlMode.IndexOf("NO_BACKSLASH_ESCAPES") != -1;
			string quotePattern = ansiQuotes ? "``\"\"" : "``";

			ContextString cs = new ContextString(quotePattern, !noBackslash);

			int leftParen = cs.IndexOf(body, '(');
			Debug.Assert(leftParen != -1);

			// trim off the first part
			body = body.Substring(leftParen + 1);

			int rightParen = FindRightParen(body, quotePattern);
			Debug.Assert(rightParen != -1);
			string parms = body.Substring(0, rightParen).Trim();

			cs.ContextMarkers += "()";
			ArrayList parmArray = new ArrayList();
			string[] paramDefs = cs.Split(parms, ",");
			if (paramDefs.Length > 0)
				foreach (string def in paramDefs)
					parmArray.Add(ParseParameter(def, cs, sqlMode));

			body = body.Substring(rightParen + 1).Trim().ToLower(CultureInfo.InvariantCulture);
			if (body.StartsWith("returns"))
				parmArray.Add(ParseParameter(body, cs, sqlMode));
			return parmArray;
		}

		private MySqlParameter ParseParameter(string parmDef, ContextString cs,
			string sqlMode)
		{
			MySqlParameter p = new MySqlParameter();

			parmDef = parmDef.Trim();
			string lowerDef = parmDef.ToLower(CultureInfo.InvariantCulture);

			if (lowerDef.StartsWith("inout "))
			{
				p.Direction = ParameterDirection.InputOutput;
				parmDef = parmDef.Substring(6);
			}
			else if (lowerDef.StartsWith("in "))
				parmDef = parmDef.Substring(3);
			else if (lowerDef.StartsWith("out "))
			{
				p.Direction = ParameterDirection.Output;
				parmDef = parmDef.Substring(4);
			}
			else if (lowerDef.StartsWith("returns "))
			{
				p.Direction = ParameterDirection.ReturnValue;
				parmDef = parmDef.Substring(8);
			}
			parmDef = parmDef.Trim();

			string[] split = cs.Split(parmDef, " \t\r\n");
            if (p.Direction != ParameterDirection.ReturnValue)
            {
                p.ParameterName = String.Format("{0}{1}", connection.ParameterMarker,
                    CleanParameterName(split[0], false));
                parmDef = parmDef.Substring(split[0].Length);
            }
            else
                p.ParameterName = String.Format("{0}RETURN_VALUE", connection.ParameterMarker);

			ParseType(parmDef, sqlMode, p);
			return p;
		}

		public ArrayList DiscoverParameters(string spName)
		{
			string sql_mode;
			bool isFunc;
			string body = GetProcedureBody(spName, out sql_mode, out isFunc);

			ArrayList parameters = ParseBody(body, sql_mode);

			return parameters;
		}

		private string StripParameterName(string name)
		{
			if (name[0] == connection.ParameterMarker)
				return name.Remove(0, 1);
			return name;
		}

		/// <summary>
		/// Creates the proper command text for executing the given stored procedure
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public string Prepare(MySqlCommand cmd)
		{
			MySqlParameter returnParameter = GetReturnParameter(cmd);

			try
			{
				ArrayList parameters = connection.ProcedureCache.GetProcedure(
				connection, cmd.CommandText);

				string sqlStr = String.Empty;
				string setStr = String.Empty;
				outSelect = String.Empty;

				foreach (MySqlParameter serverP in parameters)
				{
					if (serverP.Direction == ParameterDirection.ReturnValue)
						continue;
					int index = cmd.Parameters.IndexOf(serverP.ParameterName);
					if (index == -1)
						throw new MySqlException("Parameter '" + serverP.ParameterName + "' is not defined");

					MySqlParameter p = cmd.Parameters[index];
					if (!p.TypeHasBeenSet)
						p.MySqlDbType = serverP.MySqlDbType;
					string cleanName = StripParameterName(p.ParameterName);
					string pName = connection.ParameterMarker + cleanName;
					string vName = "@" + hash + cleanName;
					if (p.Direction == ParameterDirection.Input)
					{
						sqlStr += pName + ", ";
						continue;
					}
					else if (p.Direction == ParameterDirection.InputOutput)
						setStr += "set " + vName + "=" + pName + ";";
					sqlStr += vName + ", ";
					outSelect += vName + ", ";
				}

				if (returnParameter == null)
					sqlStr = "call " + cmd.CommandText + "(" + sqlStr;
				else
				{
					string cleanedName = CleanParameterName(returnParameter.ParameterName, true);
					string vname = "@" + hash + cleanedName;
					sqlStr = "set " + vname + "=" + cmd.CommandText + "(" + sqlStr;
					outSelect = vname + outSelect;
				}

				sqlStr = sqlStr.TrimEnd(' ', ',');
				outSelect = outSelect.TrimEnd(' ', ',');
				sqlStr += ")";
				if (setStr.Length > 0)
					sqlStr = setStr + sqlStr;
				return sqlStr;
			}
			catch (Exception ex)
			{
				throw new MySqlException("Exception during execution of '" + cmd.CommandText + "': " + ex.Message, ex);
			}
		}

		public void UpdateParameters(MySqlParameterCollection parameters)
		{
			if (outSelect.Length == 0) return;

			char marker = connection.ParameterMarker;

			MySqlCommand cmd = new MySqlCommand("SELECT " + outSelect, connection);
			MySqlDataReader reader = cmd.ExecuteReader();

			for (int i = 0; i < reader.FieldCount; i++)
			{
				string fieldName = reader.GetName(i);
				fieldName = marker + fieldName.Remove(0, hash.Length + 1);
				reader.CurrentResult[i] = parameters[fieldName].GetValueObject();
			}

			reader.Read();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				string fieldName = reader.GetName(i);
				fieldName = marker + fieldName.Remove(0, hash.Length + 1);
				parameters[fieldName].Value = reader.GetValue(i);
			}
			reader.Close();
		}

		private void ParseType(string type, string sql_mode, MySqlParameter p)
		{
			string typeName, flags = String.Empty, size;
			int end;

			type = type.ToLower(CultureInfo.InvariantCulture).Trim();
			int start = type.IndexOf("(");
			if (start != -1)
				end = type.IndexOf(')', start + 1);
			else
				end = start = type.IndexOf(' ');
			if (start == -1)
				start = type.Length;

			typeName = type.Substring(0, start);
			if (end != -1)
				flags = type.Substring(end + 1);
			bool unsigned = flags.IndexOf("unsigned") != -1;
			bool real_as_float = sql_mode.IndexOf("REAL_AS_FLOAT") != -1;

			p.MySqlDbType = GetTypeFromName(typeName, unsigned, real_as_float);

			if (end > start && p.MySqlDbType != MySqlDbType.Set && p.MySqlDbType != MySqlDbType.Enum)
			{
				size = type.Substring(start + 1, end - (start + 1));
				string[] parts = size.Split(new char[] { ',' });
				p.Size = Int32.Parse(parts[0]);
				if (p.MySqlDbType == MySqlDbType.Decimal)
				{
					p.Precision = (byte)p.Size;
					p.Size = 0;
					if (parts.Length > 1)
						p.Scale = Byte.Parse(parts[1]);
				}
			}
		}

		private MySqlDbType GetTypeFromName(string typeName, bool unsigned, bool realAsFloat)
		{
			switch (typeName)
			{
				case "char": return MySqlDbType.Char;
				case "varchar": return MySqlDbType.VarChar;
				case "date": return MySqlDbType.Date;
				case "datetime": return MySqlDbType.Datetime;
				case "numeric":
				case "decimal":
				case "dec":
				case "fixed":
					if (connection.driver.Version.isAtLeast(5, 0, 3))
						return MySqlDbType.NewDecimal;
					else
						return MySqlDbType.Decimal;
				case "year":
					return MySqlDbType.Year;
				case "time":
					return MySqlDbType.Time;
				case "timestamp":
					return MySqlDbType.Timestamp;
				case "set": return MySqlDbType.Set;
				case "enum": return MySqlDbType.Enum;
				case "bit": return MySqlDbType.Bit;

				case "tinyint":
				case "bool":
				case "boolean":
					return MySqlDbType.Byte;
				case "smallint":
					return unsigned ? MySqlDbType.UInt16 : MySqlDbType.Int16;
				case "mediumint":
					return unsigned ? MySqlDbType.UInt24 : MySqlDbType.Int24;
				case "int":
				case "integer":
					return unsigned ? MySqlDbType.UInt32 : MySqlDbType.Int32;
				case "serial":
					return MySqlDbType.UInt64;
				case "bigint":
					return unsigned ? MySqlDbType.UInt64 : MySqlDbType.Int64;
				case "float": return MySqlDbType.Float;
				case "double": return MySqlDbType.Double;
				case "real": return
					 realAsFloat ? MySqlDbType.Float : MySqlDbType.Double;
				case "blob":
				case "text":
					return MySqlDbType.Blob;
				case "longblob":
				case "longtext":
					return MySqlDbType.LongBlob;
				case "mediumblob":
				case "mediumtext":
					return MySqlDbType.MediumBlob;
				case "tinyblob":
				case "tinytext":
					return MySqlDbType.TinyBlob;
			}
			throw new MySqlException("Unhandled type encountered");
		}

	}
}
