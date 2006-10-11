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
using System.Text;
using System.Collections;
using MySql.Data.Common;
#if NET20
using System.Collections.Generic;
#else
using System.Collections.Specialized;
#endif

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CharSetMap.
	/// </summary>
	internal class CharSetMap
	{
#if NET20
      private static Dictionary<string, string> mapping;
#else
      private static StringDictionary mapping;
#endif

        // we use a static constructor here since we only want to init
        // the mapping once
		static CharSetMap() 
		{
            InitializeMapping();
      }

		/// <summary>
		/// Returns the text encoding for a given MySQL character set name
		/// </summary>
		/// <param name="version">Version of the connection requesting the encoding</param>
		/// <param name="CharSetName">Name of the character set to get the encoding for</param>
		/// <returns>Encoding object for the given character set name</returns>
		public static Encoding GetEncoding(DBVersion version, string CharSetName) 
		{
			try 
			{
				string encodingName = mapping[CharSetName];
            if (encodingName == null)
					throw new MySqlException("Character set '" + CharSetName + "' is not supported");

            return Encoding.GetEncoding(encodingName);
			}
			catch (System.NotSupportedException) 
			{
				return Encoding.GetEncoding(0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static void InitializeMapping()
		{
			LoadCharsetMap();
		}

		private static void LoadCharsetMap()
      {
#if NET20
         mapping = new Dictionary<string, string>();
#else
         mapping = new StringDictionary();
#endif

			mapping.Add("big5", "big5");		
			mapping.Add("sjis", "sjis");		
			mapping.Add("gb2312", "gb2312");
			mapping.Add("latin1", "latin1");
			mapping.Add("latin2", "latin2");
			mapping.Add("latin3", "latin3");
			mapping.Add("latin4", "latin4");
			mapping.Add("latin5", "latin5");
			mapping.Add("greek", "greek");
			mapping.Add("hebrew", "hebrew");
			mapping.Add("utf8", "utf-8");
			mapping.Add("ucs2", "UTF-16BE");
			mapping.Add("cp1251", "cp1251");
			mapping.Add("tis620", "windows-874");
			mapping.Add("cp1250", "windows-1250");
			mapping.Add("cp932", "cp932");
         mapping.Add("win1250", "windows-1250");
         mapping.Add("cp1256", "cp1256");
         mapping.Add("latin1_de", "iso-8859-1");
         mapping.Add("german1", "iso-8859-1");
         mapping.Add("danish", "iso-8859-1");
         mapping.Add("czech", "iso-8859-2");
         mapping.Add("hungarian", "iso-8859-2");
         mapping.Add("croat", "iso-8859-2");
         mapping.Add("latin7", "iso-8859-7");
         mapping.Add("latvian", "iso-8859-13");
         mapping.Add("latvian1", "iso-8859-13");
         mapping.Add("estonia", "iso-8859-13");
         mapping.Add("euckr", "euc-kr");
         mapping.Add("euc_kr", "euc-kr");
         mapping.Add("cp866", "cp866");
         mapping.Add("Cp852", "ibm852");
         mapping.Add("Cp850", "ibm850");
         mapping.Add("win1251ukr", "windows-1251");
         mapping.Add("cp1251csas", "windows-1251");
         mapping.Add("cp1251cias", "windows-1251");
         mapping.Add("win1251", "windows-1251");
         mapping.Add("cp1257", "windows-1257");
         mapping.Add("gbk", "gb2312");
         mapping.Add("koi8_ru", "koi8-u");
         mapping.Add("koi8r", "koi8-u");
         mapping.Add("dos", "ibm437");
         mapping.Add("ujis", "EUC-JP");
         mapping.Add("eucjpms", "EUC-JP");
         mapping.Add("ascii", "us-ascii");
         mapping.Add("usa7", "us-ascii");
         mapping.Add("binary", "us-ascii");
         mapping.Add("macroman", "x-mac-romanian");
         mapping.Add("macce", "x-mac-ce");
		}
	}
}


