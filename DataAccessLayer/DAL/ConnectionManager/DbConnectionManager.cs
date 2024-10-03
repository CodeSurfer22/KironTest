using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace DAL
{
    public class DbConnectionManager : IDisposable
    {
        private static readonly ConcurrentBag<SqlConnection> _connectionPool = new ConcurrentBag<SqlConnection>();
        private static readonly object _lock = new object();
        private static int _openConnections = 0;
        private readonly SqlConnection _connection;
        private readonly string _connectionString;
        private bool _disposed = false;

        public DbConnectionManager(string connectionString)
        {
            _connectionString = connectionString;

            // Enforce a maximum of 10 open connections at a time
            lock (_lock)
            {
                if (_openConnections >= 10)
                {
                    throw new InvalidOperationException("Maximum number of open connections (10) reached.");
                }

                _openConnections++;
            }

            _connection = GetOrCreateConnection();
        }

        // Retrieves a connection from the pool or creates a new one
        private SqlConnection GetOrCreateConnection()
        {
            if (_connectionPool.TryTake(out SqlConnection connection))
            {
                return connection;
            }

            return new SqlConnection(_connectionString);
        }

        // Returns the active connection to the caller
        public SqlConnection GetConnection()
        {

            if (_connection.State != ConnectionState.Open)
            {
                _connection.ConnectionString = _connectionString;
                _connection.Open();
            }


            return _connection;
        }

        // Dispose of the connection when done
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }

                // Return the connection to the pool
                _connectionPool.Add(_connection);

                lock (_lock)
                {
                    _openConnections--;
                }

                _disposed = true;
            }
        }
    }
}
