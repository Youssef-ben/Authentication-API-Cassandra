namespace Authentication.API.Models
{
    using System;
    using System.Linq;
    using Authentication.API.CustomIdentity;
    using Microsoft.Extensions.Configuration;

    public static class ModelsExtensions
    {
        public static UserDto ConvertToDto(this ApplicationUser self)
        {
            if (self is null)
            {
                throw new ArgumentNullException($"Error while converting the model to a DTO. Param is empty:[{nameof(ApplicationUser)}]");
            }

            return new UserDto()
            {
                ID = self.Id.ToString(),
                Username = self.UserName,
                Firstname = self.Firstname,
                Lastname = self.Lastname,
                Email = self.Email,
                Phone = self.Phone,
                Roles = self.Roles.ToList(),
            };
        }

        public static ApplicationUser ConvertFromDto(this UserDto self)
        {
            if (self is null)
            {
                throw new ArgumentNullException($"Error while converting the model From DTO. Param is empty:[{nameof(UserDto)}]");
            }

            return new ApplicationUser()
            {
                UserName = self.Username,
                Firstname = self.Firstname,
                Lastname = self.Lastname,
                Email = self.Email,
            };
        }

        public static ApplicationUser ConvertFromDto(this LoginDto self)
        {
            if (self is null)
            {
                throw new ArgumentNullException($"Error while converting the model From DTO. Param is empty:[{nameof(LoginDto)}]");
            }

            return new ApplicationUser()
            {
                UserName = self.Username
            };
        }

        public static ApplicationUser PatchFromDto(this ApplicationUser self, UserDto dto)
        {
            self.UserName = dto.Username;
            self.Email = dto.Email;
            self.Firstname = dto.Firstname;
            self.Lastname = dto.Lastname;

            return self;
        }

        /// <summary>
        /// Extension Used to Create an instance of the specified configuration Section.
        /// </summary>
        /// <typeparam name="TClass">Class to be used for the configuration.</typeparam>
        /// <param name="self">{IConfiguration} The instance of the configuration object.</param>
        /// <param name="section">{String} Section to be mapped</param>
        /// <returns>New Instance of the Specified Section</returns>
        public static TClass GetConfigurationInstance<TClass>(this IConfiguration self, string section)
            where TClass : class, new()
        {
            var instance = new TClass();
            self.Bind(section, instance);
            return instance;
        }
    }
}
