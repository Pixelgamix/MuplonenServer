using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Muplonen.DataAccess;
using Muplonen.Security;
using System.Threading.Tasks;

namespace Muplonen.Clients.Messages
{
    /// <summary>
    /// Handles account login request messages.
    /// </summary>
    [MessageHandler(2)]
    public sealed class AccountLoginMessageHandler : IMessageHandler
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly MuplonenDbContext _muplonenDbContext;
        private readonly PlayerSessionManager _playerSessionManager;
        private readonly ObjectPool<GodotMessage> _messageObjectPool;
        private readonly ILogger<AccountLoginMessageHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="AccountLoginMessageHandler"/> instance.
        /// </summary>
        /// <param name="passwordHasher">Hasher for creating password hashes.</param>
        /// <param name="muplonenDbContext">The database context.</param>
        /// <param name="messageObjectPool">Pool for providing <see cref="GodotMessage"/> instances.</param>
        /// <param name="logger">Logging.</param>
        public AccountLoginMessageHandler(
            IPasswordHasher passwordHasher,
            MuplonenDbContext muplonenDbContext,
            PlayerSessionManager playerSessionManager,
            ObjectPool<GodotMessage> messageObjectPool,
            ILogger<AccountLoginMessageHandler> logger)
        {
            _passwordHasher = passwordHasher;
            _muplonenDbContext = muplonenDbContext;
            _playerSessionManager = playerSessionManager;
            _messageObjectPool = messageObjectPool;
            _logger = logger;
        }



        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            var reply = _messageObjectPool.Get();
            try
            {
                var accountname = message.ReadString().ToLower();
                var password = message.ReadString();

                // Fetch account
                var account = await _muplonenDbContext.PlayerAccounts.FirstAsync(account => account.Accountname == accountname);
                if (account == null)
                {
                    reply.WriteUInt16(2);
                    reply.WriteByte(0);
                    reply.WriteString("Account does not exist.");
                    await session.Connection.Send(reply);
                    return false;
                }

                // Check password
                if (!_passwordHasher.IsSamePassword(password, account.PasswordHash))
                {
                    reply.WriteUInt16(2);
                    reply.WriteByte(0);
                    reply.WriteString("Wrong password.");
                    await session.Connection.Send(reply);
                    return false;
                }

                // Put account into session
                session.PlayerAccount = account;
                if (!_playerSessionManager.Clients.TryAdd(account.Id, session))
                {
                    if (_playerSessionManager.Clients.TryGetValue(account.Id, out IPlayerSession? existingSession))
                    {
                        _logger.LogInformation("Tried to log into account \"{0}\" ({1}) from session {2}, but account is already logged in from session {3}",
                            account.Accountname, account.Id, session.SessionId, existingSession.SessionId);
                    }
                    else
                    {
                        _logger.LogInformation("Tried to log into account \"{0}\" ({1}) from session {2}, but account was already in use by another session.",
                            account.Accountname, account.Id, session.SessionId);
                    }
                    reply.WriteUInt16(2);
                    reply.WriteByte(0);
                    reply.WriteString("Account already in use by another session.");
                    await session.Connection.Send(reply);
                    return false;
                }

                _logger.LogInformation("Account \"{0}\" ({1}) logged in with session {2}", account.Accountname, account.Id, session.SessionId);

                // Tell the client that the login was successfull
                reply.WriteUInt16(2);
                reply.WriteByte(1);
                await session.Connection.Send(reply);
                return true;
            }
            finally
            {
                _messageObjectPool.Return(reply);
            }
        }
    }
}
