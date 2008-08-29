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

using NUnit.Framework;
using MySql.Web.Security;
using System.Collections.Specialized;
using MySql.Data.MySqlClient;
using System.Resources;
using System.Data;
using System;
using System.IO;
using System.Configuration.Provider;
using System.Web.Security;

namespace MySql.Web.Tests
{
    [TestFixture]
    public class SchemaTests : BaseWebTest
    {
        [SetUp]
        public override void Setup()
        {
			base.Setup();

            DataTable dt = conn.GetSchema("Tables");
            foreach (DataRow row in dt.Rows)
                execSQL(String.Format("DROP TABLE IF EXISTS {0}", row["TABLE_NAME"]));
        }

        /// <summary>
        /// Bug #37469 autogenerateschema optimizing
        /// </summary>
        [Test]
        public void SchemaNotPresent()
        {
            MySQLMembershipProvider provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            config.Add("passwordFormat", "Clear");

            try
            {
                provider.Initialize(null, config);
                Assert.Fail("Should have failed");
            }
            catch (ProviderException)
            {
            }
        }

        [Test]
        public void SchemaV1Present()
        {
            MySQLMembershipProvider provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            config.Add("passwordFormat", "Clear");

            LoadSchema(1);
            try
            {
                provider.Initialize(null, config);
                Assert.Fail("Should have failed");
            }
            catch (ProviderException)
            {
            }
        }

        [Test]
        public void SchemaV2Present()
        {
            MySQLMembershipProvider provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            config.Add("passwordFormat", "Clear");

            LoadSchema(1);
            LoadSchema(2);
            try
            {
                provider.Initialize(null, config);
                Assert.Fail("Should have failed");
            }
            catch (ProviderException)
            {
            }
        }

        [Test]
        public void SchemaV3Present()
        {
            MySQLMembershipProvider provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            config.Add("passwordFormat", "Clear");

            LoadSchema(1);
            LoadSchema(2);
            LoadSchema(3);
            try
            {
                provider.Initialize(null, config);
                Assert.Fail("Should have failed");
            }
            catch (ProviderException)
            {
            }
        }

        [Test]
        public void SchemaV4Present()
        {
            MySQLMembershipProvider provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("applicationName", "/");
            config.Add("passwordFormat", "Clear");

            LoadSchema(1);
            LoadSchema(2);
            LoadSchema(3);
            LoadSchema(4);
            try
            {
                provider.Initialize(null, config);
            }
            catch (ProviderException)
            {
                Assert.Fail("Should not have failed");
            }
        }

        /// <summary>
        /// Bug #36444 'autogenerateschema' produces tables with 'random' collations 
        /// </summary>
        [Test]
        public void CurrentSchema()
        {
            execSQL("set character_set_database=utf8");

            LoadSchema(1);
            LoadSchema(2);
            LoadSchema(3);
            LoadSchema(4);

            MySqlCommand cmd = new MySqlCommand("SELECT * FROM my_aspnet_SchemaVersion", conn);
            object ver = cmd.ExecuteScalar();
            Assert.AreEqual(4, ver);

            cmd.CommandText = "SHOW CREATE TABLE my_aspnet_membership";
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                string createSql = reader.GetString(1);
                Assert.IsTrue(createSql.IndexOf("CHARSET=utf8") != -1);
            }
        }

        [Test]
        public void UpgradeV1ToV2()
        {
            LoadSchema(1);

            MySqlCommand cmd = new MySqlCommand("SHOW CREATE TABLE mysql_membership", conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                string createTable = reader.GetString(1);
                int index = createTable.IndexOf("COMMENT='1'");
                Assert.AreNotEqual(-1, index);
            }

            LoadSchema(2);
            cmd = new MySqlCommand("SHOW CREATE TABLE mysql_membership", conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                string createTable = reader.GetString(1);
                int index = createTable.IndexOf("COMMENT='2'");
                Assert.AreNotEqual(-1, index);
            }
        }

        private void LoadData()
        {
            LoadSchema(1);
            LoadSchema(2);
            execSQL(@"INSERT INTO mysql_membership (pkid, username, applicationname, lastactivitydate) 
                VALUES('1', 'user1', 'app1', '2007-01-01')");
            execSQL(@"INSERT INTO mysql_membership (pkid, username, applicationname, lastactivitydate) 
                VALUES('2', 'user2', 'app1', '2007-01-01')");
            execSQL(@"INSERT INTO mysql_membership (pkid, username, applicationname, lastactivitydate) 
                VALUES('3', 'user1', 'app2', '2007-01-01')");
            execSQL(@"INSERT INTO mysql_membership (pkid, username, applicationname, lastactivitydate) 
                VALUES('4', 'user2', 'app2', '2007-01-01')");
            execSQL(@"INSERT INTO mysql_roles VALUES ('role1', 'app1')");
            execSQL(@"INSERT INTO mysql_roles VALUES ('role2', 'app1')");
            execSQL(@"INSERT INTO mysql_roles VALUES ('role1', 'app2')");
            execSQL(@"INSERT INTO mysql_roles VALUES ('role2', 'app2')");
            execSQL(@"INSERT INTO mysql_UsersInRoles VALUES ('user1', 'role1', 'app1')");
            execSQL(@"INSERT INTO mysql_UsersInRoles VALUES ('user2', 'role2', 'app1')");
            execSQL(@"INSERT INTO mysql_UsersInRoles VALUES ('user1', 'role1', 'app2')");
            execSQL(@"INSERT INTO mysql_UsersInRoles VALUES ('user2', 'role2', 'app2')");
            LoadSchema(3);
            Assert.IsFalse(TableExists("mysql_membership"));
            Assert.IsFalse(TableExists("mysql_roles"));
            Assert.IsFalse(TableExists("mysql_usersinroles"));
        }

        [Test]
        public void CheckAppsUpgrade()
        {
            LoadData();

            DataTable apps = FillTable("SELECT * FROM my_aspnet_Applications");
            Assert.AreEqual(2, apps.Rows.Count);
            Assert.AreEqual(1, apps.Rows[0]["id"]);
            Assert.AreEqual("app1", apps.Rows[0]["name"]);
            Assert.AreEqual(2, apps.Rows[1]["id"]);
            Assert.AreEqual("app2", apps.Rows[1]["name"]);
        }

        [Test]
        public void CheckUsersUpgrade()
        {
            LoadData();

            DataTable dt = FillTable("SELECT * FROM my_aspnet_Users");
            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0]["id"]);
            Assert.AreEqual(1, dt.Rows[0]["applicationId"]);
            Assert.AreEqual("user1", dt.Rows[0]["name"]);
            Assert.AreEqual(2, dt.Rows[1]["id"]);
            Assert.AreEqual(1, dt.Rows[1]["applicationId"]);
            Assert.AreEqual("user2", dt.Rows[1]["name"]);
            Assert.AreEqual(3, dt.Rows[2]["id"]);
            Assert.AreEqual(2, dt.Rows[2]["applicationId"]);
            Assert.AreEqual("user1", dt.Rows[2]["name"]);
            Assert.AreEqual(4, dt.Rows[3]["id"]);
            Assert.AreEqual(2, dt.Rows[3]["applicationId"]);
            Assert.AreEqual("user2", dt.Rows[3]["name"]);
        }
           
        [Test]
        public void CheckRolesUpgrade()
        {
            LoadData();

            DataTable dt = FillTable("SELECT * FROM my_aspnet_Roles");
            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0]["id"]);
            Assert.AreEqual(1, dt.Rows[0]["applicationId"]);
            Assert.AreEqual("role1", dt.Rows[0]["name"]);
            Assert.AreEqual(2, dt.Rows[1]["id"]);
            Assert.AreEqual(1, dt.Rows[1]["applicationId"]);
            Assert.AreEqual("role2", dt.Rows[1]["name"]);
            Assert.AreEqual(3, dt.Rows[2]["id"]);
            Assert.AreEqual(2, dt.Rows[2]["applicationId"]);
            Assert.AreEqual("role1", dt.Rows[2]["name"]);
            Assert.AreEqual(4, dt.Rows[3]["id"]);
            Assert.AreEqual(2, dt.Rows[3]["applicationId"]);
            Assert.AreEqual("role2", dt.Rows[3]["name"]);
        }

        [Test]
        public void CheckMembershipUpgrade()
        {
            LoadData();

            DataTable dt = FillTable("SELECT * FROM my_aspnet_Membership");
            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0]["userid"]);
            Assert.AreEqual(2, dt.Rows[1]["userid"]);
            Assert.AreEqual(3, dt.Rows[2]["userid"]);
            Assert.AreEqual(4, dt.Rows[3]["userid"]);
        }

        [Test]
        public void CheckUsersInRolesUpgrade()
        {
            LoadData();

            DataTable dt = FillTable("SELECT * FROM my_aspnet_UsersInRoles");
            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0]["userid"]);
            Assert.AreEqual(1, dt.Rows[0]["roleid"]);
            Assert.AreEqual(2, dt.Rows[1]["userid"]);
            Assert.AreEqual(2, dt.Rows[1]["roleid"]);
            Assert.AreEqual(3, dt.Rows[2]["userid"]);
            Assert.AreEqual(3, dt.Rows[2]["roleid"]);
            Assert.AreEqual(4, dt.Rows[3]["userid"]);
            Assert.AreEqual(4, dt.Rows[3]["roleid"]);
        }

        /// <summary>
        /// Bug #39072 Web provider does not work
        /// </summary>
        [Test]
        public void AutoGenerateSchema()
        {
            MySQLMembershipProvider provider = new MySQLMembershipProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("connectionStringName", "LocalMySqlServer");
            config.Add("autogenerateschema", "true");
            config.Add("applicationName", "/");
            config.Add("passwordFormat", "Clear");

            provider.Initialize(null, config);

            MembershipCreateStatus status;
            MembershipUser user = provider.CreateUser("boo", "password", "email@email.com", 
                "question", "answer", true, null, out status);
        }
    }
}
