// Copyright (C) 2007 MySQL AB
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

//  This code was contributed by Sean Wright (srwright@alcor.concordia.ca) on 2007-01-12
//  The copyright was assigned and transferred under the terms of
//  the MySQL Contributor License Agreement (CLA)

using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Resources;
using System.IO;
using System.Diagnostics;
using MySql.Web.Properties;

namespace MySql.Web.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class SchemaManager
    {
        private const int schemaVersion = 4;

        /// <summary>
        /// Gets the most recent version of the schema.
        /// </summary>
        /// <value>The most recent version number of the schema.</value>
        public static int Version
        {
            get { return schemaVersion; }
        }

        internal static void CheckSchema(string connectionString, NameValueCollection config)
        {
            try
            {
                int ver = GetSchemaVersion(connectionString);
                if (ver == Version) return;

                if (config["autogenerateschema"] == "true")
                    UpgradeToCurrent(connectionString, ver);
                else
                    throw new ProviderException(Resources.MissingOrWrongSchema);

            }
            catch (Exception ex)
            {
                throw new ProviderException(Resources.MissingOrWrongSchema, ex);
            }
        }

        private static void UpgradeToCurrent(string connectionString, int version)
        {
            ResourceManager r = new ResourceManager("MySql.Web.Properties.Resources", 
                typeof(SchemaManager).Assembly);

            if (version == Version) return;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                for (int ver = version + 1; ver <= Version; ver++)
                {
                    string schema = r.GetString(String.Format("schema{0}", ver));
                    MySqlScript script = new MySqlScript(connection);
                    script.Query = schema;
                    script.Execute();
                }
            }
        }

        private static int GetSchemaVersion(string connectionString)
        {
            // retrieve the current schema version
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM my_aspnet_SchemaVersion", conn);
                try
                {
                    object ver = cmd.ExecuteScalar();
                    if (ver != null)
                        return (int)ver;
                }
                catch (MySqlException ex)
                {
                    if (ex.Number != (int)MySqlErrorCode.NoSuchTable)
                        throw;
                    string[] restrictions = new string[4];
                    restrictions[2] = "mysql_Membership";
                    DataTable dt = conn.GetSchema("Tables", restrictions);
                    if (dt.Rows.Count == 1)
                        return Convert.ToInt32(dt.Rows[0]["TABLE_COMMENT"]);
                }
                return 0;
            }
        }

        /// <summary>
        /// Creates the or fetch user id.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="username">The username.</param>
        /// <param name="applicationId">The application id.</param>
        /// <param name="authenticated">if set to <c>true</c> [authenticated].</param>
        /// <returns></returns>
        internal static int CreateOrFetchUserId(MySqlConnection connection, string username, 
            int applicationId, bool authenticated)
        {
            Debug.Assert(applicationId > 0);

            // first attempt to fetch an existing user id
            MySqlCommand cmd = new MySqlCommand(@"SELECT id FROM my_aspnet_Users
                WHERE applicationId = @appId AND name = @name", connection);
            cmd.Parameters.AddWithValue("@appId", applicationId);
            cmd.Parameters.AddWithValue("@name", username);
            object userId = cmd.ExecuteScalar();
            if (userId != null) return (int)userId;

            cmd.CommandText = @"INSERT INTO my_aspnet_Users VALUES 
                (NULL, @appId, @name, @isAnon, Now())";
            cmd.Parameters.AddWithValue("@isAnon", !authenticated);
            int recordsAffected = cmd.ExecuteNonQuery();
            if (recordsAffected != 1)
                throw new ProviderException(Resources.UnableToCreateUser);

            cmd.CommandText = "SELECT LAST_INSERT_ID()";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}