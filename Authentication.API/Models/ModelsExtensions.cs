namespace Authentication.API.Models
{
    using System;
    using System.Linq;
    using Authentication.API.CustomIdentity;

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
    }
}
