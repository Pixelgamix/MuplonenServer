using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muplonen.DataAccess;
using Muplonen.SessionManagement;
using System.Threading.Tasks;

namespace Muplonen.GameSystems.AccountSystem
{
    /// <summary>
    /// Handles character creation requests.
    /// </summary>
    [MessageHandler(IncomingMessages.CharacterCreation)]
    public sealed class CharacterCreationMessageHandler : IMessageHandler
    {
        private readonly MuplonenDbContext _muplonenDbContext;
        private readonly ILogger<CharacterCreationMessageHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="CharacterCreationMessageHandler"/> instance.
        /// </summary>
        /// <param name="muplonenDbContext">The database context.</param>
        /// <param name="logger">Logging.</param>
        public CharacterCreationMessageHandler(
            MuplonenDbContext muplonenDbContext,
            ILogger<CharacterCreationMessageHandler> logger)
        {
            _muplonenDbContext = muplonenDbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            if (session.PlayerAccount == null || session.PlayerCharacter != null) return false;

            var charactername = message.ReadString().ToLower();

            var characterAlreadyExists = false;
            if (_muplonenDbContext.PlayerCharacters != null)
                characterAlreadyExists = await _muplonenDbContext.PlayerCharacters.AnyAsync(character => character.Charactername == charactername);

            if (characterAlreadyExists)
            {
                await session.Connection.BuildAndSend(OutgoingMessages.CharacterCreation, reply =>
                {
                    reply.WriteByte(0);
                    reply.WriteString("Charactername already taken. Please try a different one.");
                });
                return true;
            }

            var newCharacter = new PlayerCharacter()
            {
                Charactername = charactername,
                PlayerAccountId = session.PlayerAccount.Id,
                PlayerAccount = session.PlayerAccount
            };
            _muplonenDbContext.Add(newCharacter);
            await _muplonenDbContext.SaveChangesAsync();

            _logger.LogInformation("\"{0}\" ({1}) created character \"{2}\" ({3}).",
                session.PlayerAccount.Accountname, session.PlayerAccount.Id, newCharacter.Charactername, newCharacter.Id);

            await session.Connection.BuildAndSend(OutgoingMessages.CharacterCreation, reply =>
            {
                reply.WriteByte(1);
            });

            return true;
        }
    }
}
