using Microsoft.EntityFrameworkCore;
using Muplonen.DataAccess;
using Muplonen.SessionManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Muplonen.GameSystems.AccountSystem
{
    /// <summary>
    /// Handles requests for an account's list of characters.
    /// </summary>
    [MessageHandler(IncomingMessages.CharacterList)]
    public class CharacterListMessageHandler : IMessageHandler
    {
        private readonly MuplonenDbContext _muplonenDbContext;

        public CharacterListMessageHandler(
            MuplonenDbContext muplonenDbContext)
        {
            _muplonenDbContext = muplonenDbContext;
        }

        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            if (session.PlayerAccount == null) return false;

            List<PlayerCharacter>? characters = null;

            if (_muplonenDbContext.PlayerCharacters != null)
                characters = await _muplonenDbContext.PlayerCharacters
                    .Where(character => character.PlayerAccountId == session.PlayerAccount.Id)
                    .ToListAsync();

            await session.Connection.BuildAndSend(OutgoingMessages.CharacterList, reply =>
            {
                if (characters == null || characters.Count == 0)
                {
                    reply.WriteByte(0);
                    return;
                }

                reply.WriteByte((byte)characters.Count);
                foreach (var character in characters)
                {
                    reply.WriteString(character.Charactername);
                    reply.WriteString(character.CreatedAt.ToString(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern));
                }
            });

            return true;
        }
    }
}
