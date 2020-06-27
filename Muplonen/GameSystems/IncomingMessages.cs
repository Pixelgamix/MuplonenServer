namespace Muplonen.GameSystems
{
    /// <summary>
    /// Enumeration of incoming message ids.
    /// </summary>
    public static class IncomingMessages
    {
        public const ushort AccountRegistration = 1;
        public const ushort AccountLogin = 2;
        public const ushort Chat = 3;
        public const ushort CharacterList = 4;
        public const ushort CharacterCreation = 5;
        public const ushort CharacterSelection = 6;
        public const ushort Ping = 7;
        public const ushort PlayerMove = 8;
    }
}
