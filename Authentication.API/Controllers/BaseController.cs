namespace Authentication.API.Controllers
{
    using System;
    using System.Security.Claims;
    using Authentication.API.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public string GetUserId()
        {
            if (this.User is null)
            {
                throw new ArgumentNullException("User not logged in.");
            }

            return this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public IActionResult SetError(string description, string error = "", int status = -1)
        {
            if (status == -1)
            {
                status = StatusCodes.Status500InternalServerError;
                error = "InternalError";
            }

            var customError = new CustomError()
            {
                Code = status,
                Error = error,
                Description = description,
            };

            return this.StatusCode(status, customError);
        }
    }
}