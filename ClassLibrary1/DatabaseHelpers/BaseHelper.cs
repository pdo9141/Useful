using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace ClassLibrary1.DatabaseHelpers
{
    internal class BaseHelper
    {
        private static string _connectionString;
        private static int _commandTimeout;

        static BaseHelper()
        {
            _connectionString = "";
            _commandTimeout = 300000;
        }

        public delegate T HydrateEntityDelegate<T>(SqlDataReader reader);

        internal static SqlHelper GetSqlHelper()
        {
            return null;
        }

        internal static T[] GetEntities<T>(SqlHelper sqlHelper, string procName, HydrateEntityDelegate<T> hydrateEntityDelegate, params object[] parameterValues)
        {
            List<T> result = new List<T>();
            if (sqlHelper == null) sqlHelper = GetSqlHelper();

            using (SqlDataReader reader = sqlHelper.ExecuteReader(procName, parameterValues))
                while (reader.Read())
                    result.Add(hydrateEntityDelegate(reader));

            return result.ToArray();
        }

        internal static T GetEntity<T>(SqlHelper sqlHelper, string procName, HydrateEntityDelegate<T> hydrateEntityDelegate, params object[] parameterValues)
        {
            T result = default(T);
            if (sqlHelper == null) sqlHelper = GetSqlHelper();

            using (SqlDataReader reader = sqlHelper.ExecuteReader(procName, parameterValues))
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    result = hydrateEntityDelegate(reader);
                }
            }

            return result;
        }

        internal static T CreateEntity<T>(SqlHelper sqlHelper, string procName, params object[] parameterValues)
        {
            T result = default(T);
            if (sqlHelper == null) sqlHelper = GetSqlHelper();

            var createResult = sqlHelper.ExecuteScalar(procName, parameterValues);
            if (createResult != null)
                result = (T)Convert.ChangeType(createResult, typeof(T));
            
            return result;
        }

        internal static void ExecuteBatch(SqlTransaction transaction, string procName, DataTable batch)
        {
            using (var command = new SqlCommand(procName, transaction.Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = _commandTimeout;
                command.Transaction = transaction;

                var parameter = command.Parameters.AddWithValue("@Batch", batch);
                parameter.SqlDbType = SqlDbType.Structured;

                command.ExecuteNonQuery();
            }
        }
    }
}
