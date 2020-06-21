using Muplonen.DataAccess;
using System.Threading.Tasks;

namespace Muplonen.Clients.Messages
{
    /// <summary>
    /// Handles login requests.
    /// </summary>
    [MessageHandler(1)]
    public class LoginMessageHandler : IMessageHandler
    {
        private readonly MuplonenDbContext _muplonenDbContext;

        /// <summary>
        /// Creates a new <see cref="LoginMessageHandler"/> instance.
        /// </summary>
        /// <param name="muplonenDbContext">The database context.</param>
        public LoginMessageHandler(MuplonenDbContext muplonenDbContext)
        {
            _muplonenDbContext = muplonenDbContext;
        }

        /// <summary>
        /// Handles the login request.
        /// </summary>
        /// <param name="session">The client's context.</param>
        /// <param name="message">The client's message.</param>
        /// <returns></returns>
        public async Task HandleMessage(IClientContext session, GodotMessage message)
        {
            var accountname = message.ReadString();
            var password = message.ReadString();

            var account = new PlayerAccount() { Accountname = accountname, PasswordHash = password };
            _muplonenDbContext.Add(account);
            await _muplonenDbContext.SaveChangesAsync();
        }
    }
}
