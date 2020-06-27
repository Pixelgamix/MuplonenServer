using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muplonen.DataAccess;
using Muplonen.Security;
using Muplonen.SessionManagement;
using System.Threading.Tasks;

namespace Muplonen.GameSystems.AccountSystem
{
    /// <summary>
    /// Handles account login request messages.
    /// </summary>
    [MessageHandler(IncomingMessages.AccountLogin)]
    public sealed class AccountLoginMessageHandler : IMessageHandler
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly MuplonenDbContext _muplonenDbContext;
        private readonly IPlayerSessionManager _playerSessionManager;
        private readonly ILogger<AccountLoginMessageHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="AccountLoginMessageHandler"/> instance.
        /// </summary>
        /// <param name="passwordHasher">Hasher for creating password hashes.</param>
        /// <param name="muplonenDbContext">The database context.</param>
        /// <param name="playerSessionManager">Player session manager.</param>
        /// <param name="logger">Logging.</param>
        public AccountLoginMessageHandler(
            IPasswordHasher passwordHasher,
            MuplonenDbContext muplonenDbContext,
            IPlayerSessionManager playerSessionManager,
            ILogger<AccountLoginMessageHandler> logger)
        {
            _passwordHasher = passwordHasher;
            _muplonenDbContext = muplonenDbContext;
            _playerSessionManager = playerSessionManager;
            _logger = logger;
        }



        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            var accountname = message.ReadString().ToLower();
            var password = message.ReadString();

            // Fetch account
            PlayerAccount? account = null;
            if (_muplonenDbContext.PlayerAccounts != null)
                account = await _muplonenDbContext.PlayerAccounts.FirstAsync(account => account.Accountname == accountname);

            if (account == null)
            {
                await session.Connection.BuildAndSend(OutgoingMessages.AccountLogin, reply =>
                {
                    reply.WriteByte(0);
                    reply.WriteString("Account does not exist.");
                });
                return false;
            }

            // Check password
            if (!_passwordHasher.IsSamePassword(password, account.PasswordHash))
            {
                await session.Connection.BuildAndSend(OutgoingMessages.AccountLogin, reply =>
                {
                    reply.WriteByte(0);
                    reply.WriteString("Wrong password.");
                });
                return false;
            }

            // Put account into session
            if (_playerSessionManager.Sessions.TryGetByAccountId(account.Id, out IPlayerSession? existingSession) && existingSession != null)
            {
                _logger.LogInformation("Tried to log into account \"{0}\" ({1}) from session {2}, but account is already logged in from session {3}",
                    account.Accountname, account.Id, session.SessionId, existingSession.SessionId);

                await session.Connection.BuildAndSend(OutgoingMessages.AccountLogin, reply =>
                {
                    reply.WriteByte(0);
                    reply.WriteString("Account already in use by another session.");
                });
                return false;
            }

            session.PlayerAccount = account;
            _logger.LogInformation("Account \"{0}\" ({1}) logged in with session {2}", account.Accountname, account.Id, session.SessionId);

            // Tell the client that the login was successfull
            await session.Connection.BuildAndSend(OutgoingMessages.AccountLogin, reply =>
            {
                reply.WriteByte(1);
            });
            return true;
        }
    }
}
