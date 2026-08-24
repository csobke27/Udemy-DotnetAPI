using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContextEF _entityFramework;
        public UserRepository(IConfiguration configuration)
        {
            _entityFramework = new DataContextEF(configuration);
        }

        public bool SaveChanges()
        {
            return _entityFramework.SaveChanges() > 0;
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if(entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);
            }
        }

        public void RemoveEntity<T>(T entityToRemove)
        {
            if(entityToRemove != null)
            {
                _entityFramework.Remove(entityToRemove);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            var result = _entityFramework.Users.ToList();
            return result;
        }

        public User GetSingleUser(int userId)
        {
            User? user = _entityFramework.Users.FirstOrDefault(u => u.UserId == userId);
            if(user == null)
            {
                throw new Exception("User not found.");
            }
            return user;
        }

        public UserSalary GetSingleUserSalary(int userId)
        {
            UserSalary? userSalary = _entityFramework.UserSalary.FirstOrDefault(u => u.UserId == userId);
            if(userSalary == null)
            {
                throw new Exception("User salary not found.");
            }
            return userSalary;
        }

        public UserJobInfo GetSingleUserJobInfo(int userId)
        {
            UserJobInfo? userJobInfo = _entityFramework.UserJobInfo.FirstOrDefault(u => u.UserId == userId);
            if(userJobInfo == null)
            {
                throw new Exception("User job info not found.");
            }
            return userJobInfo;
        }
    }
}