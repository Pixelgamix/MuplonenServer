using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muplonen.DataAccess;
using System.Threading.Tasks;

namespace Muplonen.Clients.MessageHandlers
{
    /// <summary>
    /// Handles character selection request messages.
    /// </summary>
    [MessageHandler(6)]
    public class CharacterSelectionMessageHandler : IMessageHandler
    {
        private readonly MuplonenDbContext _muplonenDbContext;
        private readonly ILogger<CharacterSelectionMessageHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="CharacterSelectionMessageHandler"/> instance.
        /// </summary>
        /// <param name="muplonenDbContext">The database context.</param>
        /// <param name="logger">Logging.</param>
        public CharacterSelectionMessageHandler(
            MuplonenDbContext muplonenDbContext,
            ILogger<CharacterSelectionMessageHandler> logger)
        {
            _muplonenDbContext = muplonenDbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            if (session.PlayerAccount == null || session.PlayerCharacter != null) return false;

            var charactername = message.ReadString().ToLower();

            PlayerCharacter? playerCharacter = null;
            if (_muplonenDbContext.PlayerCharacters != null)
                playerCharacter = await _muplonenDbContext.PlayerCharacters.FirstOrDefaultAsync(character => character.Charactername == charactername);

            if (playerCharacter == null)
            {
                _logger.LogInformation("\"{0}\" ({1}) tried to select character \"{2}\", but the character does not exist.",
                    session.PlayerAccount.Accountname, session.PlayerAccount.Id, charactername);
                return false;
            }

            if (playerCharacter.PlayerAccountId != session.PlayerAccount.Id)
            {
                _logger.LogInformation("\"{0}\" ({1}) tried to select character \"{2}\" ({3}), but the character belongs to account ({4}).",
                    session.PlayerAccount.Accountname, session.PlayerAccount.Id, playerCharacter.Charactername, playerCharacter.PlayerAccountId);
                return false;
            }

            session.PlayerCharacter = playerCharacter;
            await session.Connection.BuildAndSend(6, reply =>
            {
                reply.WriteByte(1);
            });

            return true;
        }
    }
}
