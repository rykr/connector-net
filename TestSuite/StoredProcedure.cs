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
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace MySql.Data.MySqlClient.Tests
{
	/// <summary>
	/// Summary description for StoredProcedure.
	/// </summary>
	[TestFixture]
	public class StoredProcedure : BaseTest
	{
		private static string fillError = null;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			csAdditions = ";pooling=false";
			Open();
			execSQL("DROP TABLE IF EXISTS Test; CREATE TABLE Test (id INT, name VARCHAR(100))");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown() 
		{
			Close();
		}

		/// <summary>
		/// Bug #7623  	Adding MySqlParameter causes error if MySqlDbType is Decimal
		/// </summary>
		[Test]
		[Category("5.0")]
		public void ReturningResultset() 
		{
			// create our procedure
			execSQL( "CREATE PROCEDURE spTest(val decimal(10,3)) begin select val; end");
			
			using (MySqlCommand cmd = new MySqlCommand("spTest", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				MySqlParameter p = cmd.Parameters.Add("val", MySqlDbType.Decimal);
				p.Precision = 10;
				p.Scale = 3;
				p.Value = 21;

				object id = cmd.ExecuteScalar();
				Assert.AreEqual( 21, id );
			}
		}

		[Test]
		[Category("5.0")]
		public void NonQuery()
		{
			execSQL("CREATE PROCEDURE spTest(IN value INT) BEGIN INSERT INTO Test VALUES(value, 'Test'); END" );

			//setup testing data
			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add( "?value", 2 );
			int rowsAffected = cmd.ExecuteNonQuery();
			Assert.AreEqual(1, rowsAffected);

			cmd.CommandText = "SELECT * FROM Test";
			cmd.CommandType = CommandType.Text;
			MySqlDataReader reader = null;
			
			try 
			{
				reader = cmd.ExecuteReader();
				Assert.IsTrue( reader.Read() );
				Assert.AreEqual( 2, reader.GetInt32(0) );
				Assert.AreEqual( "Test", reader.GetString(1) );
				Assert.IsFalse( reader.Read() );
				Assert.IsFalse( reader.NextResult() );
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
		[Category("5.0")]
		public void OutputParameters()
		{
			// create our procedure
			execSQL( "DROP PROCEDURE IF EXISTS spCount" );
			execSQL( "CREATE PROCEDURE spCount( out value VARCHAR(50), OUT intVal INT, OUT dateVal TIMESTAMP, OUT floatVal FLOAT ) " + 
				"BEGIN  SET value='42';  SET intVal=33; SET dateVal='2004-06-05 07:58:09'; SET floatVal = 1.2; END" );

			MySqlCommand cmd = new MySqlCommand("spCount", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add( new MySqlParameter("?value", MySqlDbType.VarChar));
			cmd.Parameters.Add( new MySqlParameter( "?intVal", MySqlDbType.Int32 ) );
			cmd.Parameters.Add( new MySqlParameter( "?dateVal", MySqlDbType.Datetime ) );
			cmd.Parameters.Add( new MySqlParameter( "?floatVal", MySqlDbType.Float ) );
			cmd.Parameters[0].Direction = ParameterDirection.Output;
			cmd.Parameters[1].Direction = ParameterDirection.Output;
			cmd.Parameters[2].Direction = ParameterDirection.Output;
			cmd.Parameters[3].Direction = ParameterDirection.Output;
			int rowsAffected = cmd.ExecuteNonQuery();

			Assert.AreEqual( 0, rowsAffected );
			Assert.AreEqual( "42", cmd.Parameters[0].Value );
			Assert.AreEqual( 33, cmd.Parameters[1].Value );
			Assert.AreEqual( new DateTime(2004, 6, 5, 7, 58, 9), Convert.ToDateTime(cmd.Parameters[2].Value) );
			Assert.AreEqual( 1.2, cmd.Parameters[3].Value );

			execSQL("DROP PROCEDURE spCount");
		}

		[Test()]
		[Category("5.0")]
		public void NoBatch()
		{
			try 
			{
				MySqlCommand cmd = new MySqlCommand("spTest;select * from test", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
				Assert.Fail("Should have thrown an exception");
			}
			catch (MySqlException) 
			{
			}
		}

		[Test()]
		[Category("5.0")]
		public void WrongParameters()
		{
			try 
			{
				MySqlCommand cmd = new MySqlCommand("spTest", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add( "?p2", 1 );
				cmd.ExecuteNonQuery();
				Assert.Fail("Should have thrown an exception");
			}
			catch (MySqlException) 
			{
			}
		}

		[Test]
		[Category("5.0")]
		public void NoInOutMarker() 
		{
			// create our procedure
			execSQL( "CREATE PROCEDURE spTest( valin varchar(50) ) BEGIN  SELECT valin;  END" );

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add( "?valin", "myvalue" );
			object val = cmd.ExecuteScalar();
			Assert.AreEqual( "myvalue", val );
		}

		[Test()]
		[Category("5.0")]
		public void InputOutputParameters()
		{
			// create our procedure
			execSQL( "CREATE PROCEDURE spTest( INOUT strVal VARCHAR(50), INOUT numVal INT, OUT outVal INT UNSIGNED ) " +
				"BEGIN  SET strVal = CONCAT(strVal,'ending'); SET numVal=numVal * 2;  SET outVal=99; END" );

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add( "?strVal", "beginning" );
			cmd.Parameters.Add( "?numVal", 33 );
			cmd.Parameters.Add( "?outVal", MySqlDbType.Int32);
			cmd.Parameters[0].Direction = ParameterDirection.InputOutput;
			cmd.Parameters[1].Direction = ParameterDirection.InputOutput;
			cmd.Parameters[2].Direction = ParameterDirection.Output;
			int rowsAffected = cmd.ExecuteNonQuery();
			Assert.AreEqual( 0, rowsAffected );
			Assert.AreEqual( "beginningending", cmd.Parameters[0].Value );
			Assert.AreEqual( 66, cmd.Parameters[1].Value );
			Assert.AreEqual(99, cmd.Parameters[2].Value);
		}

		[Test()]
		[Category("5.0")]
		public void NoSPOnPre50() 
		{
			try 
			{
				MySqlCommand cmd = new MySqlCommand("spTest", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
				Assert.Fail( "This should not have worked" );
			}
			catch (Exception) 
			{
			}
		}

		[Test()]
		[Category("5.0")]
		public void ExecuteScalar() 
		{
			// create our procedure
			execSQL( "CREATE PROCEDURE spTest( IN valin VARCHAR(50), OUT valout VARCHAR(50) ) " +
				"BEGIN  SET valout=valin;  SELECT 'Test'; END" );

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add( "?valin", "valuein" );
			cmd.Parameters.Add( new MySqlParameter("?valout", MySqlDbType.VarChar));
			cmd.Parameters[1].Direction = ParameterDirection.Output;
			object result = cmd.ExecuteScalar();
			Assert.AreEqual( "Test", result );
			Assert.AreEqual( "valuein", cmd.Parameters[1].Value );
		}

		[Test()]
		[Category("5.0")]
		public void ExecuteReader()
		{
			// create our procedure
			execSQL( "CREATE PROCEDURE spTest() " +
				"BEGIN  SELECT * FROM mysql.db; END" );

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.Parameters.Add("?a", 3);
			cmd.CommandType = CommandType.StoredProcedure;
			MySqlDataReader reader = cmd.ExecuteReader();
			Assert.AreEqual( true, reader.Read() );
			Assert.AreEqual( false, reader.NextResult() );
			Assert.AreEqual( false, reader.Read() );
			reader.Close();
		}

		[Test()]
		[Category("5.0")]
		public void MultipleResultsets() 
		{
			MultipleResultsetsImpl(false);
//			MultipleResultsetsImpl(true);
		}

		private void MultipleResultsetsImpl(bool prepare)
		{
			// create our procedure
			execSQL( "CREATE PROCEDURE spTest() " +
				"BEGIN  SELECT 1; SELECT 2; END" );

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			if (prepare) cmd.Prepare();
			cmd.CommandType = CommandType.StoredProcedure;
			MySqlDataReader reader = cmd.ExecuteReader();
			Assert.AreEqual( true, reader.Read() );
			Assert.AreEqual( true, reader.NextResult() );
			Assert.AreEqual( true, reader.Read() );
			Assert.AreEqual( false, reader.NextResult() );
			Assert.AreEqual( false, reader.Read() );
			reader.Close();

			DataSet ds = new DataSet();
			MySqlCommand cmd2 = new MySqlCommand("spTest", conn);
			cmd2.CommandType = CommandType.StoredProcedure;
			MySqlDataAdapter da = new MySqlDataAdapter( cmd2 );
			da.FillError += new FillErrorEventHandler(da_FillError);
			fillError = null;
			da.Fill(ds);
			Assert.AreEqual( 2, ds.Tables.Count );
			Assert.AreEqual( 1, ds.Tables[0].Rows.Count );
			Assert.AreEqual( 1, ds.Tables[1].Rows.Count );
			Assert.AreEqual( 1, ds.Tables[0].Rows[0][0] );
			Assert.AreEqual( 2, ds.Tables[1].Rows[0][0] );
			Assert.IsNull( fillError );
		}

		private void da_FillError(object sender, FillErrorEventArgs e)
		{
			fillError = e.Errors.Message;
			e.Continue = true;
		}

		[Test()]
		[Category("5.0")]
		public void FunctionNoParams() 
		{
			execSQL( "CREATE FUNCTION fnTest() RETURNS CHAR(50)" +
				"BEGIN  RETURN \"Test\"; END" );

			MySqlCommand cmd = new MySqlCommand("SELECT fnTest()", conn);
			cmd.CommandType = CommandType.Text;
			object result = cmd.ExecuteScalar();
			Assert.AreEqual( "Test", result );

			execSQL("DROP FUNCTION fnTest");
		}

		[Test()]
		[Category("5.0")]
		public void FunctionParams() 
		{
			execSQL( "CREATE FUNCTION fnTest( val1 INT, val2 CHAR(40) ) RETURNS INT " +
				"BEGIN  RETURN val1 + LENGTH(val2);  END" );

			MySqlCommand cmd = new MySqlCommand("SELECT fnTest(22, 'Test')", conn);
			cmd.CommandType = CommandType.Text;
			object result = cmd.ExecuteScalar();
			Assert.AreEqual( 26, result);

			execSQL("DROP FUNCTION fnTest");
		}

		[Test()]
		[Category("5.0")]
		public void UseOldSyntax() 
		{
			// create our procedure
			execSQL("CREATE PROCEDURE spTest( IN valin VARCHAR(50), OUT valout VARCHAR(50) ) " +
				"BEGIN  SET valout=valin;  SELECT 'Test'; END");

			MySqlConnection c2 = new MySqlConnection(conn.ConnectionString + ";old syntax=yes");
			c2.Open();

			MySqlCommand cmd = new MySqlCommand("spTest", c2);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("@valin", "value");
			cmd.Parameters.Add( new MySqlParameter("@valout", MySqlDbType.VarChar));
			cmd.Parameters[1].Direction = ParameterDirection.Output;
			object result = cmd.ExecuteScalar();
			Assert.AreEqual("Test", result);
			Assert.AreEqual("value", cmd.Parameters[1].Value);
			c2.Close();
		}

		[Test]
		[Category("5.0")]
		public void ExecuteWithCreate() 
		{
			// create our procedure
			string sql = "CREATE PROCEDURE spTest(IN var INT) BEGIN  SELECT var; END; call spTest(?v)";

			MySqlCommand cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.Add(new MySqlParameter("?v", 33));
			object val = cmd.ExecuteScalar();
			Assert.AreEqual( 33, val );
		}

		/// <summary>
		/// Bug #9722 Connector does not recognize parameters separated by a linefeed 
		/// </summary>
		[Test]
		[Category("5.0")]
		public void OtherProcSigs() 
		{
			// create our procedure
			execSQL( "CREATE PROCEDURE spTest(IN \r\nvalin DECIMAL(10,2),\nIN val2 INT) " +
				"SQL SECURITY INVOKER BEGIN  SELECT valin; END" );

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add( "?valin", 20.4 );
			cmd.Parameters.Add( "?val2", 4 );
			object val = cmd.ExecuteScalar();
			Assert.AreEqual( 20.4, val );

			// create our second procedure
			execSQL("DROP PROCEDURE spTest");
			execSQL("CREATE PROCEDURE spTest( \r\n) BEGIN  SELECT 4; END" );
			cmd.Parameters.Clear();
			val = cmd.ExecuteScalar();
			Assert.AreEqual(4, val);
		}


		/// <summary>
		/// Bug #10644 Cannot call a stored function directly from Connector/Net 
		/// </summary>
		[Test]
		[Category("5.0")]
		public void CallingStoredFunctionasProcedure()
		{
			execSQL("DROP FUNCTION IF EXISTS spFunc");
			execSQL("CREATE FUNCTION spFunc(valin int) RETURNS INT BEGIN return valin * 2; END");
			MySqlCommand cmd = new MySqlCommand("spFunc", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("?valin", 22);
			cmd.Parameters.Add("retval", MySqlDbType.Int32);
			cmd.Parameters[1].Direction = ParameterDirection.ReturnValue;
			cmd.ExecuteNonQuery();
			Assert.AreEqual(44, cmd.Parameters[1].Value);
		}

		/// <summary>
		/// Bug #11450  	Connector/Net, current database and stored procedures
		/// </summary>
		[Test]
		[Category("5.0")]
		public void NoDefaultDatabase()
		{
			// create our procedure
			execSQL("CREATE PROCEDURE spTest() BEGIN  SELECT 4; END" );

			string newConnStr = GetConnectionString(false);
			MySqlConnection c = new MySqlConnection(newConnStr);
			try 
			{
				c.Open();
				MySqlCommand cmd2 = new MySqlCommand("use test", c);
				cmd2.ExecuteNonQuery();

				MySqlCommand cmd = new MySqlCommand("spTest", c);
				cmd.CommandType = CommandType.StoredProcedure;
				object val = cmd.ExecuteScalar();
				Assert.AreEqual(4, val);

				cmd2.CommandText = "use mysql";
				cmd2.ExecuteNonQuery();

				cmd.CommandText = "test.spTest";
				val = cmd.ExecuteScalar();
				Assert.AreEqual(4, val);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				c.Close();
			}
		}

		/// <summary>
		/// Bug #13590  	ExecuteScalar returns only Int64 regardless of actual SQL type
		/// </summary>
		[Category("NotWorking")]
		[Test]
		public void TestSelectingInts()
		{
			execSQL("CREATE PROCEDURE spTest() BEGIN DECLARE myVar INT; " +
				"SET MyVar := 1; SELECT CAST(myVar as INT); END");
			
			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			object val = cmd.ExecuteScalar();
			Assert.AreEqual(1, val, "Checking value");
			Assert.IsTrue(val is System.Int32, "Checking type");
		}

		/// <summary>
		/// Bug #13632  	the MySQLCommandBuilder.deriveparameters has not been updated for MySQL 5
        /// Bug #15077  	Error MySqlCommandBuilder.DeriveParameters for sp without parameters.
        /// Bug #19515  	DiscoverParameters fails on numeric datatype
		/// </summary>
		[Category("5.0")]
		[Test]
		public void DeriveParameters()
		{
			execSQL("DROP TABLE IF EXISTS test2");
			execSQL("CREATE TABLE test2 (c CHAR(20))");
			execSQL("INSERT INTO test2 values ( 'xxxx')");
			MySqlCommand cmd2 = new MySqlCommand("SELECT * FROM test2", conn);
			MySqlDataReader reader =cmd2.ExecuteReader();
			reader.Close();

			execSQL("CREATE PROCEDURE spTest(IN \r\nvalin DECIMAL(10,2), " +
				"\nIN val2 INT, INOUT val3 FLOAT, OUT val4 DOUBLE, INOUT val5 BIT, " +
				"val6 VARCHAR(155), val7 SET('a','b'), val8 CHAR, val9 NUMERIC(10,2)) " +
                "BEGIN SELECT 1; END");

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			MySqlDataAdapter da = new MySqlDataAdapter(cmd);
			MySqlCommandBuilder.DeriveParameters(cmd);

			Assert.AreEqual(9, cmd.Parameters.Count);
			Assert.AreEqual("valin", cmd.Parameters[0].ParameterName);
			Assert.AreEqual(ParameterDirection.Input, cmd.Parameters[0].Direction);
			Assert.AreEqual(MySqlDbType.NewDecimal, cmd.Parameters[0].MySqlDbType);

			Assert.AreEqual("val2", cmd.Parameters[1].ParameterName);
			Assert.AreEqual(ParameterDirection.Input, cmd.Parameters[1].Direction);
			Assert.AreEqual(MySqlDbType.Int32, cmd.Parameters[1].MySqlDbType);

			Assert.AreEqual("val3", cmd.Parameters[2].ParameterName);
			Assert.AreEqual(ParameterDirection.InputOutput, cmd.Parameters[2].Direction);
			Assert.AreEqual(MySqlDbType.Float, cmd.Parameters[2].MySqlDbType);

			Assert.AreEqual("val4", cmd.Parameters[3].ParameterName);
			Assert.AreEqual(ParameterDirection.Output, cmd.Parameters[3].Direction);
			Assert.AreEqual(MySqlDbType.Double, cmd.Parameters[3].MySqlDbType);

			Assert.AreEqual("val5", cmd.Parameters[4].ParameterName);
			Assert.AreEqual(ParameterDirection.InputOutput, cmd.Parameters[4].Direction);
			Assert.AreEqual(MySqlDbType.Bit, cmd.Parameters[4].MySqlDbType);

			Assert.AreEqual("val6", cmd.Parameters[5].ParameterName);
			Assert.AreEqual(ParameterDirection.Input, cmd.Parameters[5].Direction);
			Assert.AreEqual(MySqlDbType.VarChar, cmd.Parameters[5].MySqlDbType);

			Assert.AreEqual("val7", cmd.Parameters[6].ParameterName);
			Assert.AreEqual(ParameterDirection.Input, cmd.Parameters[6].Direction);
			Assert.AreEqual(MySqlDbType.Set, cmd.Parameters[6].MySqlDbType);

			Assert.AreEqual("val8", cmd.Parameters[7].ParameterName);
			Assert.AreEqual(ParameterDirection.Input, cmd.Parameters[7].Direction);
			Assert.AreEqual(MySqlDbType.Char, cmd.Parameters[7].MySqlDbType);

            Assert.AreEqual("val9", cmd.Parameters[8].ParameterName);
            Assert.AreEqual(ParameterDirection.Input, cmd.Parameters[8].Direction);
            Assert.AreEqual(MySqlDbType.NewDecimal, cmd.Parameters[8].MySqlDbType);

            execSQL("DROP PROCEDURE spTest");
            execSQL("CREATE PROCEDURE spTest() BEGIN END");
            cmd.CommandText = "spTest";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            da = new MySqlDataAdapter(cmd);
            MySqlCommandBuilder.DeriveParameters(cmd);
            Assert.AreEqual(0, cmd.Parameters.Count);
        }

		/// <summary>
		/// Bug #13632  	the MySQLCommandBuilder.deriveparameters has not been updated for MySQL 5
		/// </summary>
		[Category("5.0")]
		[Test]
		public void DeriveParametersForFunction()
		{
			try 
			{
				execSQL("DROP FUNCTION IF EXISTS spTest");
				execSQL("CREATE FUNCTION spTest(v1 DATETIME) RETURNS INT " +
					" BEGIN RETURN 1; END");

				MySqlCommand cmd = new MySqlCommand("spTest", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				MySqlDataAdapter da = new MySqlDataAdapter(cmd);
				MySqlCommandBuilder.DeriveParameters(cmd, false);

				Assert.AreEqual(2, cmd.Parameters.Count);
				Assert.AreEqual("v1", cmd.Parameters[0].ParameterName);
				Assert.AreEqual(ParameterDirection.Input, cmd.Parameters[0].Direction);
				Assert.AreEqual(MySqlDbType.Datetime, cmd.Parameters[0].MySqlDbType);

				Assert.AreEqual(ParameterDirection.ReturnValue, cmd.Parameters[1].Direction);
				Assert.AreEqual(MySqlDbType.Int32, cmd.Parameters[1].MySqlDbType);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		/// <summary>
		/// Bug #11386  	Numeric parameters with Precision and Scale not taken into account by Connector
		/// </summary>
		[Test]
		[Category("5.0")]
		public void DecimalAsParameter()
		{
			execSQL("CREATE PROCEDURE spTest(IN d DECIMAL(19,4)) BEGIN SELECT d; END");

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("?d", 21);
			decimal d = (decimal)cmd.ExecuteScalar();
			Assert.AreEqual(21, d);
		}

		/// <summary>
		/// Bug #6902  	Errors in parsing stored procedure parameters
		/// </summary>
		[Test]
		[Category("5.0")]
		public void ParmWithCharacterSet()
		{
			execSQL("CREATE PROCEDURE spTest(P longtext character set utf8) " +
				"BEGIN SELECT P; END");

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("?P", "This is my value");
			string p = (string)cmd.ExecuteScalar();
			Assert.AreEqual("This is my value", p);
		}

		/// <summary>
		/// Bug #13753  	Exception calling stored procedure with special characters in parameters
		/// </summary>
		[Test]
		[Category("5.0")]
		public void SpecialCharacters()
		{
			execSQL("SET sql_mode=ANSI_QUOTES");
			try 
			{
				execSQL("CREATE PROCEDURE spTest(\"@Param1\" text) BEGIN SELECT \"@Param1\"; END");

				MySqlCommand cmd = new MySqlCommand("spTest", conn);
				cmd.Parameters.Add("@Param1", "This is my value");
				cmd.CommandType = CommandType.StoredProcedure;

				string val = (string)cmd.ExecuteScalar();
				Assert.AreEqual("This is my value", val);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				execSQL("SET sql_mode=\"\"");
			}
		}

		[Test]
		[Category("NotWorking")]
		public void CallingSPWithPrepare()
		{
			execSQL("DROP PROCEDURE IF EXISTS spTest");
			execSQL("CREATE PROCEDURE spTest(P int) BEGIN SELECT P; END");

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("?P", 33);
			cmd.Prepare();

			int p = (int)cmd.ExecuteScalar();
			Assert.AreEqual(33, p);
		}

		/// <summary>
		/// Bug #13927  	Multiple Records to same Table in Transaction Problem
		/// </summary>
		[Test]
		[Category("5.0")]
		public void MultileRecords()
		{
			execSQL("DROP PROCEDURE IF EXISTS spTest");
			execSQL("CREATE PROCEDURE spTest(id int, str VARCHAR(45)) BEGIN INSERT INTO test VALUES(id, str); END");

			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("?id", 1);
			cmd.Parameters.Add("?str", "First record");
			cmd.ExecuteNonQuery();

			cmd.Parameters.Add("?id", 2);
			cmd.Parameters.Add("?str", "Second record");
			cmd.ExecuteNonQuery();

			MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM test", conn);
			DataTable dt = new DataTable();
			da.Fill(dt);

			Assert.AreEqual(1, dt.Rows[0]["id"]);
			Assert.AreEqual(2, dt.Rows[1]["id"]);
			Assert.AreEqual("First record", dt.Rows[0]["name"]);
			Assert.AreEqual("Second record", dt.Rows[1]["name"]);
		}

		/// <summary>
		/// Bug #16788 Only byte arrays and strings can be serialized by MySqlBinary 
		/// </summary>
		[Test]
		[Category("5.0")]
		public void Bug16788()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id integer(9), state varchar(2))");
			execSQL("CREATE PROCEDURE spTest(IN p1 integer(9), IN p2 varchar(2)) " +
				"BEGIN " +
				"INSERT INTO test (id, state) VALUES (p1, p2); " +
				"END");
			
			MySqlCommand cmd = conn.CreateCommand();
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "spTest";
			cmd.Parameters.Add("p1", MySqlDbType.UInt16, 9);
			cmd.Parameters["p1"].Value = 44;
			cmd.Parameters.Add("p2", MySqlDbType.VarChar, 2);
			cmd.Parameters["p2"].Value = "ss";
			try
			{
				cmd.ExecuteNonQuery();
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

        [Test]
        [Category("5.0")]
        public void ReturningEmptyResultSet()
        {
            execSQL("DROP PROCEDURE IF EXISTS spTest");
            execSQL("DROP TABLE IF EXISTS test1");
            execSQL("DROP TABLE IF EXISTS test2");
            execSQL("CREATE TABLE test1 (id int AUTO_INCREMENT NOT NULL, " +
                "Name VARCHAR(100) NOT NULL, PRIMARY KEY(id))");
            execSQL("CREATE TABLE test2 (id int AUTO_INCREMENT NOT NULL, " +
                "id1 INT NOT NULL, id2 INT NOT NULL, PRIMARY KEY(id))");
            
            execSQL("INSERT INTO test1 (Id, Name) VALUES (1, 'Item1')");
            execSQL("INSERT INTO test1 (Id, Name) VALUES (2, 'Item2')");
            execSQL("INSERT INTO test2 (Id, Id1, Id2) VALUES (1, 1, 1)");
            execSQL("INSERT INTO test2 (Id, Id1, Id2) VALUES (2, 2, 1)");

            execSQL("CREATE PROCEDURE spTest(Name VARCHAR(100), OUT Table1Id INT) " +
                "BEGIN SELECT t1.Id INTO Table1Id FROM test1 t1 WHERE t1.Name LIKE Name; " +
                "SELECT t3.Id2 FROM test2 t3 WHERE t3.Id1 = Table1Id; END");

            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "spTest";
            cmd.Parameters.Add("Name", "Item3");
            cmd.Parameters.Add("Table1Id", MySqlDbType.Int32);
            cmd.Parameters["Table1Id"].Direction = ParameterDirection.Output;

            DataSet ds = new DataSet();
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(ds);
        }
	}
}
