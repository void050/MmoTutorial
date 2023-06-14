// ReSharper disable ArrangeNamespaceBody
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

using MemoryPack;

namespace Shared
{
    [MemoryPackable]
    public partial struct JoinRequest
    {
        public string Login;
        public string Password;
    }

    [MemoryPackable]
    public partial struct JoinResponse
    {
        public ushort PlayerId;
    }

    [MemoryPackable]
    public partial struct InputRequest
    {
        public PlayerInput Input;
    }

    [MemoryPackable]
    public partial struct SnapshotResponse
    {
        public GameSnapshot Snapshot;
    }
}