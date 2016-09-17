using System;
using System.Data;
using System.Data.SqlClient;

namespace ConsoleApplication1
{
    public static class DBHelper
    {
        public static T GetDBValue<T>(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] != DBNull.Value)
                return (T)Convert.ChangeType(reader[columnName], typeof(T));
            return default(T);
        }

        public static DataTable GetIdDataTable(int[] ids)
        {
            DataTable result = null;

            if (ids != null)
            {
                result = new DataTable();
                result.Columns.Add("ID", typeof(int));
                foreach (int id in ids)
                    result.Rows.Add(new object[] { id });
            }

            return result;
        }

        public static DataTable GetDataTableForBigInt(long[] ids)
        {
            DataTable result = null;

            if (ids != null)
            {
                result = new DataTable();
                result.Columns.Add("ID", typeof(long));
                foreach (long id in ids)
                    result.Rows.Add(new object[] { id });
            }

            return result;
        }

        public static DataTable GetIdDataTable<T>(object[] ids)
        {
            DataTable result = null;

            if (ids != null)
            {
                result = new DataTable();
                result.Columns.Add("ID", typeof(T));
                foreach (T id in ids)
                    result.Rows.Add(new object[] { id });
            }

            return result;
        }
    }
}
