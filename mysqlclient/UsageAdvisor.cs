using System;
using System.Diagnostics;

namespace MySql.Data.MySqlClient
{
	internal class UsageAdvisor
	{
		private MySqlConnection conn;

		public UsageAdvisor(MySqlConnection conn)
		{
			this.conn = conn;
		}

		public void ReadPartialResultSet(string cmdText)
		{
			if (! conn.Settings.UseUsageAdvisor) return;

			LogUAWarning(cmdText, "Not all rows in resultset were read.");
		}

		public void UsingNoIndex(string cmdText)
		{
			if (! conn.Settings.UseUsageAdvisor) return;

			LogUAWarning(cmdText, "Not using an index.");
		}

		public void UsingBadIndex(string cmdText) 
		{
			if (! conn.Settings.UseUsageAdvisor) return;

			LogUAWarning(cmdText, "Using a bad index.");
		}

		public void AbortingSequentialAccess(MySqlField[] fields, int startIndex)
		{
			if (! conn.Settings.UseUsageAdvisor) return;
			LogUAHeader(null);
			Logger.WriteLine("");
			Logger.WriteLine("A rowset that was being accessed using SequentialAccess had to load " +
				"all of its remaining columns.  This can cause performance problems.  This is most " +
				"likely due to calling Prepare() on a command before reading all the columns of a " +
				"rowset that is being accessed with SequentialAccess");
			LogUAFooter();
		}

		public void ReadPartialRowSet(string cmdText, bool[] uaFieldsUsed, MySqlField[] fields)
		{
			LogUAHeader(cmdText);
			Logger.WriteLine("Reason: Every column was not accessed.  Consider a more focused query.");
			Logger.Write("Fields not accessed: ");
			for (int i=0; i < uaFieldsUsed.Length; i++)
				if (! uaFieldsUsed[i])
					Logger.Write(" " + fields[i].ColumnName);
			Logger.WriteLine(" ");
			LogUAFooter();
		}

        public void Converting(string cmdText, string columnName, 
                               string fromType, string toType)
        {
			LogUAHeader(cmdText);
			Logger.WriteLine("Reason: Performing unnecessary conversion on field " 
                             + columnName + ".");
			Logger.WriteLine("From: " + fromType + " to " + toType);
			LogUAFooter();
        }

		private void LogUAWarning(string cmdText, string reason) 
		{
			LogUAHeader(cmdText);
			Logger.WriteLine("Reason: " + reason);
			LogUAFooter();
		}

		private void LogUAHeader(string cmdText) 
		{
			Logger.WriteLine("USAGE ADVISOR WARNING -------------");
			Logger.WriteLine("Host: " + conn.Settings.Server);
			if (cmdText != null && cmdText.Length > 0)
				Logger.WriteLine("Command Text:  " + cmdText);
		}

		private void LogUAFooter() 
		{
			Logger.WriteLine("-----------------------------------");
		}
	}
}
