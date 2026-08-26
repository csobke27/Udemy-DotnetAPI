using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;

namespace DotnetAPI.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(UserComplete user)
        {
            string sql = @"EXEC TutorialAppSchema.spUser_Upsert
                       @FirstName = @FirstNameParam,
                       @LastName = @LastNameParam,
                       @Email = @EmailParam,
                       @Gender = @GenderParam,
                       @Active = @ActiveParam,
                       @JobTitle = @JobTitleParam,
                       @Department = @DepartmentParam,
                       @Salary = @SalaryParam,
                       @UserId = @UserIdParam;";
            var parameters = new DynamicParameters();
            parameters.Add("FirstNameParam", user.FirstName);
            parameters.Add("LastNameParam", user.LastName);
            parameters.Add("EmailParam", user.Email);
            parameters.Add("GenderParam", user.Gender);
            parameters.Add("ActiveParam", user.Active);
            parameters.Add("JobTitleParam", user.JobTitle);
            parameters.Add("DepartmentParam", user.Department);
            parameters.Add("SalaryParam", user.Salary);
            parameters.Add("UserIdParam", user.UserId);

            return _dapper.ExecuteSQL(sql, parameters);
        }

    }
}