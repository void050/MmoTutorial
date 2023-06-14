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
    public enum PlayerKeyboard
    {
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3
    }
}