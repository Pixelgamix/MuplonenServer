namespace Muplonen.Security
{
    /// <summary>
    /// Provides password hashing functionality.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Returns a hash of the specified password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The password's hash.</returns>
        string CreateHashedPassword(string password);

        /// <summary>
        /// Checks, if the specified password is the same as the one
        /// that was used to create the specified hash.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <returns>True, if the password is equal to the one that was used to create the hash.</returns>
        bool IsSamePassword(string password, string hashedPassword);
    }
}
