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

using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using MySql.Data.Common;
using System.Diagnostics;
using System;
using System.Globalization;

namespace MySql.Data.MySqlClient
{
    class ProcedureCache
    {
        private Hashtable procHash;
        private Queue<int> hashQueue;
        private int maxSize;

        public ProcedureCache(int size)
        {
            maxSize = size;
            hashQueue = new Queue<int>(maxSize);
            procHash = new Hashtable(maxSize);
        }

        public ArrayList GetProcedure(MySqlConnection conn, string spName)
        {
            int dotIndex = spName.IndexOf('.');
            if (dotIndex == -1)
                spName = conn.Database + "." + spName;

            int hash = spName.GetHashCode();
            ArrayList array = (ArrayList)procHash[hash];
            if (array == null)
                array = AddNew(conn, spName);
            return array;
        }

        private ArrayList AddNew(MySqlConnection connection, string spName)
        {
            ArrayList procData = GetProcData(connection, spName);
            if (maxSize > 0)
            {
                if (procHash.Keys.Count == maxSize)
                    TrimHash();
                int hash = spName.GetHashCode();
                procHash.Add(hash, procData);
                hashQueue.Enqueue(hash);
            }
            return procData;
        }

        private void TrimHash()
        {
            int oldestHash = hashQueue.Dequeue();
            procHash.Remove(oldestHash);
        }

        private ArrayList GetProcData(MySqlConnection connection, string spName)
        {
            MySqlCommand cmd = new MySqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            StoredProcedure sp = new StoredProcedure(connection);
            return sp.DiscoverParameters(spName);
        }
    }
}
