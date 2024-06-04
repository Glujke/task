﻿using Microsoft.EntityFrameworkCore;
using System.Data;
using Task.Connector.Models;
using Task.Connector.Repository;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private string _connectionString;
        public void StartUp(string connectionString)
        {
            var str = connectionString.Split("ConnectionString=\'");
            var st = str[1].Split("\'");
            _connectionString = st[0];
        }

        public void CreateUser(UserToCreate user)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
            {
                User usr = User.AddUser(user);
                db.Users.Add(usr);
                Password pass = new Password() { Password1 = user.HashPassword, UserId = usr.Login };
                db.Passwords.Add(pass);
                db.SaveChanges();
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
            {
                var properties = new List<Property>();
                    properties.Add(new Property("lastName", "Фамилия"));
                    properties.Add(new Property("firstName", "Имя"));
                    properties.Add(new Property("midleName", "Отчество"));
                    properties.Add(new Property("telephonenumber", "Номер телефона"));
                    properties.Add(new Property("isLead", "Лидерство"));
                    properties.Add(new Property("password", "Пароль"));

                return properties;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
            {
                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);
                var properties = new List<UserProperty>();
                if (user.LastName != null && user.LastName != "") properties.Add(new UserProperty("lastName", user.LastName));
                if (user.FirstName != null && user.FirstName != "") properties.Add(new UserProperty("firstName", user.FirstName));
                if (user.MiddleName != null && user.MiddleName != "") properties.Add(new UserProperty("midleName", user.MiddleName));
                if (user.TelephoneNumber != null && user.TelephoneNumber != "") properties.Add(new UserProperty("telephonenumber", user.TelephoneNumber));
                if (user.IsLead != null) properties.Add(new UserProperty("isLead", user.IsLead ? "true" : "false"));

                return properties;
            }
        }

        public bool IsUserExists(string userLogin)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
            {
                return db.Users.Any(u => u.Login == userLogin);
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
            {
                var user = db.Users.FirstOrDefault(u => u.Login == userLogin);
                var lastname = properties.FirstOrDefault(u => u.Name.ToLower() == "lastname")?.Value.ToString();
                var firstname = properties.FirstOrDefault(u => u.Name.ToLower() == "firstname")?.Value.ToString();
                var middleName = properties.FirstOrDefault(u => u.Name.ToLower() == "middleame")?.Value.ToString();
                var telePhoneNumber = properties.FirstOrDefault(u => u.Name.ToLower() == "telephonenumber")?.Value.ToString();
                var isLead = properties.FirstOrDefault(u => u.Name.ToLower() == "islead")?.Value.ToLower() == "true";
                if (lastname != null && lastname != "lastname") user.LastName = lastname;
                if (firstname != null && firstname != "firstname") user.FirstName = firstname;
                if (middleName != null && middleName != "midlename") user.MiddleName = middleName;
                if (telePhoneNumber != null && middleName != "telephonenumber") user.TelephoneNumber = telePhoneNumber;
                if (properties.Any(p => p.Name.ToLower() == "islead")) user.IsLead = isLead;
                db.Users.Update(user);
                db.SaveChanges();
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var allPermissions = new List<Permission>();
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
            {
                var roles = db.ItRoles.ToList();
                var rights = db.RequestRights.ToList();
                foreach(var role in roles)
                {
                    allPermissions.Add(new Permission(role.Id.ToString(), role.Name, $"Role"));
                }
                foreach (var right in rights)
                {
                    allPermissions.Add(new Permission(right.Id.ToString(), right.Name, $"Request"));
                }
                return allPermissions;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach(var rightId in rightIds)
            {
                var splitString = rightId.Split(":");
                if (splitString[0] == "Role")
                {
                    var id = int.Parse(splitString[1]);
                    var userItRole = new UserItrole()
                    {
                        RoleId = id,
                        UserId = userLogin
                    }; 
                    var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
                    optionsBuilder.UseSqlServer(_connectionString);
                    using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
                    {
                        db.UserItroles.Add(userItRole);
                        db.SaveChanges();
                    }
                
                } else if (splitString[0] == "Request")
                {
                    var id = int.Parse(splitString[1]);
                    var userItRole = new UserRequestRight()
                    {
                        RightId = id,
                        UserId = userLogin
                    };
                    var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
                    optionsBuilder.UseSqlServer(_connectionString);
                    using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
                    {
                        db.UserRequestRights.Add(userItRole);
                    }
                }
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            foreach (var rightId in rightIds)
            {
                var splitString = rightId.Split(":");
                if (splitString[0] == "Role")
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
                    optionsBuilder.UseSqlServer(_connectionString);
                    var id = int.Parse(splitString[1]);
                    using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
                    {
                        var userItRole = db.UserItroles.Where(u => userLogin == u.UserId && u.RoleId == id);
                        db.UserItroles.RemoveRange(userItRole);
                        db.SaveChanges();
                    }

                }
                else if (splitString[0] == "Request")
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
                    optionsBuilder.UseSqlServer(_connectionString);
                    var id = int.Parse(splitString[1]);
                    using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
                    {
                        var userItRole = db.UserRequestRights.Where(u => userLogin == u.UserId && u.RightId == id);
                        db.UserRequestRights.RemoveRange(userItRole);
                        db.SaveChanges();
                    }
                }
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var allUserPermissions = new List<string>();

            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (TestDbContext db = new TestDbContext(optionsBuilder.Options))
            {
                var roles = db.UserItroles.Where(u=> u.UserId == userLogin).ToList();
                var rights = db.UserRequestRights.Where(u => u.UserId == userLogin).ToList();
                foreach (var role in roles)
                {
                    var name = db.ItRoles.Where(u => u.Id == role.RoleId).SingleOrDefault();
                    allUserPermissions.Add(name.Name);
                }
                foreach (var right in rights)
                {
                    var name = db.RequestRights.Where(u => u.Id == right.RightId).SingleOrDefault();
                    allUserPermissions.Add(name.Name);
                }
                return allUserPermissions;
            }
        }

        public ILogger Logger { get; set; }
    }
}