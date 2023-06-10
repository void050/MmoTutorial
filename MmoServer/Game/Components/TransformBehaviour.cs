using System.Numerics;

namespace Game.Components;

public class TransformBehaviour : Behaviour
{
    public Vector2 Position { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} Position: {Position.ToString()}";
    }
}