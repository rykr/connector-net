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
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace MySql.Data.MySqlClient.Tests
{
	[TestFixture]
	public class TimeoutAndCancel : BaseTest
	{
        private delegate void CommandInvokerDelegate(MySqlCommand cmdToRun);

        private void CommandRunner(MySqlCommand cmdToRun)
        {
            try
            {
                object o = cmdToRun.ExecuteScalar();
                Assert.IsNull(o);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Category("5.0")]
        [Test]
        public void CancelSingleQuery()
        {
            // first we need a routine that will run for a bit
            execSQL(@"CREATE PROCEDURE spTest(duration INT) 
                BEGIN 
                    SELECT SLEEP(duration);
                END");

            MySqlCommand cmd = new MySqlCommand("spTest", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("duration", 60);

            // now we start execution of the command
            CommandInvokerDelegate d = new CommandInvokerDelegate(CommandRunner);
            IAsyncResult iar = d.BeginInvoke(cmd, null, null);

            // sleep 5 seconds
            Thread.Sleep(5000);

            // now cancel the command
            cmd.Cancel();
        }

        int stateChangeCount;
        [Test]
        public void WaitTimeoutExpiring()
        {
            MySqlConnection c = new MySqlConnection(GetConnectionString(true));
            c.Open();
            c.StateChange += new StateChangeEventHandler(c_StateChange);

            // set the session wait timeout on this new connection
            MySqlCommand cmd = new MySqlCommand("SET SESSION interactive_timeout=10", c);
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SET SESSION wait_timeout=10";
            cmd.ExecuteNonQuery();

            stateChangeCount = 0;
            // now wait 10 seconds
            Thread.Sleep(15000);

            try
            {
                cmd.CommandText = "SELECT now()";
                object date = cmd.ExecuteScalar();
            }
            catch (Exception) { }
            Assert.AreEqual(1, stateChangeCount);
            Assert.AreEqual(ConnectionState.Closed, c.State);

            c = new MySqlConnection(GetConnectionString(true));
            c.Open();
            cmd = new MySqlCommand("SELECT now() as thetime, database() as db", c);
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                Assert.IsTrue(r.Read());
            }
        }

        void c_StateChange(object sender, StateChangeEventArgs e)
        {
            stateChangeCount++;
        }

        [Category("5.0")]
        [Test]
        public void TimeoutExpiring()
        {
            // first we need a routine that will run for a bit
            execSQL(@"CREATE PROCEDURE spTest(duration INT) 
                BEGIN 
                    SELECT SLEEP(duration);
                END");

            DateTime start = DateTime.Now;
            try
            {
                MySqlCommand cmd = new MySqlCommand("spTest", conn);
                cmd.Parameters.AddWithValue("duration", 60);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 5;
                cmd.ExecuteNonQuery();
                Assert.Fail("Should not get to this point");
            }
            catch (MySqlException ex)
            {
                TimeSpan ts = DateTime.Now.Subtract(start);
                Assert.IsTrue(ts.TotalSeconds <= 10);
                Assert.IsTrue(ex.Message.StartsWith("Timeout expired"), "Message is wrong");
            }
        }

        [Category("5.0")]
        [Test]
        public void TimeoutNotExpiring()
        {
            // first we need a routine that will run for a bit
            execSQL(@"CREATE PROCEDURE spTest(duration INT) 
                BEGIN 
                    SELECT SLEEP(duration);
                END");

            try
            {
                MySqlCommand cmd = new MySqlCommand("spTest", conn);
                cmd.Parameters.AddWithValue("duration", 10);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 15;
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Category("5.0")]
        [Test]
        public void TimeoutDuringBatch()
        {
            execSQL(@"CREATE PROCEDURE spTest(duration INT) 
                BEGIN 
                    SELECT SLEEP(duration);
                END");

            execSQL("DROP TABLE IF EXISTS test");
            execSQL("CREATE TABLE test (id INT)");

            MySqlCommand cmd = new MySqlCommand(
                "call spTest(10);INSERT INTO test VALUES(4)", conn);
            cmd.CommandTimeout = 5;
            try
            {
                cmd.ExecuteNonQuery();
                Assert.Fail("This should have timed out");
            }
            catch (MySqlException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Timeout expired"), "Message is wrong");
            }

            cmd.CommandText = "SELECT COUNT(*) FROM test";
            Assert.AreEqual(0, cmd.ExecuteScalar());
        }
    }
}