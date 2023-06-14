// ReSharper disable ArrangeNamespaceBody
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Shared
{
    public static class NetworkConfig
    {
        public const int TickRate = 20;
        public const float TickInterval = 1f / TickRate;
        public const int TickIntervalMilliseconds = 1000 / TickRate;
        public const int ServerPort = 12345;
        public const int MaxClients = 100;
    }
}