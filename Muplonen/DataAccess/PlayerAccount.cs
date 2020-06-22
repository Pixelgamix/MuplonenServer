using System;

namespace Muplonen.DataAccess
{
    public class PlayerAccount
    {
        public Guid Id { get; set; }
        public string Accountname { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
