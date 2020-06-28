using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muplonen.DataAccess;
using Muplonen.SessionManagement;
using Muplonen.World;
using System;
using System.Threading.Tasks;

namespace Muplonen.GameSystems.AccountSystem
{
    /// <summary>
    /// Handles character selection request messages.
    /// </summary>
    [MessageHandler(IncomingMessages.CharacterSelection)]
    public class CharacterSelectionMessageHandler : IMessageHandler
    {
        private readonly MuplonenDbContext _muplonenDbContext;
        private readonly IRoomManager _roomManager;
        private readonly ILogger<CharacterSelectionMessageHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="CharacterSelectionMessageHandler"/> instance.
        /// </summary>
        /// <param name="muplonenDbContext">The database context.</param>
        /// <param name="roomManager">The room manager.</param>
        /// <param name="logger">Logging.</param>
        public CharacterSelectionMessageHandler(
            MuplonenDbContext muplonenDbContext,
            IRoomManager roomManager,
            ILogger<CharacterSelectionMessageHandler> logger)
        {
            _muplonenDbContext = muplonenDbContext;
            _roomManager = roomManager;
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
            await session.Connection.BuildAndSend(OutgoingMessages.CharacterSelection, reply =>
            {
                reply.WriteByte(1);
            });

            // Join room
            await _roomManager.AddPlayerToRoomInstance(session, Guid.Empty);

            return true;
        }
    }
}
