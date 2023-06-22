using System.Numerics;
using Box2DSharp.Collision.Shapes;
using Framework.Physics;
using Game;
using Game.Components;
using Shared;

namespace ServerTests;

public class PlayerTest
{
    private readonly GameObjectSystem _gameObjectSystem = new();
    private readonly PhysicsWorld _physicsWorld;
    private readonly GameObject _player;

    public PlayerTest()
    {
        _physicsWorld = new PhysicsWorld();

        var rigidBody = new RigidBody(new CircleShape { Radius = 0.5f })
        {
            Type = RigidBody.BodyType.DynamicBody
        };
        _player = new(_gameObjectSystem, _physicsWorld, "player", new TransformBehaviour(),
            new PlayerMovementBehaviour(),
            new PlayerInputComponent(),
            new RigidBodyBehaviour(rigidBody));
    }

    [Test]
    public void TestMovement()
    {
        _gameObjectSystem.Update(0);

        var playerInput = _player.GetRequiredComponent<PlayerInputComponent>();
        playerInput.PlayerInput = new PlayerInput { Keyboard = PlayerKeyboard.Right };
        var transformComponent = _player.GetRequiredComponent<TransformBehaviour>();


        Console.WriteLine(_player);
        Vector2 positionStart = transformComponent.Position;

        float timeStep = NetworkConfig.TickInterval;
        float totalTime = 10f;
        while (totalTime > 0)
        {
            totalTime -= timeStep;
            _gameObjectSystem.Update(timeStep);
            _physicsWorld.Update(timeStep);
        }

        Vector2 positionEnd = transformComponent.Position;
        Assert.That(positionEnd.X > positionStart.X);
        Console.WriteLine(_player);
    }
}