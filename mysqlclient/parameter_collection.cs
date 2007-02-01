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
using System.Collections;
using System.ComponentModel;
using System.Globalization;
#if NET20
using System.Collections.Generic;
#else
using System.Collections.Specialized;
#endif

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Represents a collection of parameters relevant to a <see cref="MySqlCommand"/> as well as their respective mappings to columns in a <see cref="DataSet"/>. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlParameterCollection.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
	[Editor("MySql.Data.MySqlClient.Design.DBParametersEditor,MySql.Design", typeof(System.Drawing.Design.UITypeEditor))]
	[ListBindable(true)]
	public sealed class MySqlParameterCollection : MarshalByRefObject, IDataParameterCollection,
		IList, ICollection, IEnumerable
	{
		private ArrayList _parms = new ArrayList();
		private char paramMarker = '?';
		private Hashtable ciHash;
		private Hashtable hash;
        private int returnParameterIndex;

		internal MySqlParameterCollection()
		{
			hash = new Hashtable();
#if NET20
			ciHash = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
#else
			ciHash = new Hashtable(new CaseInsensitiveHashCodeProvider(),
				new CaseInsensitiveComparer());
#endif
            Clear();
		}

		internal char ParameterMarker
		{
			get { return paramMarker; }
			set { paramMarker = value; }
		}

		private int InternalIndexOf(string name)
		{
			int index = IndexOf(name);
			if (index != -1) return index;

			// we have failed to find a parameter with the given name.
			// We now check to see if the user gave us the parameter without a marker.
			if (name.StartsWith(ParameterMarker.ToString()))
			{
				string newName = name.Substring(1);
				index = IndexOf(newName);
				if (index != -1)
					throw new ArgumentException(String.Format(Resources.WrongParameterName,
						name, newName));
			}
			throw new MySqlException("A MySqlParameter with ParameterName '" + name +
				"' is not contained by this MySqlParameterCollection.");
		}

		#region ICollection support

		/// <summary>
		/// Gets the number of MySqlParameter objects in the collection.
		/// </summary>
		public int Count
		{
			get { return _parms.Count; }
		}

		/// <summary>
		/// Copies MySqlParameter objects from the MySqlParameterCollection to the specified array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(Array array, int index)
		{
			_parms.CopyTo(array, index);
		}

		bool ICollection.IsSynchronized
		{
			get { return _parms.IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return _parms.SyncRoot; }
		}
		#endregion

		#region IList

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		public void Clear()
		{
            foreach (MySqlParameter p in _parms)
                p.Collection = null;

			_parms.Clear();
			hash.Clear();
			ciHash.Clear();
            returnParameterIndex = -1;
		}

		/// <summary>
		/// Gets a value indicating whether a MySqlParameter exists in the collection.
		/// </summary>
		/// <param name="value">The value of the <see cref="MySqlParameter"/> object to find. </param>
		/// <returns>true if the collection contains the <see cref="MySqlParameter"/> object; otherwise, false.</returns>
		/// <overloads>Gets a value indicating whether a <see cref="MySqlParameter"/> exists in the collection.</overloads>
		public bool Contains(object value)
		{
			return _parms.Contains(value);
		}

		/// <summary>
		/// Gets the location of a <see cref="MySqlParameter"/> in the collection.
		/// </summary>
		/// <param name="value">The <see cref="MySqlParameter"/> object to locate. </param>
		/// <returns>The zero-based location of the <see cref="MySqlParameter"/> in the collection.</returns>
		/// <overloads>Gets the location of a <see cref="MySqlParameter"/> in the collection.</overloads>
		public int IndexOf(object value)
		{
			return _parms.IndexOf(value);
		}

		/// <summary>
		/// Inserts a MySqlParameter into the collection at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public void Insert(int index, object value)
		{
			_parms.Insert(index, value);
		}

		bool IList.IsFixedSize
		{
			get { return _parms.IsFixedSize; }
		}

		bool IList.IsReadOnly
		{
			get { return _parms.IsReadOnly; }
		}

		/// <summary>
		/// Removes the specified MySqlParameter from the collection.
		/// </summary>
		/// <param name="value"></param>
		public void Remove(object value)
		{
			_parms.Remove(value);

            MySqlParameter p = (value as MySqlParameter);
            hash.Remove(p.ParameterName);
            ciHash.Remove(p.ParameterName);
            p.Collection = null;
            if (p.Direction == ParameterDirection.ReturnValue)
                returnParameterIndex = -1;
		}

		/// <summary>
		/// Removes the specified <see cref="MySqlParameter"/> from the collection using a specific index.
		/// </summary>
		/// <param name="index">The zero-based index of the parameter. </param>
		/// <overloads>Removes the specified <see cref="MySqlParameter"/> from the collection.</overloads>
		public void RemoveAt(int index)
		{
            MySqlParameter p = this[index];
            Remove(p);
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set
			{
				if (!(value is MySqlParameter)) 
                    throw new MySqlException("Only MySqlParameter objects may be stored");
				this[index] = (MySqlParameter)value;
			}
		}

		/// <summary>
		/// Adds the specified <see cref="MySqlParameter"/> object to the <see cref="MySqlParameterCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="MySqlParameter"/> to add to the collection.</param>
		/// <returns>The index of the new <see cref="MySqlParameter"/> object.</returns>
		public int Add(object value)
		{
			if (!(value is MySqlParameter))
				throw new MySqlException("Only MySqlParameter objects may be stored");

			MySqlParameter p = (MySqlParameter)value;

			if (p.ParameterName == null || p.ParameterName == String.Empty)
				throw new MySqlException("Parameters must be named");

			p = Add(p);
			return IndexOf(p);
		}

		#endregion

		#region IDataParameterCollection

		/// <summary>
		/// Gets a value indicating whether a <see cref="MySqlParameter"/> with the specified parameter name exists in the collection.
		/// </summary>
		/// <param name="name">The name of the <see cref="MySqlParameter"/> object to find.</param>
		/// <returns>true if the collection contains the parameter; otherwise, false.</returns>
		public bool Contains(string name)
		{
			return IndexOf(name) != -1;
		}

		/// <summary>
		/// Gets the location of the <see cref="MySqlParameter"/> in the collection with a specific parameter name.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="MySqlParameter"/> object to retrieve. </param>
		/// <returns>The zero-based location of the <see cref="MySqlParameter"/> in the collection.</returns>
		public int IndexOf(string parameterName)
		{
			object o = hash[parameterName];
            if (o != null)
                return (int)o;

			o = ciHash[parameterName];
            if (o != null)
                return (int)o;

            if (returnParameterIndex != -1)
            {
                MySqlParameter p = (MySqlParameter)_parms[returnParameterIndex];
                if (String.Compare(parameterName, p.ParameterName, true) == 0)
                    return returnParameterIndex;
            }
            return -1;
		}

		/// <summary>
		/// Removes the specified <see cref="MySqlParameter"/> from the collection using the parameter name.
		/// </summary>
		/// <param name="name">The name of the <see cref="MySqlParameter"/> object to retrieve. </param>
		public void RemoveAt(string name)
		{
            int index = InternalIndexOf(name);
            RemoveAt(index);
		}

		object IDataParameterCollection.this[string name]
		{
			get { return this[name]; }
			set
			{
				if (!(value is MySqlParameter)) throw new MySqlException("Only MySqlParameter objects may be stored");
				this[name] = (MySqlParameter)value;
			}
		}
		#endregion

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_parms).GetEnumerator();
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the <see cref="MySqlParameter"/> at the specified index.
		/// </summary>
		/// <overloads>Gets the <see cref="MySqlParameter"/> with a specified attribute.
		/// [C#] In C#, this property is the indexer for the <see cref="MySqlParameterCollection"/> class.
		/// </overloads>
		public MySqlParameter this[int index]
		{
			get { return (MySqlParameter)_parms[index]; }
			set 
            {
                MySqlParameter p = (MySqlParameter)_parms[index];
                if (p.Direction == ParameterDirection.ReturnValue)
                    returnParameterIndex = -1;
                else 
                {
                    ciHash.Remove(p.ParameterName);
                    hash.Remove(p.ParameterName);
                }
                _parms[index] = value;
                if (value.Direction == ParameterDirection.ReturnValue)
                    returnParameterIndex = index;
                else
                {
                    ciHash.Add(value.ParameterName, index);
                    hash.Add(value.ParameterName, index);
                }
            }
		}

		/// <summary>
		/// Gets the <see cref="MySqlParameter"/> with the specified name.
		/// </summary>
		public MySqlParameter this[string name]
		{
			get { return (MySqlParameter)_parms[InternalIndexOf(name)]; }
			set { this[InternalIndexOf(name)] = value; }
		}

		/// <summary>
		/// Adds the specified <see cref="MySqlParameter"/> object to the <see cref="MySqlParameterCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="MySqlParameter"/> to add to the collection.</param>
		/// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(MySqlParameter value)
		{
			if (value == null)
				throw new ArgumentException("The MySqlParameterCollection only accepts non-null MySqlParameter type objects.", "value");

			if (value.Direction == ParameterDirection.ReturnValue)
				return AddReturnParameter(value);

			string inComingName = value.ParameterName.ToLower();
			if (inComingName[0] == paramMarker)
				inComingName = inComingName.Substring(1, inComingName.Length - 1);

			for (int i = 0; i < _parms.Count; i++)
			{
				MySqlParameter p = (MySqlParameter)_parms[i];
				string name = p.ParameterName.ToLower();
				if (name[0] == paramMarker)
					name = name.Substring(1, name.Length - 1);
				if (name == inComingName)
				{
					_parms[i] = value;
					return value;
				}
			}

			int index = _parms.Add(value);
			hash.Add(value.ParameterName, index);
			ciHash.Add(value.ParameterName, index);
            value.Collection = this;
			return value;
		}

		private MySqlParameter AddReturnParameter(MySqlParameter value)
		{
            if (returnParameterIndex != -1)
                throw new InvalidOperationException(Resources.ReturnParameterExists);
            else
                returnParameterIndex = _parms.Add(value);
            return value;
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> given the specified parameter name and value.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="value">The <see cref="MySqlParameter.Value"/> of the <see cref="MySqlParameter"/> to add to the collection.</param>
		/// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(string parameterName, object value)
		{
			return Add(new MySqlParameter(parameterName, value));
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> given the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(string parameterName, MySqlDbType dbType)
		{
			return Add(new MySqlParameter(parameterName, dbType));
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> with the parameter name, the data type, and the column length.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the column.</param>
		/// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(string parameterName, MySqlDbType dbType, int size)
		{
			return Add(new MySqlParameter(parameterName, dbType, size));
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> with the parameter name, the data type, the column length, and the source column name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the column.</param>
		/// <param name="sourceColumn">The name of the source column.</param>
		/// <returns>The newly added <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(string parameterName, MySqlDbType dbType, int size, string sourceColumn)
		{
			return Add(new MySqlParameter(parameterName, dbType, size, sourceColumn));
		}

		#endregion

        internal void ParameterNameChanged(MySqlParameter p, string oldName, string newName)
        {
            if (p.Direction == ParameterDirection.ReturnValue)
                return;
            int index = IndexOf(oldName);
            hash.Remove(oldName);
            ciHash.Remove(oldName);
            hash.Add(newName, index);
            ciHash.Add(newName, index);
        }

	}
}
