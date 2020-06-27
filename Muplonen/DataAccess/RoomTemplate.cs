using System;
using System.Collections.Generic;

namespace Muplonen.DataAccess
{
    /// <summary>
    /// Template for rooms.
    /// </summary>
    public class RoomTemplate
    {
        /// <summary>
        /// Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The room template's title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Player characters that were last in this room template.
        /// </summary>
        public List<PlayerCharacter>? PlayerCharacters { get; set; }
    }
}
