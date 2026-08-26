using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
    public class DataContextDapper
    {
        private readonly IConfiguration _config;
        public DataContextDapper(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }

        public T? LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingleOrDefault<T>(sql);
        }

        public IEnumerable<T> LoadDataWithParams<T>(string sql, object? parameters = null)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql, parameters);
        }

        public T? LoadDataSingleWithParams<T>(string sql, object? parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingleOrDefault<T>(sql, parameters);
        }

        public bool ExecuteSQL(string sql, object? parameters)
        {
            using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return dbConnection.Execute(sql, parameters) > 0;
            }
        }

        public int ExecuteSQLWithRowCount(string sql)
        {
            using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return dbConnection.Execute(sql);
            }
        } 

        public void ExecuteProcedureMulti(string sql, IDbConnection dbConnection)
        {
            dbConnection.Execute(sql);
        }

        public string TrimEndComma(string sql)
        {
            if(sql.EndsWith(","))
            {
                sql = sql.TrimEnd(',');
            }
            return sql;
        }

    }
}