namespace Muplonen.GameSystems
{
    public static class OutgoingMessages
    {
        public const ushort AccountRegistration = 1;
        public const ushort AccountLogin = 2;
        public const ushort Chat = 3;
        public const ushort CharacterList = 4;
        public const ushort CharacterCreation = 5;
        public const ushort CharacterSelection = 6;
        public const ushort Pong = 7;
        public const ushort SelfEnterRoom = 8;
        public const ushort OtherPlayerEnterRoom = 9;
        public const ushort OtherPlayerLeftRoom = 10;
        public const ushort OtherPlayerMove = 11;
    }
}
