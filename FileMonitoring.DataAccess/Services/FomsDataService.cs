using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using System.Linq;

namespace FileMonitoring.DataAccess.Services
{
    public class FomsDataService
    {
        /// <summary>
        /// Адресат запроса
        /// </summary>
        public const string DestinationRequest = "1.2.643.2.40.3.3.1.0";

        private readonly string _connectionString;
        private readonly ILogger _logger;

        public FomsDataService(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Добавляет параметр в команду
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void AddParameter(NpgsqlCommand command, NpgsqlDbType type, string name, object value)
        {
            command.Parameters.Add(name, type);
            command.Parameters[name].Value = value;
        }

        public string GetOkatoByInstitutionCode(string code)
        {
            return ExecuteQuery(command =>
            {
                command.CommandText = "SELECT \"RelationCode\" FROM \"gateway\".\"Concept\" WHERE \"Oid\"=@Oid " +
                                      "AND \"Code\"=@Code";
                AddParameter(command, NpgsqlDbType.Text, "@Oid", DestinationRequest);
                AddParameter(command, NpgsqlDbType.Text, "@Code", code);
                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return result.ToString();

                return string.Empty;
            });
        }

        protected TResult ExecuteQuery<TResult>(Func<NpgsqlCommand, TResult> func)
        {
            var sql = string.Empty;
            NpgsqlParameterCollection sqlParams = null;
            var result = default(TResult);
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        sql = command.CommandText;
                        sqlParams = command.Parameters;
                        command.CommandType = CommandType.Text;
                        result = func(command);
                    }
                }
                catch (NpgsqlException e)
                {
                    var paramMessage = (sqlParams != null) ?
                        $"Параметры {string.Join(',', sqlParams.Select(x => x.Value.ToString().ToArray()))}" :
                        string.Empty;

                    _logger.LogError(e, $"Ошибка при выполнении запроса в базу. " +
                        $"Текст запроса:\n {sql}\n {paramMessage}");
                }
                finally
                {
                    connection.Close();
                }
            }
            return result;
        }
    }
}
