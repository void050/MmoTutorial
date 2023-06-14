// ReSharper disable ArrangeNamespaceBody
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Shared
{
    public enum GameMessageId: ushort
    {
        None = 0,
        JoinRequest = 1,
        JoinResponse = 2,
        InputRequest = 3,
        SnapshotResponse = 4,
    }

    public static class GameMessageIdExtension
    {
        public static ushort ToUShort(this GameMessageId messageId)
        {
            return (ushort)messageId;
        }
    }
}