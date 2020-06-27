using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muplonen.DataAccess;
using Muplonen.Security;
using Muplonen.SessionManagement;
using System.Threading.Tasks;

namespace Muplonen.GameSystems.AccountSystem
{
    /// <summary>
    /// Handles account registration requests.
    /// </summary>
    [MessageHandler(IncomingMessages.AccountRegistration)]
    public class AccountRegistrationMessageHandler : IMessageHandler
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly MuplonenDbContext _muplonenDbContext;
        private readonly ILogger<AccountLoginMessageHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="AccountRegistrationMessageHandler"/> instance.
        /// </summary>
        /// <param name="passwordHasher">Hasher for creating password hashes.</param>
        /// <param name="muplonenDbContext">The database context.</param>
        /// <param name="logger">Logging.</param>
        public AccountRegistrationMessageHandler(
            IPasswordHasher passwordHasher,
            MuplonenDbContext muplonenDbContext,
            ILogger<AccountLoginMessageHandler> logger)
        {
            _passwordHasher = passwordHasher;
            _muplonenDbContext = muplonenDbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            var accountname = message.ReadString().ToLower();
            var password = message.ReadString();

            // Check, if name is taken
            var accountAlreadyExists = false;
            if (_muplonenDbContext.PlayerAccounts != null)
                accountAlreadyExists = await _muplonenDbContext.PlayerAccounts.AnyAsync(account => account.Accountname == accountname);

            if (accountAlreadyExists)
            {
                await session.Connection.BuildAndSend(OutgoingMessages.AccountRegistration, reply =>
                {
                    reply.WriteByte(0);
                    reply.WriteString("Account name already in use. Please choose a different name.");
                });
                return false;
            }

            var hashedPassword = _passwordHasher.CreateHashedPassword(password);

            // Create and save account
            var account = new PlayerAccount() { Accountname = accountname, PasswordHash = hashedPassword };
            _muplonenDbContext.Add(account);
            await _muplonenDbContext.SaveChangesAsync();
            _logger.LogInformation("Account \"{0}\" ({1}) created by session {2}", accountname, account.Id, session.SessionId);

            // Tell the client that the account has been created and close the connection.
            await session.Connection.BuildAndSend(OutgoingMessages.AccountRegistration, reply =>
            {
                reply.WriteByte(1);
                reply.WriteString("Account created. You can now log in.");
            });
            await session.Connection.Close();
            return true;
        }
    }
}
