using System;
using System.Collections.Generic;

namespace Muplonen.DataAccess
{
    /// <summary>
    /// Player account.
    /// </summary>
    public class PlayerAccount
    {
        /// <summary>
        /// Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique account name.
        /// </summary>
        public string Accountname { get; set; } = string.Empty;

        /// <summary>
        /// The password's hash.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Creation date of the account.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// List of <see cref="PlayerCharacter"/>s that belong to this account.
        /// </summary>
        public List<PlayerCharacter>? PlayerCharacters { get; set; }
    }
}
