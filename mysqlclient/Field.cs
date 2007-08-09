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
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Types;

namespace MySql.Data.MySqlClient
{
	internal enum ColumnFlags : int
	{
		NOT_NULL = 1,
		PRIMARY_KEY = 2,
		UNIQUE_KEY = 4,
		MULTIPLE_KEY = 8,
		BLOB = 16,
		UNSIGNED = 32,
		ZERO_FILL = 64,
		BINARY = 128,
		ENUM = 256,
		AUTO_INCREMENT = 512,
		TIMESTAMP = 1024,
		SET = 2048,
		NUMBER = 32768
	};

	/// <summary>
	/// Summary description for Field.
	/// </summary>
	internal class MySqlField
	{
		#region Fields

		// public fields
		public string CatalogName;
		public int ColumnLength;
		public string ColumnName;
		public string OriginalColumnName;
		public string TableName;
		public string RealTableName;
		public string DatabaseName;
		public Encoding Encoding;
		public int maxLength;

		// protected fields
		protected ColumnFlags colFlags;
		protected int charSetIndex;
		protected byte precision;
		protected byte scale;
		protected MySqlDbType mySqlDbType;
		protected DBVersion connVersion;

		#endregion

		public MySqlField(DBVersion connVersion)
		{
			this.connVersion = connVersion;
			maxLength = 1;
		}

		#region Properties

		public int CharacterSetIndex
		{
			get { return charSetIndex; }
			set { charSetIndex = value; }
		}

		public MySqlDbType Type
		{
			get { return mySqlDbType; }
			set { mySqlDbType = value; }
		}

		public byte Precision
		{
			get { return precision; }
			set { precision = value; }
		}

		public byte Scale
		{
			get { return scale; }
			set { scale = value; }
		}

		public int MaxLength
		{
			get { return maxLength; }
			set { maxLength = value; }
		}

		public ColumnFlags Flags
		{
			get { return colFlags; }
			set { colFlags = value; }
		}

		public bool IsAutoIncrement
		{
			get { return (colFlags & ColumnFlags.AUTO_INCREMENT) > 0; }
		}

		public bool IsNumeric
		{
			get { return (colFlags & ColumnFlags.NUMBER) > 0; }
		}

		public bool AllowsNull
		{
			get { return (colFlags & ColumnFlags.NOT_NULL) == 0; }
		}

		public bool IsUnique
		{
			get { return (colFlags & ColumnFlags.UNIQUE_KEY) > 0; }
		}

		public bool IsPrimaryKey
		{
			get { return (colFlags & ColumnFlags.PRIMARY_KEY) > 0; }
		}

		public bool IsBlob
		{
			get { return (colFlags & ColumnFlags.BLOB) > 0; }
		}

		public bool IsBinary
		{
			get 
			{ 
				if (connVersion.isAtLeast(4,1,0))
					return (CharacterSetIndex == 63);
				return (colFlags & ColumnFlags.BINARY) > 0; 
			}
		}

		public bool IsUnsigned
		{
			get { return (colFlags & ColumnFlags.UNSIGNED) > 0; }
		}

		public bool IsTextField
		{
			get
			{
				return Type == MySqlDbType.VarString || Type == MySqlDbType.VarChar ||
					 ((Type == MySqlDbType.TinyBlob || Type == MySqlDbType.MediumBlob ||
						Type == MySqlDbType.Blob || Type == MySqlDbType.LongBlob) &&
						!IsBinary);
			}

		}

		#endregion

		public void SetTypeAndFlags(MySqlDbType type, ColumnFlags flags)
		{
			colFlags = flags;
			mySqlDbType = type;

			// if our type is an unsigned number, then we need
			// to bump it up into our unsigned types
			// we're trusting that the server is not going to set the UNSIGNED
			// flag unless we are a number
			if (IsUnsigned)
			{
				switch (type)
				{
					case MySqlDbType.Byte:
						mySqlDbType = MySqlDbType.UByte; break;
					case MySqlDbType.Int16:
						mySqlDbType = MySqlDbType.UInt16; break;
					case MySqlDbType.Int24:
						mySqlDbType = MySqlDbType.UInt24; break;
					case MySqlDbType.Int32:
						mySqlDbType = MySqlDbType.UInt32; break;
					case MySqlDbType.Int64:
						mySqlDbType = MySqlDbType.UInt64; break;
				}
			}

			if (IsBlob && !IsBinary)
			{
				if (type == MySqlDbType.TinyBlob)
					mySqlDbType = MySqlDbType.TinyText;
				else if (type == MySqlDbType.MediumBlob)
					mySqlDbType = MySqlDbType.MediumText;
				else if (type == MySqlDbType.Blob)
					mySqlDbType = MySqlDbType.Text;
				else if (type == MySqlDbType.LongBlob)
					mySqlDbType = MySqlDbType.LongText;
			}
		}

		public MySqlDbType ProviderType()
		{
			if (IsUnsigned)
			{
				switch (Type)
				{
					case MySqlDbType.Byte: return MySqlDbType.UByte;
					case MySqlDbType.Int16: return MySqlDbType.UInt16;
					case MySqlDbType.Int24: return MySqlDbType.UInt24;
					case MySqlDbType.Int32: return MySqlDbType.UInt32;
					case MySqlDbType.Int64: return MySqlDbType.UInt64;
				}
			}
			return Type;
		}

		public MySqlValue GetValueObject()
		{
			MySqlValue valueObject = MySqlValue.GetMySqlValue(ProviderType());
			return valueObject;
		}


	}

}
