namespace Authentication.API.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Authentication.API.CustomIdentity;
    using Authentication.API.Models;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    [Route("api/identity/user")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly ApplicationUserManager userManager;
        private readonly RoleManager<ApplicationRole> rolesManager;

        public UserController(ApplicationUserManager userManager, RoleManager<ApplicationRole> rolesManager)
        {
            this.userManager = userManager;
            this.rolesManager = rolesManager;
        }

        [HttpGet]
        [Route("{username}")]
        public async Task<IActionResult> GetUserAsync(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return this.SetError($"The parameter [{nameof(username)}] is required !!", "NullParameter", StatusCodes.Status400BadRequest);
                }

                ApplicationUser user;

                if ("me".Equals(username.ToLower()))
                {
                    user = await this.userManager.FindByIdAsync(this.GetCurrentUserId());
                }
                else
                {
                    user = await this.userManager.FindByNameAsync(username);
                }

                if (user is null)
                {
                    return this.SetError($"The user with the [Username:'{username}'] not found!!", "NotFound", StatusCodes.Status404NotFound);
                }

                return this.StatusCode(StatusCodes.Status200OK, user.ConvertToDto());
            }
            catch (Exception ex)
            {
                return this.SetError(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserDto userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.Password))
                {
                    return this.SetError($"The parameter [{nameof(userDto.Password)}] is required !!", "NullParameter", StatusCodes.Status400BadRequest);
                }

                var user = userDto.ConvertFromDto();

                var result = await this.userManager.CreateAsync(user, userDto.Password);
                if (!result.Succeeded)
                {
                    var error = result.Errors.FirstOrDefault();
                    return this.SetError(error.Description, error.Code, StatusCodes.Status500InternalServerError);
                }

                foreach (var item in userDto.Roles)
                {
                    if (!await this.rolesManager.RoleExistsAsync(item))
                    {
                        return this.SetError($"The Role [{item}] doesn't exists!", "RoleNotFound", StatusCodes.Status400BadRequest);
                    }

                    var roleResult = await this.userManager.AddToRoleAsync(user, item);
                    if (!roleResult.Succeeded)
                    {
                        var error = roleResult.Errors.FirstOrDefault();
                        return this.SetError(error.Description, error.Code, StatusCodes.Status500InternalServerError);
                    }
                }

                return this.StatusCode(StatusCodes.Status200OK, user.ConvertToDto());
            }
            catch (Exception ex)
            {
                return this.SetError(ex.Message);
            }
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateUserAsync(string id, [FromBody] UserDto userDto)
        {
            try
            {
                if (userDto is null
                    || string.IsNullOrWhiteSpace(id)
                    || string.IsNullOrWhiteSpace(userDto.Email)
                    || string.IsNullOrWhiteSpace(userDto.Username)
                    || string.IsNullOrWhiteSpace(userDto.Lastname)
                    || string.IsNullOrWhiteSpace(userDto.Firstname))
                {
                    return this.SetError($"The user object has some invalid parameters", "NullParameter", StatusCodes.Status400BadRequest);
                }

                var user = await this.userManager.FindByIdAsync(id);
                if (user is null)
                {
                    return this.SetError($"The user with the [Id:'{id}'] not found!!", "NotFound", StatusCodes.Status404NotFound);
                }

                var result = await this.userManager.UpdateAsync(user.PatchFromDto(userDto));
                if (!result.Succeeded)
                {
                    var error = result.Errors.FirstOrDefault();
                    return this.SetError(error.Description, error.Code, StatusCodes.Status500InternalServerError);
                }

                return this.StatusCode(StatusCodes.Status200OK, user.ConvertToDto());
            }
            catch (Exception ex)
            {
                return this.SetError(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return this.SetError($"The parameter [ID] is required!", "NullParameter", StatusCodes.Status400BadRequest);
                }

                var user = await this.userManager.FindByIdAsync(id);
                if (user is null)
                {
                    return this.SetError($"The user with the [Id:'{id}'] not found!!", "NotFound", StatusCodes.Status404NotFound);
                }

                var result = await this.userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var error = result.Errors.FirstOrDefault();
                    return this.SetError(error.Description, error.Code, StatusCodes.Status500InternalServerError);
                }

                return this.StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                return this.SetError(ex.Message);
            }
        }
    }
}