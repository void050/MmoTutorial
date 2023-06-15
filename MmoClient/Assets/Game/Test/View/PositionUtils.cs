using UnityEngine;

namespace Game
{
    public static class PositionUtils
    {
        public static Vector3 ToVector3(this Vector2 position)
        {
            return new Vector3(position.x, 0, position.y);
        }
    }
}