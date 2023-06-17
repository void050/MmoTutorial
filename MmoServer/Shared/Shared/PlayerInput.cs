// ReSharper disable ArrangeNamespaceBody
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using MemoryPack;

namespace Shared
{
    [MemoryPackable]
    public partial struct PlayerInput
    {
        public PlayerKeyboard Keyboard;
    }

    [Flags]
    public enum PlayerKeyboard : byte
    {
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        AttackSkill1 = 1 << 4,
        AttackSkill2 = 1 << 5
    }
}