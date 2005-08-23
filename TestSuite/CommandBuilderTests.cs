// Copyright (C) 2004-2005 MySQL AB
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
	[TestFixture()]
	public class CommandBuilderTest : BaseTest
	{
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			Open();

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(100), dt DATETIME, tm TIME,  `multi word` int, PRIMARY KEY(id))");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown() 
		{
			Close();
		}

		[Test()]
		public void MultiWord()
		{
			MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			DataTable dt = new DataTable();
			da.Fill(dt);

			DataRow row = dt.NewRow();
			row["id"] = 1;
			row["name"] = "Name";
			row["dt"] = DBNull.Value;
			row["tm"] = DBNull.Value;
			row["multi word"] = 2;
			dt.Rows.Add( row );
			da.Update( dt );
			Assert.AreEqual( 1, dt.Rows.Count );
			Assert.AreEqual( 2, dt.Rows[0]["multi word"] );

			dt.Rows[0]["multi word"] = 3;
			da.Update( dt );
			Assert.AreEqual( 1, dt.Rows.Count );
			Assert.AreEqual( 3, dt.Rows[0]["multi word"] );
		}

		[Test()]
		public void LastOneWins() 
		{
			execSQL("INSERT INTO Test (id, name) VALUES (1, 'Test')");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da, true);
			DataTable dt = new DataTable();
			da.Fill( dt );
			Assert.AreEqual( 1, dt.Rows.Count );

			execSQL("UPDATE Test SET name='Test2' WHERE id=1");

			dt.Rows[0]["name"] = "Test3";
			Assert.AreEqual( 1, da.Update( dt ) );

			dt.Rows.Clear();
			da.Fill( dt );
			Assert.AreEqual( 1, dt.Rows.Count );
			Assert.AreEqual( "Test3", dt.Rows[0]["name"] );			
		}

		[Test()]
		public void NotLastOneWins() 
		{
			execSQL("INSERT INTO Test (id, name) VALUES (1, 'Test')");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			DataTable dt = new DataTable();
			da.Fill(dt);
			Assert.AreEqual(1, dt.Rows.Count);

			execSQL("UPDATE Test SET name='Test2' WHERE id=1");

			try 
			{
				dt.Rows[0]["name"] = "Test3";
				int cnt = da.Update( dt );
				Assert.Fail("This should not work");
			}
			catch (DBConcurrencyException) 
			{
			}

			dt.Rows.Clear();
			da.Fill( dt );
			Assert.AreEqual( 1, dt.Rows.Count );
			Assert.AreEqual( "Test2", dt.Rows[0]["name"] );			
		}

		/// <summary>
		/// Bug #8574 - MySqlCommandBuilder unable to support sub-queries
		/// </summary>
		[Test]
		public void UsingFunctions() 
		{
			execSQL("INSERT INTO test (id, name) VALUES (1,'test1')");
			execSQL("INSERT INTO test (id, name) VALUES (2,'test2')");
			execSQL("INSERT INTO test (id, name) VALUES (3,'test3')");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT id, name, now() as ServerTime FROM test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);

			ds.Tables[0].Rows[0]["id"] = 4;
			DataSet changes = ds.GetChanges();
			da.Update(changes);
			ds.Merge( changes );
			ds.AcceptChanges();
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

			conn.ChangeDatabase("mysql");

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT id, name FROM test.test", conn);
			MySqlCommandBuilder cb = new MySqlCommandBuilder(da);
			DataSet ds = new DataSet();
			da.Fill(ds);

			ds.Tables[0].Rows[0]["id"] = 4;
			DataSet changes = ds.GetChanges();
			da.Update(changes);
			ds.Merge( changes );
			ds.AcceptChanges();
			
			conn.ChangeDatabase("test");
		}

	}
}
