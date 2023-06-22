using System.Numerics;
using Framework.Physics;
using Shared;

namespace Game.Components;

public class PlayerMovementBehaviour : Behaviour
{
    private PlayerInputComponent _playerInput = null!;
    private RigidBody _rigidBody = null!;
    public float Speed { get; set; } = 5f;

    protected override void Start()
    {
        _rigidBody = GameObject.GetRequiredComponent<RigidBodyBehaviour>().RigidBody;
        _playerInput = GameObject.GetRequiredComponent<PlayerInputComponent>();
    }

    protected override void Update(float deltaTime)
    {
        var movementDirection = GetMovementDirection(_playerInput.PlayerInput.Keyboard);

        _rigidBody.LinearVelocity = movementDirection * Speed;
    }

    private Vector2 GetMovementDirection(PlayerKeyboard keyboard)
    {
        Vector2 movementDirection = Vector2.Zero;

        if (keyboard.HasFlag(PlayerKeyboard.Up))
        {
            movementDirection += new Vector2(0, 1);
        }

        if (keyboard.HasFlag(PlayerKeyboard.Down))
        {
            movementDirection += new Vector2(0, -1);
        }

        if (keyboard.HasFlag(PlayerKeyboard.Left))
        {
            movementDirection += new Vector2(-1, 0);
        }

        if (keyboard.HasFlag(PlayerKeyboard.Right))
        {
            movementDirection += new Vector2(1, 0);
        }

        float length = movementDirection.Length();
        if (length > 0)
        {
            movementDirection = Vector2.Normalize(movementDirection);
        }

        return movementDirection;
    }
}