using Game;

namespace Framework.Physics;

public class RigidBodyBehaviour : Behaviour
{
    public RigidBody RigidBody { get; }

    public RigidBodyBehaviour(RigidBody rigidBody)
    {
        RigidBody = rigidBody;
    }

    protected internal override void Start()
    {
        PhysicsWorld.AddBody(RigidBody);
    }

    protected internal override void OnDestroy()
    {
        PhysicsWorld.RemoveBody(RigidBody);
    }

    public override string ToString()
    {
        return $"{base.ToString()} Velocity: {RigidBody.LinearVelocity}";
    }
}