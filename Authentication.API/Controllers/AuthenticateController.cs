namespace Authentication.API.Controllers
{
    using System;
    using System.Threading.Tasks;
    using AspNetCore.Identity.Cassandra;
    using Authentication.API.Config;
    using Authentication.API.Config.Settings;
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
        private readonly IOptionsSnapshot<JwtSettings> jwtOptions;

        public AuthenticateController(IOptionsSnapshot<CassandraOptions> snapshot, IOptionsSnapshot<JwtSettings> jwtOptions, ApplicationSignInManager signInManager, ApplicationUserManager userManager, RoleManager<ApplicationRole> rolesManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.rolesManager = rolesManager;
            this.cassandraOptions = snapshot;
            this.jwtOptions = jwtOptions;
        }

        [Route("config")]
        public IActionResult GetConfig()
        {
            return this.Ok(this.jwtOptions.Value);
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
    }
}
