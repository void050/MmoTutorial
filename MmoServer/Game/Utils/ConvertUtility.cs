namespace Game;

public static class ConvertUtility
{
    public static UnityEngine.Vector2 ToClientVector2(this System.Numerics.Vector2 vector2)
    {
        return new UnityEngine.Vector2
        {
            x = vector2.X,
            y = vector2.Y
        };
    }
}