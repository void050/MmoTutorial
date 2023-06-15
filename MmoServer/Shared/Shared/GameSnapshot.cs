// ReSharper disable ArrangeNamespaceBody
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Globalization;
using System.Text;
using MemoryPack;
using UnityEngine;

namespace Shared
{
    [MemoryPackable]
    public partial struct GameSnapshot
    {
        public PlayerSnapshot[] Players;

        public override string ToString()
        {
            StringBuilder sb = new ();
            sb.Append($"Players: {Players.Length.ToString()}[");
            for (int i = 0; i < Players.Length; i++)
            {
                sb.Append(Players[i].ToString());
                if (i < Players.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append("]");

            return sb.ToString();
        }
    }

    [MemoryPackable]
    public partial struct PlayerSnapshot
    {
        public ushort PlayerId;
        public byte ViewId;
        public Vector2 Position;

        public override string ToString()
        {
            return $"PlayerId: {PlayerId.ToString()}, ViewId: {ViewId.ToString()} Position: {Position.ToString()}";
        }
    }
}