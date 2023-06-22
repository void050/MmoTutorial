using System.Numerics;
using Framework.Physics;

namespace Game.Components;

public class TransformBehaviour : Behaviour
{
    protected override void Start()
    {
        _rigidBody = GetComponent<RigidBodyBehaviour>()?.RigidBody;
    }

    private RigidBody? _rigidBody;

    private Vector2 _position;

    public Vector2 Position
    {
        get => _rigidBody?.Position ?? _position;
        set
        {
            if (_rigidBody != null)
            {
                _rigidBody.Position = value;
            }
            else
            {
                _position = value;
            }
        }
    }
    public override string ToString()
    {
        return $"{base.ToString()} Position: {Position.ToString()}";
    }
}