namespace Authentication.API.Controllers
{
    using System.Threading.Tasks;
    using Authentication.API.CustomIdentity;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    [Route("api/identity/role")]
    [ApiController]
    public class RoleController : BaseController
    {
        private readonly RoleManager<ApplicationRole> rolesManager;

        public RoleController(RoleManager<ApplicationRole> rolesManager)
        {
            this.rolesManager = rolesManager;
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