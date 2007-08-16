// Copyright (C) 2004-2007 MySQL AB
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
using NUnit.Framework;

namespace MySql.Data.MySqlClient.Tests
{
	[TestFixture]
	public class CommandBuilderTests : BaseTest
	{
		[TestFixtureSetUp]
		protected override void FixtureSetup()
		{
            csAdditions += ";logging=true;";
			base.FixtureSetup();
		}

		protected override void Setup()
		{
			base.Setup();

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(100), dt DATETIME, tm TIME,  `multi word` int, PRIMARY KEY(id))");
		}

		[Test]
		public void MultiWord()
		{
			try
			{
				MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Test", conn);
				MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
				cb.ToString();  // keep mono happy
				DataTable dt = new DataTable();
				da.Fill(dt);

				DataRow row = dt.NewRow();
				row["id"] = 1;
				row["name"] = "Name";
				row["dt"] = DBNull.Value;
				row["tm"] = DBNull.Value;
				row["multi word"] = 2;
				dt.Rows.Add(row);
				da.Update(dt);
				Assert.AreEqual(1, dt.Rows.Count);
				Assert.AreEqual(2, dt.Rows[0]["multi word"]);

				dt.Rows[0]["multi word"] = 3;
				da.Update(dt);
				Assert.AreEqual(1, dt.Rows.Count);
				Assert.AreEqual(3, dt.Rows[0]["multi word"]);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void LastOneWins()
		{
			execSQL("INSERT INTO Test (id, name) VALUES (1, 'Test')");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da, true);
			cb.ToString();

			DataTable dt = new DataTable();
			da.Fill(dt);
			Assert.AreEqual(1, dt.Rows.Count);

			execSQL("UPDATE Test SET name='Test2' WHERE id=1");

			dt.Rows[0]["name"] = "Test3";
			Assert.AreEqual(1, da.Update(dt));

			dt.Rows.Clear();
			da.Fill(dt);
			Assert.AreEqual(1, dt.Rows.Count);
			Assert.AreEqual("Test3", dt.Rows[0]["name"]);
		}

		[Test]
		public void NotLastOneWins()
		{
			execSQL("INSERT INTO Test (id, name) VALUES (1, 'Test')");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			cb.ToString();  // keep the compiler happy
			DataTable dt = new DataTable();
			da.Fill(dt);
			Assert.AreEqual(1, dt.Rows.Count);

			execSQL("UPDATE Test SET name='Test2' WHERE id=1");

			try
			{
				dt.Rows[0]["name"] = "Test3";
				da.Update(dt);
				Assert.Fail("This should not work");
			}
			catch (DBConcurrencyException)
			{
			}

			dt.Rows.Clear();
			da.Fill(dt);
			Assert.AreEqual(1, dt.Rows.Count);
			Assert.AreEqual("Test2", dt.Rows[0]["name"]);
		}

		/// <summary>
		/// Bug #8574 - MySqlCommandBuilder unable to support sub-queries
		/// Bug #11947 - MySQLCommandBuilder mishandling CONCAT() aliased column
		/// </summary>
		[Test]
		public void UsingFunctions()
		{
			execSQL("INSERT INTO test (id, name) VALUES (1,'test1')");
			execSQL("INSERT INTO test (id, name) VALUES (2,'test2')");
			execSQL("INSERT INTO test (id, name) VALUES (3,'test3')");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT id, name, now() as ServerTime FROM test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			DataTable dt = new DataTable();
			da.Fill(dt);

			dt.Rows[0]["id"] = 4;
			da.Update(dt);

			da.SelectCommand.CommandText = "SELECT id, name, CONCAT(name, '  boo') as newname from test where id=4";
			dt.Clear();
			da.Fill(dt);
			Assert.AreEqual(1, dt.Rows.Count);
			Assert.AreEqual("test1", dt.Rows[0]["name"]);
			Assert.AreEqual("test1  boo", dt.Rows[0]["newname"]);

			dt.Rows[0]["id"] = 5;
			da.Update(dt);

			dt.Clear();
			da.SelectCommand.CommandText = "SELECT * FROM test WHERE id=5";
			da.Fill(dt);
			Assert.AreEqual(1, dt.Rows.Count);
			Assert.AreEqual("test1", dt.Rows[0]["name"]);

			da.SelectCommand.CommandText = "SELECT *, now() as stime FROM test WHERE id<4";
			cb = new MySqlCommandBuilder(da, true);
			da.InsertCommand = cb.GetInsertCommand();
		}

		/// <summary>
		/// Bug #8382  	Commandbuilder does not handle queries to other databases than the default one-
		/// </summary>
		[Test]
		[Category("4.1")]
		public void DifferentDatabase()
		{
			execSQL("INSERT INTO test (id, name) VALUES (1,'test1')");
			execSQL("INSERT INTO test (id, name) VALUES (2,'test2')");
			execSQL("INSERT INTO test (id, name) VALUES (3,'test3')");

            conn.ChangeDatabase(database1);

			MySqlDataAdapter da = new MySqlDataAdapter(
                String.Format("SELECT id, name FROM `{0}`.test", database0), conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			cb.ToString();
			DataSet ds = new DataSet();
			da.Fill(ds);

			ds.Tables[0].Rows[0]["id"] = 4;
			DataSet changes = ds.GetChanges();
			da.Update(changes);
			ds.Merge(changes);
			ds.AcceptChanges();

			conn.ChangeDatabase(database0);
		}

		/// <summary>
		/// Bug #13036  	Returns error when field names contain any of the following chars %<>()/ etc
		/// </summary>
		[Test]
		public void SpecialCharactersInFieldNames()
		{
			execSQL("DROP TABLE IF EXISTS test");
			execSQL("CREATE TABLE test (`col%1` int PRIMARY KEY, `col()2` int, `col<>3` int, `col/4` int)");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			cb.ToString();  // keep the compiler happy
			DataTable dt = new DataTable();
			da.Fill(dt);
			DataRow row = dt.NewRow();
			row[0] = 1;
			row[1] = 2;
			row[2] = 3;
			row[3] = 4;
			dt.Rows.Add(row);
			da.Update(dt);
		}

		/// <summary>
		/// Bug #14631  	"#42000Query was empty"
		/// </summary>
		[Test]
		public void SemicolonAtEndOfSQL()
		{
			execSQL("DROP TABLE IF EXISTS test");
			execSQL("CREATE TABLE test (id INT NOT NULL, name VARCHAR(100), PRIMARY KEY(id))");
			execSQL("INSERT INTO test VALUES(1, 'Data')");

			try
			{
				DataSet ds = new DataSet();
				MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM `test`;", conn);
				da.FillSchema(ds, SchemaType.Source, "test");

				MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
				cb.ToString();
				DataTable dt = new DataTable();
				da.Fill(dt);
				dt.Rows[0]["id"] = 2;
				da.Update(dt);

				dt.Clear();
				da.Fill(dt);
				Assert.AreEqual(1, dt.Rows.Count);
				Assert.AreEqual(2, dt.Rows[0]["id"]);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

        /// <summary>
        /// Bug #23862 Problem with CommandBuilder 'GetInsertCommand' method 
        /// </summary>
        [Test]
        public void AutoIncrementColumnsOnInsert()
        {
            execSQL("DROP TABLE IF EXISTS test");
            execSQL("CREATE TABLE test (id INT UNSIGNED NOT NULL AUTO_INCREMENT, " +
                "name VARCHAR(100), PRIMARY KEY(id))");
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM test", conn);
            MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			cb.ToString();

            DataTable dt = new DataTable();
            da.Fill(dt);
            dt.Columns[0].AutoIncrement = true;
            //Assert.IsTrue(dt.Columns[0].AutoIncrement);
            dt.Columns[0].AutoIncrementSeed = -1;
            dt.Columns[0].AutoIncrementStep = -1;
            DataRow row = dt.NewRow();
            row["name"] = "Test";

            try
            {
                dt.Rows.Add(row);
                da.Update(dt);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            dt.Clear();
            da.Fill(dt);
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0]["id"]);
            Assert.AreEqual("Test", dt.Rows[0]["name"]);
        }
	}
}
