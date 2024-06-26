﻿using System.Reflection;
using Task.Connector.Attributes;
using Task.Connector.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.DataHandlers
{
    internal class UserHandler
    {

        public (User usr, Password pass) GetDataUser(UserToCreate userToCreate)
        {
            var user = GetUserFrom(userToCreate);
            var password = GetPasswordFrom(userToCreate);
            return (user, password);
        }

        public UserToCreate GetUserToCreate(User user, Password password)
        {
            return GetUserToCreateFromUser(user, password);
        }

        private User GetUserFrom(UserToCreate userToCreate)
        {
            var userProps = userToCreate.Properties;
            var user = new User()
            {
                Login = userToCreate.Login
            };
            SetUserProperties(user, userProps);
            return user;
        }

        private UserToCreate GetUserToCreateFromUser(User user, Password password)
        {
            var userToCreate = new UserToCreate(user.Login, password.Password1);
            userToCreate.Properties = GetUserPropertiesFromUser(user);
            return userToCreate;
        }

        public IEnumerable<UserProperty> GetUserPropertiesFromUser(User user)
        {
            var properties = new List<UserProperty>();
            var props = user.GetType().GetProperties().Where(p => p.GetCustomAttribute<PropertyAttribute>() != null);
            foreach (var property in props)
            {
                var propertyAttributes = property.GetCustomAttributes(typeof(PropertyAttribute), false);
                foreach (var attribute in propertyAttributes)
                {
                    var propertyAttribute = attribute as PropertyAttribute;
                    if (property.PropertyType == typeof(string))
                    {
                        properties.Add(new UserProperty(propertyAttribute.Name, property.GetValue(user) as string));
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        var value = (bool)property.GetValue(user) ? "true" : "false";
                        properties.Add(new UserProperty(propertyAttribute.Name, value));
                    }
                    else throw new Exception("Указан неподдерживаемый тип данных.");
                }
            }
            return properties;
        }

        public void SetUserProperties(User user, IEnumerable<UserProperty> userProps)
        {
            var props = user.GetType().GetProperties().Where(p => p.GetCustomAttribute<PropertyAttribute>() != null);
            foreach (var property in props)
            {
                var propertyAttributes = property.GetCustomAttributes(typeof(PropertyAttribute), false);
                foreach (var attribute in propertyAttributes)
                {
                    var propertyAttribute = attribute as PropertyAttribute;
                    if (propertyAttribute != null)
                    {
                        var value = userProps.Where(u => u.Name == propertyAttribute.Name).SingleOrDefault();
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(user, value != null ? value.Value : propertyAttribute.DefaultValue);
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            if (value == null) value = new UserProperty("", propertyAttribute.DefaultValue);
                            property.SetValue(user, value.Value == "false" ? false : true);
                        }
                    }
                }
            }
        }

        private Password GetPasswordFrom(UserToCreate userToCreate)
        {
            return new Password() { UserId = userToCreate.Login, Password1 = userToCreate.HashPassword };
        }
    }
}
