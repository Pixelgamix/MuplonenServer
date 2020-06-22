using Microsoft.EntityFrameworkCore;
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
        private readonly ObjectPool<GodotMessage> _messageObjectPool;

        /// <summary>
        /// Creates a new <see cref="AccountLoginMessageHandler"/> instance.
        /// </summary>
        /// <param name="passwordHasher">Hasher for creating password hashes.</param>
        /// <param name="muplonenDbContext">The database context.</param>
        /// <param name="messageObjectPool">Pool for providing <see cref="GodotMessage"/> instances.</param>
        public AccountLoginMessageHandler(
            IPasswordHasher passwordHasher,
            MuplonenDbContext muplonenDbContext,
            ObjectPool<GodotMessage> messageObjectPool)
        {
            _passwordHasher = passwordHasher;
            _muplonenDbContext = muplonenDbContext;
            _messageObjectPool = messageObjectPool;
        }


        /// <summary>
        /// Handles the account login request.
        /// </summary>
        /// <param name="session">The client's context.</param>
        /// <param name="message">The client's message.</param>
        /// <returns></returns>
        public async Task HandleMessage(IPlayerSession session, GodotMessage message)
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
                    await session.Connection.Close();
                    return;
                }

                // Check password
                if (!_passwordHasher.IsSamePassword(password, account.PasswordHash))
                {
                    reply.WriteUInt16(2);
                    reply.WriteByte(0);
                    reply.WriteString("Wrong password.");
                    await session.Connection.Send(reply);
                    await session.Connection.Close();
                    return;
                }

                // Put account into session
                session.PlayerAccount = account;

                // Tell the client that the login was successfull
                reply.WriteUInt16(2);
                reply.WriteByte(1);
                await session.Connection.Send(reply);
            }
            finally
            {
                _messageObjectPool.Return(reply);
            }
        }
    }
}
