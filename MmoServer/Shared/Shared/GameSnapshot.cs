// ReSharper disable ArrangeNamespaceBody
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Diagnostics.Contracts;
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
            StringBuilder sb = new();
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
        public const float MinHealth = 0f;
        public const float MaxHealth = 1f;

        public ushort PlayerId;
        public byte ViewId;
        public Vector2 Position;
        public PlayerKeyboard Keyboard;
        public ByteFloat Health;
        public ActiveSkill ActiveSkill;

        public override string ToString()
        {
            return $"PlayerId: {PlayerId.ToString()}, ViewId: {ViewId.ToString()} Position: {Position.ToString()}";
        }
    }

    [Flags]
    public enum ActiveSkill : byte
    {
        None = 0,
        AttackSkill1 = 1 << 0,
        AttackSkill2 = 1 << 1
    }

    [MemoryPackable]
    public partial struct ByteFloat
    {
        public byte Value;

        public static ByteFloat FromFloat(float value, float min, float max)
        {
            return new ByteFloat
            {
                Value = (byte)Math.Round((value - min) / (max - min) * 255)
            };
        }

        [Pure]
        public float Unpack(float min, float max)
        {
            return min + (max - min) * Value / 255f;
        }
    }
}