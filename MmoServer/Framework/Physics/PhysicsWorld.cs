using System.Numerics;
using Box2DSharp.Dynamics;

namespace Framework.Physics;

public class PhysicsWorld
{
    private readonly World _world;

    public PhysicsWorld()
    {
        _world = new World(Vector2.Zero);
    }

    public void AddBody(RigidBody rigidBody)
    {
        rigidBody.Initialize(_world);
    }

    public void RemoveBody(RigidBody rigidBody)
    {
        rigidBody.Destroy(_world);
    }

    public void Update(float dt)
    {
        const int velocityIterations = 9;
        const int positionIterations = 3;
        _world.Step(dt, velocityIterations, positionIterations);
    }
}