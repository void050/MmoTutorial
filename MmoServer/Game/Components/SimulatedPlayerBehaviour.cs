using Shared;

namespace Game.Components;

public class SimulatedPlayerBehaviour : Behaviour
{
    private static readonly PlayerKeyboard[] PlayerKeyboards =
    {
        PlayerKeyboard.Up,
        PlayerKeyboard.Up | PlayerKeyboard.Left,
        PlayerKeyboard.Left,
        PlayerKeyboard.Down | PlayerKeyboard.Left,
        PlayerKeyboard.Down,
        PlayerKeyboard.Down | PlayerKeyboard.Right,
        PlayerKeyboard.Right,
        PlayerKeyboard.Up | PlayerKeyboard.Right
    };

    private PlayerMovementBehaviour _playerMovementBehaviour = null!;

    public float ChangeDirectionEverySeconds = 0.5f;

    private float _nextChangeDirectionTime;

    private readonly bool _clockwise = Random.Shared.NextSingle() > 0.5f;
    private int _currentKeyboard;

    protected override void Start()
    {
        _playerMovementBehaviour = GetRequiredBehaviour<PlayerMovementBehaviour>();
    }

    protected override void Update(float deltaTime)
    {
        _nextChangeDirectionTime -= deltaTime;
        if (!(_nextChangeDirectionTime < 0))
        {
            return;
        }

        _nextChangeDirectionTime += ChangeDirectionEverySeconds;

        _currentKeyboard++;
        int keyboardIndex = _clockwise
            ? _currentKeyboard % PlayerKeyboards.Length
            : PlayerKeyboards.Length - _currentKeyboard % PlayerKeyboards.Length - 1;
        _playerMovementBehaviour.PlayerInput = new PlayerInput
        {
            Keyboard = PlayerKeyboards[keyboardIndex]
        };
    }
}