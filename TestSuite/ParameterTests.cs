// Copyright (C) 2004 MySQL AB
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
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace MySql.Data.MySqlClient.Tests
{
	/// <summary>
	/// Summary description for ConnectionTests.
	/// </summary>
	[TestFixture] 
	public class ParameterTests : BaseTest
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			Open();
			execSQL("DROP TABLE IF EXISTS Test; CREATE TABLE Test (id INT NOT NULL, name VARCHAR(100), dt DATETIME, tm TIME, ts TIMESTAMP, PRIMARY KEY(id))");
		}

		[TestFixtureTearDown]
		public void TearDown() 
		{
			Close();
		}

		[Test()]
		public void TestUserVariables()
		{
			MySqlCommand cmd = new MySqlCommand("SET @myvar = 'test'", conn);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT @myvar";
			MySqlDataReader reader = cmd.ExecuteReader();
			try 
			{
				Assert.AreEqual( true, reader.Read());
				Assert.AreEqual( "test", reader.GetValue(0));
			}
			catch (Exception ex)
			{
				Assert.Fail( ex.Message );
			}
			finally 
			{
				reader.Close();
			}
		}

		[Test()]
		public void TestQuoting()
		{
			MySqlCommand cmd = new MySqlCommand("", conn);
			cmd.CommandText = "INSERT INTO Test VALUES (?id, ?name, NULL,NULL,NULL)";
			cmd.Parameters.Add( new MySqlParameter("?id", 1));
			cmd.Parameters.Add( new MySqlParameter("?name", "my ' value"));
			cmd.ExecuteNonQuery();

			cmd.Parameters[0].Value = 2;
			cmd.Parameters[1].Value = @"my "" value";
			cmd.ExecuteNonQuery();

			cmd.Parameters[0].Value = 3;
			cmd.Parameters[1].Value = @"my ` value";
			cmd.ExecuteNonQuery();

			cmd.Parameters[0].Value = 4;
			cmd.Parameters[1].Value = @"my � value";
			cmd.ExecuteNonQuery();

			cmd.Parameters[0].Value = 5;
			cmd.Parameters[1].Value = @"my \ value";
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT * FROM Test";
			MySqlDataReader reader = null;
			try 
			{
				reader = cmd.ExecuteReader();
				reader.Read();
				Assert.AreEqual( "my ' value", reader.GetString(1));
				reader.Read();
				Assert.AreEqual( @"my "" value", reader.GetString(1));
				reader.Read();
				Assert.AreEqual( "my ` value", reader.GetString(1));
				reader.Read();
				Assert.AreEqual( "my � value", reader.GetString(1));
				reader.Read();
				Assert.AreEqual( @"my \ value", reader.GetString(1));
			}
			catch (Exception ex) 
			{
				Assert.Fail( ex.Message );
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test()]
		public void TestDateTimeParameter()
		{
			MySqlCommand cmd = new MySqlCommand("", conn);

			TimeSpan time = new TimeSpan(0, 1, 2, 3);
			DateTime dt = new DateTime( 2003, 11, 11, 1, 2, 3 );
			cmd.CommandText = "INSERT INTO Test VALUES (1, 'test', ?dt, ?time, NULL)";
			cmd.Parameters.Add( new MySqlParameter("?time", time));
			cmd.Parameters.Add( new MySqlParameter("?dt", dt));
			int cnt = cmd.ExecuteNonQuery();
			Assert.AreEqual( 1, cnt, "Insert count" );

			cmd = new MySqlCommand("SELECT tm, dt, ts FROM Test WHERE id=1", conn);
			MySqlDataReader reader = cmd.ExecuteReader();
			reader.Read();
			TimeSpan time2 = (TimeSpan)reader.GetValue(0);
			Assert.AreEqual( time, time2 );

			DateTime dt2 = reader.GetDateTime(1);
			Assert.AreEqual( dt, dt2 );

			DateTime ts2 = reader.GetDateTime(2);
			reader.Close();

			// now check the timestamp column.  We won't check the minute or second for obvious reasons
			DateTime now = DateTime.Now;
			Assert.AreEqual( now.Year, ts2.Year );
			Assert.AreEqual( now.Month, ts2.Month );
			Assert.AreEqual( now.Day, ts2.Day );
			Assert.AreEqual( now.Hour, ts2.Hour );

			// now we'll set some nulls and see how they are handled
			cmd = new MySqlCommand("UPDATE Test SET tm=?ts, dt=?dt WHERE id=1", conn);
			cmd.Parameters.Add( new MySqlParameter("?ts", DBNull.Value ));
			cmd.Parameters.Add( new MySqlParameter("?dt", DBNull.Value));
			cnt = cmd.ExecuteNonQuery();
			Assert.AreEqual( 1, cnt, "Update null count" );

			cmd = new MySqlCommand("SELECT tm, dt FROM Test WHERE id=1", conn);
			reader = cmd.ExecuteReader();
			reader.Read();
			object tso = reader.GetValue(0);
			object dto = reader.GetValue(1);
			Assert.AreEqual( DBNull.Value, tso, "Time column" );
			Assert.AreEqual( DBNull.Value, dto, "DateTime column" );

			reader.Close();

			cmd.CommandText = "DELETE FROM Test WHERE id=1";
			cmd.ExecuteNonQuery();
		}

		[Test()]
		public void NestedQuoting() 
		{
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test (id, name) VALUES(1, 'this is ?\"my value\"')", conn);
			int count = cmd.ExecuteNonQuery();
			Assert.AreEqual( 1, count );
		}

		[Test()]
		public void SetDbType() 
		{
			try 
			{
				IDbConnection conn2 = (IDbConnection)conn;
				IDbCommand cmd = conn.CreateCommand();
				IDbDataParameter prm = cmd.CreateParameter();
				prm.DbType = DbType.Int32;
				Assert.AreEqual( DbType.Int32, prm.DbType );
			}
			catch (Exception ex) 
			{
				Assert.Fail( ex.Message );
			}
		}

		[Test()]
		public void UseOldSyntax() 
		{
			string connStr = conn.ConnectionString + ";old syntax=yes;pooling=false";
			MySqlConnection conn2 = new MySqlConnection(connStr);
			conn2.Open();

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test (id, name) VALUES (@id, @name)", conn2);
			cmd.Parameters.Add( "@id", 33 );
			cmd.Parameters.Add( "@name", "Test" );
			cmd.ExecuteNonQuery();

			MySqlDataReader reader = null;
			try 
			{
				cmd.CommandText = "SELECT * FROM Test";
				reader = cmd.ExecuteReader();
				reader.Read();
				Assert.AreEqual( 33, reader.GetInt32(0) );
				Assert.AreEqual( "Test", reader.GetString(1) );
			}
			catch( Exception ex) 
			{
				Assert.Fail( ex.Message );
			}
			finally 
			{
				if (reader != null) reader.Close(); 
				conn2.Close();
			}
		}

		[Test()]
		[ExpectedException(typeof(ArgumentException))]
		public void NullParameterObject() 
		{
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test (id, name) VALUES (1, ?name)", conn);
			cmd.Parameters.Add( null );
		}

		/// <summary>
		/// Bug #7398  	MySqlParameterCollection doesn't allow parameters without filled in names
		/// </summary>
		[Test]
		public void AllowUnnamedParameters() 
		{
			MySqlCommand cmd = new MySqlCommand("INSERT INTO test (id,name) VALUES (?id, ?name)", conn);
			cmd.Parameters.Add( new MySqlParameter() );
			cmd.Parameters.Add( new MySqlParameter() );
			cmd.Parameters[0].ParameterName = "?id";
			cmd.Parameters[0].Value = 1;
			cmd.Parameters[1].ParameterName = "?name";
			cmd.Parameters[1].Value = "test";
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT id FROM test";
			Assert.AreEqual(1, cmd.ExecuteScalar());

			cmd.CommandText = "SELECT name FROM test";
			Assert.AreEqual( "test", cmd.ExecuteScalar());
		}

		[Test()]
		public void NullParameterValue() 
		{
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test (id, name) VALUES (1, ?name)", conn);
			cmd.Parameters.Add( new MySqlParameter("?name", null));
			cmd.ExecuteNonQuery();
			
			cmd.CommandText = "SELECT name FROM Test WHERE id=1";
			object name = cmd.ExecuteScalar();
			Assert.AreEqual( DBNull.Value, name );
		}

		[Test()]
		public void OddCharsInParameterNames() 
		{
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test (id, name) VALUES (1, ?nam$es)", conn);
			cmd.Parameters.Add( new MySqlParameter("?nam$es", "Test"));
			cmd.ExecuteNonQuery();

			cmd.CommandText = "INSERT INTO Test (id, name) VALUES (2, ?nam_es)";
			cmd.Parameters.Clear();
			cmd.Parameters.Add( new MySqlParameter("?nam_es", "Test2"));
			cmd.ExecuteNonQuery();

			cmd.CommandText = "INSERT INTO Test (id, name) VALUES (3, ?nam.es)";
			cmd.Parameters.Clear();
			cmd.Parameters.Add( new MySqlParameter("?nam.es", "Test3"));
			cmd.ExecuteNonQuery();
			
			cmd.CommandText = "SELECT name FROM Test WHERE id=1";
			object name = cmd.ExecuteScalar();
			Assert.AreEqual( "Test", name );

			cmd.CommandText = "SELECT name FROM Test WHERE id=2";
			name = cmd.ExecuteScalar();
			Assert.AreEqual( "Test2", name );

			cmd.CommandText = "SELECT name FROM Test WHERE id=3";
			name = cmd.ExecuteScalar();
			Assert.AreEqual( "Test3", name );
		}
	}
}
