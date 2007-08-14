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
using NUnit.Framework;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace MySql.Data.MySqlClient.Tests
{
	/// <summary>
	/// Summary description for BaseTest.
	/// </summary>
	public class BaseTest
	{
		protected MySqlConnection conn;
		private MySqlConnection rootConn;
		protected string table;
		protected string csAdditions = String.Empty;
		protected string host;
		protected string user;
		protected string password;
		protected int port;
		protected string pipeName;
		protected string memoryName;
		protected string rootUser;
		protected string rootPassword;
		protected string database0;
		protected string database1;

		public BaseTest()
		{
			csAdditions = ";pooling=false;";
			user = "test";
			password = "test";
			host = "localhost";
			port = 3306;
			pipeName = "MYSQL";
			memoryName = "MYSQL";
			rootUser = "root";
			rootPassword = "";

			database0 = System.IO.Path.GetFileNameWithoutExtension(
				System.IO.Path.GetTempFileName());
			database1 = System.IO.Path.GetFileNameWithoutExtension(
				System.IO.Path.GetTempFileName());

#if NET20
			string strHost = ConfigurationManager.AppSettings["host"];
			string strPort = ConfigurationManager.AppSettings["port"];
			string strPipeName = ConfigurationManager.AppSettings["pipename"];
			string strMemName = ConfigurationManager.AppSettings["memory_name"];
#else
			string strHost = ConfigurationSettings.AppSettings["host"];
			string strPort = ConfigurationSettings.AppSettings["port"];
			string strPipeName = ConfigurationSettings.AppSettings["pipename"];
			string strMemName = ConfigurationSettings.AppSettings["memory_name"];
#endif
			if (strHost != null)
				host = strHost;
			if (strPort != null)
				port = Int32.Parse(strPort);
			if (strPipeName != null)
				pipeName = strPipeName;
			if (strMemName != null)
				memoryName = strMemName;
		}

		[TestFixtureSetUp]
		protected virtual void FixtureSetup()
		{
			// open up a root connection
			string connStr = String.Format("server={0};user id={1};password={2};database=mysql;" +
				"persist security info=true;pooling=false;", host, rootUser, rootPassword);
			connStr += GetConnectionInfo();
			rootConn = new MySqlConnection(connStr);
			rootConn.Open();

            // now create our databases
            suExecSQL("CREATE DATABASE `" + database0 + "`");
            suExecSQL("CREATE DATABASE `" + database1 + "`");

            // now allow our user to access them
            suExecSQL(String.Format(@"GRANT ALL ON `{0}`.* to 'test'@'localhost' 
				identified by 'test'", database0));
            suExecSQL(String.Format(@"GRANT ALL ON `{0}`.* to 'test'@'localhost' 
				identified by 'test'", database1));
			suExecSQL(String.Format(@"GRANT ALL ON `{0}`.* to 'test'@'%' 
				identified by 'test'", database0));
			suExecSQL(String.Format(@"GRANT ALL ON `{0}`.* to 'test'@'%' 
				identified by 'test'", database1));
			suExecSQL("FLUSH PRIVILEGES");

			rootConn.ChangeDatabase(database0);

			Open();
		}

		[TestFixtureTearDown]
		protected virtual void TestFixtureTearDown()
		{
			suExecSQL("DROP DATABASE " + database0);
			suExecSQL("DROP DATABASE " + database1);

			rootConn.Close();
			Close();
		}

		protected virtual string GetConnectionInfo()
		{
			return String.Format("protocol=tcp;port={0}", port);
		}

		protected string GetConnectionString(bool includedb)
		{
			string connStr = String.Format("server={0};user id={1};password={2};" +
				 "persist security info=true;{3}", host, user, password, csAdditions);
			if (includedb)
				connStr += String.Format("database={0};", database0);
			connStr += GetConnectionInfo();
			return connStr;
		}

		protected string GetConnectionStringEx(string user, string pw, bool includedb)
		{
			string connStr = String.Format("server={0};user id={1};" +
				 "persist security info=true;{2}", host, user, csAdditions);
			if (includedb)
				connStr += String.Format("database={0};", database0);
			if (pw != null)
				connStr += String.Format("password={0};", pw);
			connStr += GetConnectionInfo();
			return connStr;
		}

		protected void Open()
		{
			try
			{
				string connString = GetConnectionString(true);
				conn = new MySqlConnection(connString);
				conn.Open();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine(ex.Message);
				throw;
			}
		}

		protected void Close()
		{
			try
			{
				// delete the table we created.
				if (conn.State == ConnectionState.Closed)
					conn.Open();
				execSQL("DROP TABLE IF EXISTS Test");
				conn.Close();
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		protected bool Is51
		{
			get
			{
				string v = conn.ServerVersion;
				return v.StartsWith("5.1");
			}
		}

		protected bool Is50
		{
			get
			{
				string v = conn.ServerVersion;
				return v.StartsWith("5.0") || v.StartsWith("5.1");
			}
		}

		protected bool Is41
		{
			get { return conn.ServerVersion.StartsWith("4.1"); }
		}

		protected bool Is40
		{
			get { return conn.ServerVersion.StartsWith("4.0"); }
		}

		[SetUp]
		protected virtual void Setup()
		{
			try
			{
				IDataReader reader = execReader("SHOW TABLES LIKE 'Test'");
				bool exists = reader.Read();
				reader.Close();
				if (exists)
					execSQL("TRUNCATE TABLE Test");
				if (Is50)
				{
					execSQL("DROP PROCEDURE IF EXISTS spTest");
					execSQL("DROP FUNCTION IF EXISTS fnTest");
				}
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[TearDown]
		protected virtual void Teardown()
		{
			if (Is50)
			{
				execSQL("DROP PROCEDURE IF EXISTS spTest");
				execSQL("DROP FUNCTION IF EXISTS fnTest");
			}
		}

		protected void KillConnection(MySqlConnection c)
		{
			int threadId = c.ServerThread;
			MySqlCommand cmd = new MySqlCommand("KILL " + threadId, conn);
			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
			c.Ping();  // this final ping will cause MySQL to clean up the killed thread
		}

		protected void createTable(string sql, string engine)
		{
			if (Is41 || Is50)
				sql += " ENGINE=" + engine;
			else
				sql += " TYPE=" + engine;
			execSQL(sql);
		}

		protected void suExecSQL(string sql)
		{
			MySqlCommand cmd = new MySqlCommand(sql, rootConn);
			cmd.ExecuteNonQuery();
		}

		protected void execSQL(string sql)
		{
			MySqlCommand cmd = new MySqlCommand(sql, conn);
			cmd.ExecuteNonQuery();
		}

		protected IDataReader execReader(string sql)
		{
			MySqlCommand cmd = new MySqlCommand(sql, conn);
			return cmd.ExecuteReader();
		}

	}
}
