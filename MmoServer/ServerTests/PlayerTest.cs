using System.Numerics;
using Game;
using Game.Components;
using Shared;

namespace ServerTests;

public class PlayerTest
{
    private readonly GameObjectSystem _gameObjectSystem = new();
    private readonly GameObject _player;

    public PlayerTest()
    {
        _player = new(_gameObjectSystem, "player", new TransformBehaviour(), new PlayerMovementBehaviour(),
            new PlayerInputBehaviour());
    }


    [Test]
    public void TestMovement()
    {
        var playerInput = _player.GetRequiredBehaviour<PlayerInputBehaviour>();
        playerInput.PlayerInput = new PlayerInput { Keyboard = PlayerKeyboard.Right };
        var transformBehaviour = _player.GetRequiredBehaviour<TransformBehaviour>();


        Console.WriteLine(_player);
        Vector2 positionStart = transformBehaviour.Position;
        _gameObjectSystem.Update(10f);
        Vector2 positionEnd = transformBehaviour.Position;
        Assert.That(positionEnd.X > positionStart.X);
        Console.WriteLine(_player);
    }
}