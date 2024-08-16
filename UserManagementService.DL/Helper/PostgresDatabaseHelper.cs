using System.Data;
using Dapper;
using Npgsql;
using Serilog;

namespace UserManagementService.DL.Helper;

/// <summary>
    /// Helper to query the database.
    /// </summary>
    public class PostgresDatabaseHelper
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<PostgresDatabaseHelper>();

        
        private string _connectionStringWithCredentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDatabaseHelper"/> class.
        /// </summary>
        /// <param name="connectionString">String responsible for creating a connection to the database.</param>
        public PostgresDatabaseHelper(string connectionString)
        {
            _connectionStringWithCredentials = connectionString;
        }

        /// <summary>
        /// Opens the database connection, begins a transaction and runs the specified query and commits upon success asynchronously.
        /// Upon failure, the transaction will rollback.
        /// </summary>
        /// <typeparam name="T">The type returned by the query task.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>A task.</returns>
        public async Task<T> TransactReader<T>(Func<NpgsqlConnection, Task<T>> query)
        {
            using (var connection = new NpgsqlConnection(_connectionStringWithCredentials))
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                T result;
                try
                {
                    result = await query(connection);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error occured while handling transaction.");

                    throw;
                }

                return result;
            }
        }

        /// <summary>
        /// Opens the database connection, begins a transaction and runs the specified query and commits upon success asynchronously.
        /// Upon failure, the transaction will rollback.
        /// </summary>
        /// <typeparam name="T">The type returned by the query task.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>A task.</returns>
        public async Task<T> Transact<T>(Func<NpgsqlConnection, Task<T>> query)
        {
            using (var connection = new NpgsqlConnection(_connectionStringWithCredentials))
            {
                await connection.OpenAsync();
                
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var transaction = connection.BeginTransaction())
                {
                    T result;
                    try
                    {
                        result = await query(connection);
                    }
                    catch (Exception e)
                    {
                        try 
                        {
                            await transaction.RollbackAsync(); // Attempt rollback
                        }
                        catch (Exception rollbackEx)
                        {
                            Log.Error(rollbackEx, "Error during transaction rollback."); 
                        }

                        Log.Error(e, "Error while handling transaction.");

                        throw;
                    }

                    await transaction.CommitAsync();

                    return result;
                }
            }
        }

        /// <summary>
        /// Executes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Task.</returns>
        public async Task Execute(Func<NpgsqlConnection, Task> query)
        {
            using (var connection = new NpgsqlConnection(_connectionStringWithCredentials))
            {
                await connection.OpenAsync();
                
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await query(connection);
                    }
                    catch (Exception e)
                    {
                        try 
                        {
                            await transaction.RollbackAsync(); // Attempt rollback
                        }
                        catch (Exception rollbackEx)
                        {
                            Log.Error(rollbackEx, "Error during transaction rollback."); 
                        }

                        Log.Error(e, "Error while handling transaction.");

                        throw;
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        /// <summary>
        /// Queries the database.
        /// </summary>
        /// <param name="query">The query to perform.</param>
        /// <param name="exception">The exception to throw if execution of this SQL statement was not successful.</param>
        /// <returns>A task.</returns>
        public async Task QueryDatabase(Func<NpgsqlConnection, Task> query, Exception exception = null)
        {
            using (
                var connection =
                new NpgsqlConnection(_connectionStringWithCredentials))
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var transaction = connection.BeginTransaction();

                try
                {
                    await query(connection);
                }
                catch (Exception e)
                {
                    try 
                    {
                        await transaction.RollbackAsync(); // Attempt rollback
                    }
                    catch (Exception rollbackEx)
                    {
                        Log.Error(rollbackEx, "Error during transaction rollback."); 
                    }

                    Log.Error("Error while executing query: {exception}", e);

                    if (exception != null)
                    {
                        throw exception;
                    }

                    throw;
                }

                await transaction.CommitAsync();
            }
        }

        /// <summary>
        /// Queries the database without transaction.
        /// </summary>
        /// <typeparam name="T">The type returned by the query task.</typeparam>
        /// <param name="query">The query to perform.</param>
        /// <param name="exception">The exception to throw if execution of this SQL statement was not successful.</param>
        /// <returns>A task.</returns>
        /// 
        public async Task<T> QueryDatabaseWithResult<T>(Func<NpgsqlConnection, Task<T>> query, Exception exception = null)
        {
            using (
                var connection =
                new NpgsqlConnection(_connectionStringWithCredentials))
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                T result;

                try
                {
                    result = await query(connection);
                }
                catch (Exception e)
                {
                    Log.Error("Error while executing query: {exception}", e);

                    if (exception != null)
                    {
                        throw exception;
                    }

                    throw;
                }

                return result;
            }
        }

        /// <summary>
        /// Executes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="queryParameters">The query parameters.</param>
        /// <returns>Task.</returns>
        public async Task Execute(string query, object queryParameters = null)
        {
            await Transact(async connection => await connection.ExecuteAsync(query, queryParameters));
        }
    }
