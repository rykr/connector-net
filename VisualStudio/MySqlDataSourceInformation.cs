// Copyright (C) 2006-2007 MySQL AB
//
// This file is part of MySQL Tools for Visual Studio.
// MySQL Tools for Visual Studio is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public 
// License version 2.1 as published by the Free Software Foundation
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA using System;

/*
 * This file contains class with data source specific information.
 */

using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;
using MySql.Data.VisualStudio.Properties;

namespace MySql.Data.VisualStudio
{
	/// <summary>
	/// Represents a custom data source information class for MySQL
	/// </summary>
	public class MySqlDataSourceInformation : AdoDotNetDataSourceInformation
	{
        #region Properties names
        public const string DataSource = "DataSource";        
        #endregion

        #region Constructor
        /// <summary>
        /// Constructors fills available properties information
        /// </summary>
        /// <param name="connection">Reference to database connection object</param>
        public MySqlDataSourceInformation(DataConnection connection)
            : base(connection)
        {
            AddProperty(DataSource);
            AddProperty(DefaultSchema);
            AddProperty(SupportsAnsi92Sql, true);
            AddProperty(SupportsQuotedIdentifierParts, true);
            AddProperty(IdentifierOpenQuote, "`");
            AddProperty(IdentifierCloseQuote, "`");
            AddProperty(ServerSeparator, ".");
            AddProperty(CatalogSupported, false);
            AddProperty(CatalogSupportedInDml, false);
            AddProperty(SchemaSupported, true);
            AddProperty(SchemaSupportedInDml, true);
            AddProperty(SchemaSeparator, ".");
            AddProperty(ParameterPrefix, "?");
            AddProperty(ParameterPrefixInName, true);
        } 
        #endregion

        #region Value retrieving
        /// <summary>
        /// Called to retrieve property value. Supports following custom properties:
        /// DataSource � MySQL server name.
        /// Database � default schema name.
        /// </summary>
        /// <param name="propertyName">Name of property to retrieve.</param>
        /// <returns>Property value</returns>
        protected override object RetrieveValue(string propertyName)
        {
            if (propertyName.Equals(DataSource, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConnectionWrapper.ServerName;
            }
            else if (propertyName.Equals(DefaultSchema, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConnectionWrapper.Schema;
            }
            return base.RetrieveValue(propertyName);
        } 
        #endregion

        #region Connection wrapper
        /// <summary>
        /// Returns wrapper for the underlying connection. Creates it at the first call.
        /// </summary>
        private DataConnectionWrapper ConnectionWrapper
        {
            get
            {
                if (connectionWrapperRef == null)
                    connectionWrapperRef = new DataConnectionWrapper(Connection);
                return connectionWrapperRef;
            }
        }
        /// <summary>
        /// Used to stroe connection wrapper.
        /// </summary>
        private DataConnectionWrapper connectionWrapperRef;
        #endregion
	}
}
