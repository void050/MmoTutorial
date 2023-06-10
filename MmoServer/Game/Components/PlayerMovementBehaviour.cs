using System.Numerics;
using Shared;

namespace Game.Components;

public class PlayerMovementBehaviour : Behaviour
{
    private TransformBehaviour _transform;
    public PlayerInput PlayerInput { get; set; }
    public float Speed { get; set; } = 5f;

    protected override void Start()
    {
        _transform = GameObject.GetRequiredBehaviour<TransformBehaviour>();
    }

    protected override void Update(float deltaTime)
    {
        var movementDirection = GetMovementDirection();
        _transform.Position += movementDirection * (Speed * deltaTime);
    }

    private Vector2 GetMovementDirection()
    {
        Vector2 movementDirection = Vector2.Zero;
        if (PlayerInput.Keyboard.HasFlag(PlayerKeyboard.Up))
        {
            movementDirection += new Vector2(0, 1);
        }

        if (PlayerInput.Keyboard.HasFlag(PlayerKeyboard.Down))
        {
            movementDirection += new Vector2(0, -1);
        }

        if (PlayerInput.Keyboard.HasFlag(PlayerKeyboard.Left))
        {
            movementDirection += new Vector2(-1, 0);
        }

        if (PlayerInput.Keyboard.HasFlag(PlayerKeyboard.Right))
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