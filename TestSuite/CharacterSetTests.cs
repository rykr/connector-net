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
using System.IO;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace MySql.Data.MySqlClient.Tests
{
	[TestFixture]
	public class CharacterSetTests : BaseTest
	{
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			csAdditions = ";pooling=false";
			Open();
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			Close();
		}

		[Test]
		public void UseFunctions()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test ( valid char, UserCode varchar(100), password varchar(100) ) CHARSET latin1");

			MySqlConnection c = new MySqlConnection( conn.ConnectionString + ";charset=latin1" );
			c.Open();
			MySqlCommand cmd = new MySqlCommand("SELECT valid FROM Test WHERE Valid = 'Y' AND " +
				"UserCode = 'username' AND Password = AES_ENCRYPT('Password','abc')", c);
			cmd.ExecuteScalar();
			c.Close();
		}

		[Test]
		[Category("4.1")]
		public void VarBinary() 
		{
			execSQL("DROP TABLE IF EXISTS test");
			createTable("CREATE TABLE test (id int, name varchar(200) collate utf8_bin) charset utf8", "InnoDB");
			execSQL("INSERT INTO test VALUES (1, 'Test1')");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM test", conn);
			MySqlDataReader reader = null;
			try 
			{
				reader = cmd.ExecuteReader();
				Assert.IsTrue(reader.Read());
				object o = reader.GetValue(1);
				Assert.IsTrue(o is string);
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test]
		public void Latin1Connection() 
		{
			if (! Is41 && ! Is50) return;

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, name VARCHAR(200)) CHARSET latin1");
			execSQL("INSERT INTO Test VALUES( 1, _latin1 'Test')");

			MySqlConnection c = new MySqlConnection( conn.ConnectionString + ";charset=latin1" );
			c.Open();

			MySqlCommand cmd = new MySqlCommand("SELECT id FROM Test WHERE name LIKE 'Test'", c);
			object id = cmd.ExecuteScalar();
			Assert.AreEqual( 1, id );
			c.Close();
		}

		/// <summary>
		/// Bug #11621  	connector does not support charset cp1250
		/// </summary>
		[Test]
		[Category("NotWorking")]
		public void CP1250Connection() 
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (name VARCHAR(200)) CHARSET cp1250");

			MySqlConnection c = new MySqlConnection(conn.ConnectionString + ";charset=cp1250");
			c.Open();

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES('€ŤŽš')", c);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT name FROM Test";
			object name = cmd.ExecuteScalar();
			Assert.AreEqual("€ŤŽš", name);
			c.Close();
		}

        /// <summary>
        /// Bug #14592 Wrong column length returned for VARCHAR UTF8 columns 
        /// </summary>
        [Test]
        public void GetSchemaOnUTF8()
        {
            execSQL("DROP TABLE IF EXISTS test");
            execSQL("CREATE TABLE test(name VARCHAR(40) NOT NULL, name2 VARCHAR(20)) " +
                "CHARACTER SET utf8");
            execSQL("INSERT INTO test VALUES('Test', 'Test')");

            MySqlDataReader reader = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM test", conn);
                reader = cmd.ExecuteReader();
                DataTable dt = reader.GetSchemaTable();
                Assert.AreEqual(40, dt.Rows[0]["ColumnSize"]);
                Assert.AreEqual(20, dt.Rows[1]["ColumnSize"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

	}
}
