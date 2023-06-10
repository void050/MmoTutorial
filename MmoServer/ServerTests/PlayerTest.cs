using System.Numerics;
using Game;
using Game.Components;
using Shared;

namespace ServerTests;

public class PlayerTest
{
    readonly GameObjectSystem _gameObjectSystem = new();
    readonly GameObject _player = new("player", new TransformBehaviour(), new PlayerMovementBehaviour());

    [Test]
    public void TestMovement()
    {
        _gameObjectSystem.AddGameObject(_player);
        var playerMovement = _player.GetRequiredBehaviour<PlayerMovementBehaviour>();
        playerMovement.PlayerInput = new PlayerInput { Keyboard = PlayerKeyboard.Right };
        var transformBehaviour = _player.GetRequiredBehaviour<TransformBehaviour>();


        Console.WriteLine(_player);
        Vector2 positionStart = transformBehaviour.Position;
        _gameObjectSystem.Update(10f);
        Vector2 positionEnd = transformBehaviour.Position;
        Assert.That(positionEnd.X > positionStart.X);
        Console.WriteLine(_player);
    }
}