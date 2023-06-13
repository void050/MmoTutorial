namespace Shared
{
    public enum GameMessageId: ushort
    {
        None = 0,
        JoinRequest = 1,
        JoinResponse = 2
    }

    public static class GameMessageIdExtension
    {
        public static ushort ToUShort(this GameMessageId messageId)
        {
            return (ushort)messageId;
        }
    }
}