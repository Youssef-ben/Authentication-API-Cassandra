namespace Authentication.API.Controllers
{
    using System;
    using System.Threading.Tasks;
    using AspNetCore.Identity.Cassandra;
    using Authentication.API.Config;
    using Authentication.API.CustomIdentity;
    using Authentication.API.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [Route("api/identity")]
    [ApiController]
    public class AuthenticateController : BaseController
    {
        private readonly ApplicationSignInManager signInManager;
        private readonly ApplicationUserManager userManager;
        private readonly RoleManager<ApplicationRole> rolesManager;

        private readonly IOptionsSnapshot<CassandraOptions> cassandraOptions;
        private readonly IOptionsSnapshot<JwtOptions> jwtOptions;

        public AuthenticateController(IOptionsSnapshot<CassandraOptions> snapshot, IOptionsSnapshot<JwtOptions> jwtOptions, ApplicationSignInManager signInManager, ApplicationUserManager userManager, RoleManager<ApplicationRole> rolesManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.rolesManager = rolesManager;
            this.cassandraOptions = snapshot;
            this.jwtOptions = jwtOptions;
        }

        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignInAsync([FromBody] LoginDto login)
        {
            try
            {
                if (login is null || string.IsNullOrWhiteSpace(login.Password))
                {
                    return this.SetError($"The parameter [{nameof(login.Password)}] is required !!", "NullParameter", StatusCodes.Status400BadRequest);
                }

                var signInResult = await this.signInManager.PasswordSignInAsync(login.Username, login.Password, false, true);
                if (!signInResult.Succeeded)
                {
                    return this.SetError($"Login Failed for the User [{login.Username}].", "InvalidCredentials", StatusCodes.Status400BadRequest);
                }

                var user = await this.userManager.FindByNameAsync(login.Username);
                var result = JwtGenerator.GenerateJwtToken(user, this.jwtOptions);

                return this.StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception ex)
            {
                return this.SetError(ex.Message);
            }
        }

        [HttpGet]
        [Route("test")]
        public async Task<IActionResult> GetUserAsync(string username)
        {
            var user = await this.userManager.FindByNameAsync(username);
            if (user is null)
            {
                return this.StatusCode(StatusCodes.Status404NotFound, new { Error = $"The user [{username}] not found." });
            }

            return this.StatusCode(StatusCodes.Status200OK, user);
        }

        [HttpPost]
        [Route("user")]
        public async Task<IActionResult> CreateUserAsync()
        {
            var user = new ApplicationUser()
            {
                UserName = "ben-ucef@hotmail.fr",
                Email = "ben-ucef@hotmail.fr",
            };

            var result = await this.userManager.CreateAsync(user, "Azerty&0123");
            if (!result.Succeeded)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, new { Error = result.Errors });
            }

            var roleResult = await this.userManager.AddToRoleAsync(user, "Admin");

            var data = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.Phone,
            };

            return this.StatusCode(StatusCodes.Status200OK, data);
        }

        [HttpPost]
        [Route("role")]
        public async Task<IActionResult> CreateRoleAsync([FromBody] ApplicationRole role)
        {
            if (role is null)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, new { Error = $"Parameter {nameof(role)} required." });
            }

            var result = await this.rolesManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, new { Error = result.Errors });
            }

            return this.StatusCode(StatusCodes.Status200OK, role);
        }
    }
}
