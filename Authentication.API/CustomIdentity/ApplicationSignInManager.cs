namespace Authentication.API.CustomIdentity
{
    using System;
    using System.Threading.Tasks;
    using Cassandra.Data.Linq;
    using Cassandra.Mapping;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class ApplicationSignInManager : SignInManager<ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IMapper mapper;
        private readonly Table<ApplicationUser> usersTable;

        public ApplicationSignInManager(
            ApplicationUserManager userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            Cassandra.ISession session)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, null)
        {
            this.Session = session;
            this.mapper = new Mapper(this.Session);
            this.usersTable = new Table<ApplicationUser>(this.Session);

            this.userManager = userManager;
            this.contextAccessor = contextAccessor;
        }

        private Cassandra.ISession Session { get; }

        /// <summary>
        /// Override the {SingInManager.PasswordSignInAsync(string, string, bool, bool)} method to match our need.
        /// Attempts to sign in the specified <paramref name="userName"/> and <paramref name="password"/> combination
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="userName">The user name to sign in.</param>
        /// <param name="password">The password to attempt to sign in with.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <param name="lockoutOnFailure">Flag indicating if the user account should be locked if the sign in fails.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see name="SignInResult"/>
        /// for the sign-in attempt.</returns>
        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await this.userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return await this.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        /// <summary>
        /// Override the {SingInManager.PasswordSignInAsync(User, string, bool, bool)} method to match our need.
        /// Attempts to sign in the specified <paramref name="user"/> and <paramref name="password"/> combination
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to sign in.</param>
        /// <param name="password">The password to attempt to sign in with.</param>
        /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
        /// <param name="lockoutOnFailure">Flag indicating if the user account should be locked if the sign in fails.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see name="SignInResult"/>
        /// for the sign-in attempt.</returns>
        public override async Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentNullException(nameof(user));
            }

            var attempt = await this.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
            return attempt.Succeeded
                ? await this.SignInOrTwoFactorAsync(user, isPersistent)
                : attempt;
        }

        /// <summary>
        /// Override the {SingInManager.CheckPasswordSignInAsync(User, string, bool)} method to match our need.
        /// Attempts a password sign in for a user.
        /// </summary>
        /// <param name="user">The user to sign in.</param>
        /// <param name="password">The password to attempt to sign in with.</param>
        /// <param name="lockoutOnFailure">Flag indicating if the user account should be locked if the sign in fails.</param>
        /// <returns>The task object representing the asynchronous operation containing the <see name="SignInResult"/>
        /// for the sign-in attempt.</returns>
        /// </returns>
        public override async Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
        {
            if (user is null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(user));
            }

            var error = await this.PreSignInCheck(user);
            if (error != null)
            {
                return error;
            }

            if (await this.userManager.CheckPasswordAsync(user, password))
            {
                var alwaysLockout = AppContext.TryGetSwitch("Microsoft.AspNetCore.Identity.CheckPasswordSignInAlwaysResetLockoutOnSuccess", out var enabled) && enabled;

                // Only reset the lockout when TFA is not enabled when not in quirks mode
                if (alwaysLockout || !await this.IsTfaEnabled(user))
                {
                    await this.ResetLockout(user);
                }

                return SignInResult.Success;
            }

            if (this.userManager.SupportsUserLockout && lockoutOnFailure)
            {
                // If lockout is requested, increment access failed count which might lock out the user
                await this.userManager.AccessFailedAsync(user);
                if (await this.userManager.IsLockedOutAsync(user))
                {
                    return await this.LockedOut(user);
                }
            }

            return SignInResult.Failed;
        }

        private async Task<bool> IsTfaEnabled(ApplicationUser user)
           => this.userManager.SupportsUserTwoFactor &&
           await this.userManager.GetTwoFactorEnabledAsync(user) &&
           (await this.userManager.GetValidTwoFactorProvidersAsync(user)).Count > 0;
    }
}
