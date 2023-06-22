using System.Numerics;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;

namespace Framework.Physics;

public class RigidBody
{
    public enum BodyType
    {
        StaticBody = 0,
        KinematicBody = 1,
        DynamicBody = 2
    }

    private Body _body = null!;
    private readonly Shape _shape;
    public float Density = 1f;
    public float Mass = 1f;
    public Vector2 SpawnPosition;
    public BodyType Type = BodyType.StaticBody;

    public RigidBody(Shape shape)
    {
        _shape = shape;
    }

    public void Initialize(World world)
    {
        BodyDef def = new();
        _body = world.CreateBody(def);
        _body.BodyType = (Box2DSharp.Dynamics.BodyType)Type;
        _body.SetMassData(new MassData { Mass = Mass });
        Position = SpawnPosition;
        FixtureDef fd = new()
        {
            Shape = _shape,
            Density = Density
        };
        _body.CreateFixture(fd);
    }

    public void ApplyLinearImpulseToCenter(Vector2 impulse)
    {
        _body.ApplyLinearImpulseToCenter(impulse, true);
    }

    public Vector2 Position
    {
        get => _body.GetPosition();
        set => _body.SetTransform(value, _body.GetAngle());
    }

    public Vector2 LinearVelocity
    {
        get => _body.LinearVelocity;
        set => _body.SetLinearVelocity(value);
    }

    public float Angle
    {
        get => _body.GetAngle();
        set => _body.SetTransform(_body.GetPosition(), value);
    }

    public void Destroy(World world)
    {
        world.DestroyBody(_body);
    }
}