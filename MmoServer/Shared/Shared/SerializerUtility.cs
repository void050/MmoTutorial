// ReSharper disable ArrangeNamespaceBody
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

using MemoryPack;
using Riptide;

namespace Shared
{
    public static class SerializerUtility
    {
        public static void Get<T>(this Message message, out T data) where T : struct
        {
            //todo add brotli compression
            data = MemoryPackSerializer.Deserialize<T>(message.GetBytes());
        }

        public static void Add<T>(this Message message, T parameters) where T : struct
        {
            var serialized = MemoryPackSerializer.Serialize(parameters);
            message.AddBytes(serialized);
        }
    }
}