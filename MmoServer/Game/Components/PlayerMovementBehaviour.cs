using System.Numerics;
using Shared;

namespace Game.Components;

public class PlayerMovementBehaviour : Behaviour
{
    private TransformBehaviour _transform = null!;
    private PlayerInputBehaviour _playerInput = null!;
    public float Speed { get; set; } = 5f;

    protected override void Start()
    {
        _transform = GameObject.GetRequiredBehaviour<TransformBehaviour>();
        _playerInput = GameObject.GetRequiredBehaviour<PlayerInputBehaviour>();
    }

    protected override void Update(float deltaTime)
    {
        var movementDirection = GetMovementDirection();
        _transform.Position += movementDirection * (Speed * deltaTime);
    }

    private Vector2 GetMovementDirection()
    {
        Vector2 movementDirection = Vector2.Zero;
        PlayerKeyboard keyboard = _playerInput.PlayerInput.Keyboard;

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