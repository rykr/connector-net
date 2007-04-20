using System;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace MySql.Data.MySqlClient
{
    internal class MySqlProviderServices : DbProviderServices
    {
        public override DbCommandDefinition CreateCommandDefinition(DbConnection connection, 
            CommandTree commandTree)
        {
            DbCommand prototype = CreateCommand(connection, commandTree);
            DbCommandDefinition result = this.CreateCommandDefinition(prototype);
            return result;
        }

        public override System.Xml.XmlReader GetProviderManifest(DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            return this.GetXmlResource(
                "OrcasSampleProvider.Resources.SampleProviderServices.ProviderManifest");
        }

        private XmlReader GetXmlResource(string resourceName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream stream = executingAssembly.GetManifestResourceStream(resourceName);
            return XmlReader.Create(stream);
        }

        #region Internal methods

        internal DbCommand CreateCommand(DbConnection connection, CommandTree commandTree)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (commandTree == null)
                throw new ArgumentNullException("commandTree");

//            MySqlConnection conn = (MySqlConnection)connection;
            MySqlCommand command = new MySqlCommand();

            List<DbParameter> parameters;
            command.CommandText = MySqlGenerator.GenerateSql(commandTree, out parameters);
            command.CommandType = CommandType.Text;

            // Now make sure we populate the command's parameters from the CQT's parameters:
            foreach (KeyValuePair<string, TypeUsage> queryParameter in commandTree.Parameters)
            {
                DbParameter parameter = CreateParameterFromQueryParameter(queryParameter);
                command.Parameters.Add(parameter);
            }

            // Now add parameters added as part of SQL gen (note: this feature is only 
            // safe for DML SQL gen which does not support user parameters, 
            // where there is no risk of name collision)
            if (null != parameters && 0 < parameters.Count)
            {
                foreach (DbParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        private DbParameter CreateParameterFromQueryParameter(
            KeyValuePair<string, TypeUsage> queryParameter)
        {
            // We really can't have a parameter here that isn't a scalar type...
            Debug.Assert(MetadataHelpers.IsPrimitiveType(queryParameter.Value), "Non-PrimitiveType used as query parameter type");

            DbParameter result = MySqlClientFactory.Instance.CreateParameter();
            result.ParameterName = queryParameter.Key;
            result.Direction = ParameterDirection.Input;

            return result;
        }

        #endregion
    }
}
