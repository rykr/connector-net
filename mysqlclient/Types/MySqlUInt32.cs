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

namespace MySql.Data.Types
{

	internal struct MySqlUInt32 : IMySqlValue
	{
		private uint	mValue;
		private	bool	isNull;
		private bool	is24Bit;

		private MySqlUInt32(MySqlDbType type) 
		{
			is24Bit = type == MySqlDbType.Int24 ? true : false;
			isNull = true;
			mValue = 0;
		}

		public MySqlUInt32(MySqlDbType type, bool isNull) : this(type)
		{
			this.isNull = isNull;
		}

		public MySqlUInt32(MySqlDbType type, uint val) : this(type)
		{
			this.isNull = false;
			mValue = val;
		}

		#region IMySqlValue Members

		public bool IsNull
		{
			get { return isNull; }
		}

		public MySql.Data.MySqlClient.MySqlDbType MySqlDbType
		{
			get	{ return MySqlDbType.UInt32; }
		}

		public System.Data.DbType DbType
		{
			get	{ return DbType.UInt32; }
		}

		object IMySqlValue.Value 
		{
			get { return mValue; }
		}

		public uint Value
		{
			get { return mValue; }
		}

		public Type SystemType
		{
			get	{ return typeof(UInt32); }
		}

		public string MySqlTypeName
		{
			get	{ return is24Bit ? "MEDIUMINT" : "INT"; }
		}

		void IMySqlValue.WriteValue(MySqlStreamWriter writer, bool binary, object v, int length)
		{
			uint val = Convert.ToUInt32( v );
			if (binary)
				writer.Write( BitConverter.GetBytes( val ) );
			else
				writer.WriteStringNoNull( val.ToString() );		
		}

		IMySqlValue IMySqlValue.ReadValue(MySqlStreamReader reader, long length, bool nullVal)
		{
			if (nullVal) return new MySqlUInt32(MySqlDbType, true);

			if (length == -1) 
				return new MySqlUInt32(MySqlDbType, (uint)reader.ReadInteger(is24Bit ? 3 : 4));
			else 
				return new MySqlUInt32(MySqlDbType, UInt32.Parse(reader.ReadString( length )));
		}

		void IMySqlValue.SkipValue(MySqlStreamReader reader)
		{
			reader.SkipBytes(4);
		}

		#endregion

	}


	/// <summary>
	/// Summary description for MySqlInt32.
	/// </summary>
/*	internal class MySqlUInt32Old : MySqlValue
	{
		private uint	mValue;

		public MySqlUInt32Old(MySqlDbType type) : base()
		{
			mySqlDbType = type;
		}

		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			uint v = Convert.ToUInt32( value );
			if (binary)
				writer.Write( BitConverter.GetBytes( v ) );
			else
				writer.WriteStringNoNull( v.ToString() );
		}

		public uint Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value; }
		}

		internal override Type SystemType
		{
			get { return typeof(UInt32); }
		}

		internal override string GetMySqlTypeName()
		{
			return "INT";
		}

		internal override MySqlValue ReadValue( PacketReader reader, long length )
		{
			if (length == -1) 
			{
				if (mySqlDbType == MySqlDbType.Int24)
					Value = (uint)reader.ReadInteger( 3 );
				else
					Value = (uint)reader.ReadLong( 4  );
			}
			else 
			{
				string value = reader.ReadString( length );
				Value = UInt32.Parse( value );
			}
			return this;
		}

		internal override void Skip(PacketReader reader)
		{
			reader.Skip(4);
		}
	}*/
}
