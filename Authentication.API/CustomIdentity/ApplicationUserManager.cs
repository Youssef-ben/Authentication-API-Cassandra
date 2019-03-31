namespace Authentication.API.CustomIdentity
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private readonly ILogger logger;

        public ApplicationUserManager(
            IUserStore<ApplicationUser> userStore,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<ApplicationUserManager> logger)
            : base(userStore, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Override the {UserManager.CreateAsync(...)} method to match our need.
        /// Creates the specified <paramref name="user"/> in the backing store with given password,
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <param name="password">The password for the user to hash and store.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>
        /// of the operation.
        /// </returns>
        public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            this.ThrowIfDisposed();

            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var result = await this.GeneratePasswordHash(user, password);
            if (!result.Succeeded)
            {
                return result;
            }

            return await this.CreateAsync(user);
        }

        /// <summary>
        /// Override the {UserManager.CheckPasswordAsync(...)} method to match our need.
        /// Returns a flag indicating whether the given <paramref name="password"/> is valid for the
        /// specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose password should be validated.</param>
        /// <param name="password">The password to validate</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing true if
        /// the specified <paramref name="password" /> matches the one store for the <paramref name="user"/>,
        /// otherwise false.</returns>
        public override async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            this.ThrowIfDisposed();
            if (user is null)
            {
                return false;
            }

            var success = this.VerifyPasswordAsync(user, password) != PasswordVerificationResult.Failed;
            if (!success)
            {
                this.Logger.LogWarning(0, "Invalid password for user {userId}.", await this.GetUserIdAsync(user));
            }

            return success;
        }

        /// <summary>
        /// Override the {UserManager.VerifyPasswordAsync(...)} method to match our need.
        /// Returns a <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.
        /// </summary>
        /// <param name="user">The user whose password should be verified.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="PasswordVerificationResult"/>
        /// of the operation.
        /// </returns>
        private PasswordVerificationResult VerifyPasswordAsync(ApplicationUser user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash) || string.IsNullOrWhiteSpace(user.PasswordPublicSalt) || string.IsNullOrWhiteSpace(user.PasswordPrivateSalt))
            {
                return PasswordVerificationResult.Failed;
            }

            var isValid = PasswrodHelper.ValidateUserCredentials(user, password);
            return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }

        private async Task<IdentityResult> GeneratePasswordHash(ApplicationUser user, string newPassword, bool validatePassword = true)
        {
            if (validatePassword)
            {
                var validate = await this.ValidatePasswordAsync(user, newPassword);
                if (!validate.Succeeded)
                {
                    return validate;
                }
            }

            var passwordStore = this.GetPasswordStore();

            var password = PasswrodHelper.GenratePassword(newPassword);
            user.PasswordPublicSalt = password.PasswordPublicSalt;
            user.PasswordPrivateSalt = password.PasswordPrivateSalt;

            await passwordStore.SetPasswordHashAsync(user, password.PasswordHash, this.CancellationToken);
            await this.UpdateSecurityStampInternal(user);
            return IdentityResult.Success;
        }

        private async Task UpdateSecurityStampInternal(ApplicationUser user)
        {
            if (this.SupportsUserSecurityStamp)
            {
                await this.GetSecurityStore().SetSecurityStampAsync(user, Guid.NewGuid().ToString(), this.CancellationToken);
            }
        }

        private IUserSecurityStampStore<ApplicationUser> GetSecurityStore()
        {
            if (this.Store is IUserSecurityStampStore<ApplicationUser> cast)
            {
                return cast;
            }

            throw new NotSupportedException("Store does not implement {IUserSecurityStampStore}");
        }

        private IUserPasswordStore<ApplicationUser> GetPasswordStore()
        {
            if (this.Store is IUserPasswordStore<ApplicationUser> cast)
            {
                return cast;
            }

            throw new NotSupportedException("Store does not implement {IUserPasswordStore}");
        }
    }
}
