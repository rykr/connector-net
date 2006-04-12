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
using System.Diagnostics;
using NUnit.Framework;

namespace MySql.Data.MySqlClient.Tests
{
	[TestFixture()]
	public class UsageAdvisorTests : BaseTest
	{
		private MemoryTraceListener listener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			csAdditions = ";Usage Advisor=true";
			Open();

			execSQL("DROP TABLE IF EXISTS Test");
			createTable( "CREATE TABLE Test (id int, name VARCHAR(200))", "INNODB" );

			listener = new MemoryTraceListener();
			Trace.Listeners.Add(listener);
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			Trace.Listeners.Remove(listener);
			Close();
		}

		[Test]
		public void NotReadingEveryField() 
		{
			execSQL("INSERT INTO Test VALUES (1, 'Test1')");
			execSQL("INSERT INTO Test VALUES (2, 'Test2')");
			execSQL("INSERT INTO Test VALUES (3, 'Test3')");
			execSQL("INSERT INTO Test VALUES (4, 'Test4')");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test; SELECT * FROM Test WHERE id > 2", conn);
			MySqlDataReader reader = null;
			try 
			{
				reader = cmd.ExecuteReader();
				reader.Read();
				int id = reader.GetInt32(0);
				reader.Read();

				listener.Clear();
				Assert.IsTrue( reader.NextResult() );
				Assert.IsTrue(VerifyFieldsNotRead("name"));

				reader.Read();
				listener.Clear();

				Assert.AreEqual( "Test3", reader.GetString(1) );
				Assert.IsFalse( reader.NextResult() );
				Assert.IsTrue(VerifyFieldsNotRead("id"));
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
		public void NotReadingEveryRow() 
		{
			execSQL("INSERT INTO Test VALUES (1, 'Test1')");
			execSQL("INSERT INTO Test VALUES (2, 'Test2')");
			execSQL("INSERT INTO Test VALUES (3, 'Test3')");
			execSQL("INSERT INTO Test VALUES (4, 'Test4')");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test; SELECT * FROM Test WHERE id > 2", conn);
			MySqlDataReader reader = null;
			try 
			{
				reader = cmd.ExecuteReader();
				reader.Read();
				reader.Read();

				listener.Clear();
				Assert.IsTrue( reader.NextResult() );
				Assert.IsTrue(DidNotReadAllRows());

				reader.Read();
				reader.Read();
				listener.Clear();

				Assert.IsFalse(reader.NextResult());
				Assert.IsFalse(DidNotReadAllRows());
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

		private string GetMessage() 
		{
			string message = String.Empty;

			string line = listener.ReadLine();
			while (line != null)
			{
				message += line;
				line = listener.ReadLine();
			}
			return message;
		}

		private bool VerifyFieldsNotRead(string fieldNames) 
		{
			string msg = GetMessage();			
			int index = msg.IndexOf( "Fields not accessed:  " + fieldNames );
			return index != -1;
		}

		private bool DidNotReadAllRows() 
		{
			string msg = GetMessage();
			int index = msg.IndexOf("Not all rows in resultset were read");
			return index != -1;
		}
	}
}