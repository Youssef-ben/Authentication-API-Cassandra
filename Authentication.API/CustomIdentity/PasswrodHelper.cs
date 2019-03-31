namespace Authentication.API.CustomIdentity
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;

    public static class PasswrodHelper
    {
        /// <summary>
        /// Used To generate password hash and its salts based on the given plain text password.
        /// </summary>
        /// <param name="plainTextPassword">{String} plain text password.</param>
        /// <returns>{PasswordModel} Newly generated password.</returns>
        public static PasswordModel GenratePassword(string plainTextPassword)
        {
            var publicSalt = PasswrodHelper.GenerateNewSaltBase64();
            var privateSalt = PasswrodHelper.GenerateNewSaltBase64();
            var firstHashBase64 = PasswrodHelper.GenerateSha1HashBase64(plainTextPassword, publicSalt);
            var finalHash = PasswrodHelper.GenerateSha512HashBase64(firstHashBase64, privateSalt);

            return new PasswordModel()
            {
                Password = plainTextPassword,
                PasswordHash = finalHash,
                PasswordPublicSalt = publicSalt,
                PasswordPrivateSalt = privateSalt,
            };
        }

        /// <summary>
        /// Used to validate the user database password with the recieved one.
        /// </summary>
        /// <param name="user">{ApplicationUser} The user informations from the database.</param>
        /// <param name="plainTextPassword">{string} the recieved plain text password.</param>
        /// <returns>{bool} True if valid, False otherwise.</returns>
        public static bool ValidateUserCredentials(ApplicationUser user, string plainTextPassword)
        {
            string sha1PwdHash = PasswrodHelper.GenerateSha1HashBase64(plainTextPassword, user.PasswordPublicSalt);
            string sha512PwdHash = PasswrodHelper.GenerateSha512HashBase64(sha1PwdHash, user.PasswordPrivateSalt);

            return PasswrodHelper.ValidatePassword(sha512PwdHash, user.PasswordHash as string);
        }

        /// <summary>
        /// Used to Generate a random password with the public and private salt.
        /// </summary>
        /// <returns>{PasswordHolder} The Newly created password.</returns>
        public static PasswordModel GenerateRandomPassword()
        {
            var random = new byte[8];
            new RNGCryptoServiceProvider().GetBytes(random);
            var password = Convert.ToBase64String(random);
            var publicSalt = PasswrodHelper.GenerateNewSaltBase64();
            var privateSalt = PasswrodHelper.GenerateNewSaltBase64();
            var firstHashBase64 = PasswrodHelper.GenerateSha1HashBase64(password, publicSalt);
            var finalHash = PasswrodHelper.GenerateSha512HashBase64(firstHashBase64, privateSalt);

            return new PasswordModel()
            {
                Password = password,
                PasswordHash = finalHash,
                PasswordPublicSalt = publicSalt,
                PasswordPrivateSalt = privateSalt,
            };
        }

        /// <summary>
        /// Used to Generate an SHA1 password hash based on the plain text password and the public salt.
        /// </summary>
        /// <param name="plainTextPassword">{string} Plain text password.</param>
        /// <param name="passwordPublicSalt">{string} Password Salt either from the database or a newly generated salt.</param>
        /// <returns>{string} base 64 Passwrod SHA1 hash.</returns>
        public static string GenerateSha1HashBase64(string plainTextPassword, string passwordPublicSalt)
        {
            HashAlgorithm firstHasher = new SHA1CryptoServiceProvider();
            byte[] firstHash = firstHasher.ComputeHash(PasswrodHelper.CombineSalt(System.Text.Encoding.UTF8.GetBytes(plainTextPassword), Convert.FromBase64String(passwordPublicSalt)));

            return Convert.ToBase64String(firstHash);
        }

        /// <summary>
        /// Used to Generate a second hash {SHA512} based on the SHA1 password hash and the private salt.
        /// </summary>
        /// <param name="passwordSha1HashBase64">{string} SHA1 password Hash base 64.</param>
        /// <param name="passwordPrivateSalt">{string} Password Salt either from the database or a newly generated salt.</param>
        /// <returns>{string} base 64 Passwrod SHA512 hash.</returns>
        public static string GenerateSha512HashBase64(string passwordSha1HashBase64, string passwordPrivateSalt)
        {
            HashAlgorithm secondHasher = new SHA512CryptoServiceProvider();
            byte[] secondHash = secondHasher.ComputeHash(PasswrodHelper.CombineSalt(Convert.FromBase64String(passwordSha1HashBase64), Convert.FromBase64String(passwordPrivateSalt)));

            return Convert.ToBase64String(secondHash);
        }

        /// <summary>
        /// Used to Generate a new random Salt.
        /// </summary>
        /// <returns>{string} Base 64 of salt.</returns>
        public static string GenerateNewSaltBase64()
        {
            byte[] newSalt = new byte[64];
            new RNGCryptoServiceProvider().GetBytes(newSalt);
            return Convert.ToBase64String(newSalt);
        }

        /// <summary>
        ///  Used to validate if the user SHA512 hash is valide.
        /// </summary>
        /// <param name="pwdSha512HashBase64">{String} Password SHA265 Hash base 64.</param>
        /// <param name="dataBasePasswodSha512HashBase64">{String} Database Password SHA265 Hash base 64.</param>
        /// <returns>{bool} : True if equals, False otherwise.</returns>
        public static bool ValidatePassword(string pwdSha512HashBase64, string dataBasePasswodSha512HashBase64)
        {
            return pwdSha512HashBase64.SequenceEqual(dataBasePasswodSha512HashBase64);
        }

        /// <summary>
        ///  Used To Combine a password with the specifier salt.
        /// </summary>
        /// <param name="password">{Byte} password.</param>
        /// <param name="salt">{byte} salt.</param>
        /// <returns>{Byte} the combinaison of the password and its salt.</returns>
        public static byte[] CombineSalt(byte[] password, byte[] salt)
        {
            byte[] combine = new byte[password.Length + salt.Length];

            Array.Copy(password, combine, password.Length);
            Array.Copy(salt, 0, combine, password.Length, salt.Length);

            return combine;
        }
    }
}
