﻿using Bygdrift.Tools.LogTool;
using Microsoft.Data.SqlClient;
using RepoDb;
using System;

namespace Bygdrift.Tools.MssqlTool
{
    /// <summary>
    /// Access to edit Microsoft SQL database data
    /// </summary>
    public partial class Mssql : IDisposable
    {
        private SqlConnection _connection;
        private string _connectionString;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schemaName"></param>
        /// <param name="log"></param>
        public Mssql(string connectionString, string schemaName, Log log)
        {
            ConnectionString = connectionString;
            SchemaName = schemaName;
            Log = log;
        }

        /// <summary>
        /// MS Sql connection
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                Dispose();
            }
        }

        /// <summary>
        /// The schema name
        /// </summary>
        public string SchemaName { get; }

        /// <summary>
        /// Info about excecution
        /// </summary>
        public Log Log { get; }


        /// <summary>
        /// The MS SQL connection
        /// </summary>
        public SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    if (string.IsNullOrEmpty(ConnectionString))
                    {
                        Log.LogError("The database connectionString, has to be set.");
                        throw new ArgumentNullException("The database connectionString, has to be set.");
                    }

                    SqlServerBootstrap.Initialize();
                    var builder = new SqlConnectionStringBuilder(ConnectionString)
                    {
                        ConnectTimeout = 30,
                        CommandTimeout = 3600
                    };

                    _connection = new SqlConnection(builder.ToString());
                }
                if (_connection.State == System.Data.ConnectionState.Closed)
                    try
                    {
                        _connection.Open();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Could not login to MSSql database.", e);
                    }

                return _connection;
            }
            set { _connection = value; }
        }

        /// <summary>
        /// Necesary if there has been any alterings
        /// </summary>
        public void FlushRepoDb()
        {
            DbFieldCache.Flush(); // Remove all the cached DbField
            FieldCache.Flush(); // Remove all the cached DbField
            IdentityCache.Flush(); // Remove all the cached DbField
            PrimaryCache.Flush(); // Remove all the cached DbField
        }


        /// <summary>
        /// Called when disposing
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
