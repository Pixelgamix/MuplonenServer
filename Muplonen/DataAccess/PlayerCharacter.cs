using System;

namespace Muplonen.DataAccess
{
    /// <summary>
    /// Player character.
    /// </summary>
    public class PlayerCharacter
    {
        /// <summary>
        /// Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The character's unique name.
        /// </summary>
        public string Charactername { get; set; } = string.Empty;

        /// <summary>
        /// Creation date of the character.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// The id of the <see cref="PlayerAccount"/> the character belongs to.
        /// </summary>
        public Guid PlayerAccountId { get; set; }

        /// <summary>
        /// The <see cref="PlayerAccount"/> this character belongs to.
        /// </summary>
        public PlayerAccount? PlayerAccount { get; set; }

        /// <summary>
        /// The room's template id the character was last located in.
        /// </summary>
        public Guid? RoomTemplateId { get; set; }

        /// <summary>
        /// The <see cref="RoomTemplate"/> the character was last located in.
        /// </summary>
        public RoomTemplate? RoomTemplate { get; set; }
    }
}
