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

    private PlayerInputBehaviour _playerInput = null!;

    public float ChangeDirectionEverySeconds = 0.5f;

    private float _nextChangeDirectionTime;

    private readonly bool _clockwise = Random.Shared.NextSingle() > 0.5f;
    private int _currentKeyboard;

    protected override void Start()
    {
        _playerInput = GetRequiredBehaviour<PlayerInputBehaviour>();
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
        PlayerKeyboard playerKeyboard = PlayerKeyboards[keyboardIndex];
        if (Random.Shared.NextSingle() > 0.9f)
        {
            playerKeyboard |= Random.Shared.NextSingle() > 0.8f
                ? PlayerKeyboard.AttackSkill2
                : PlayerKeyboard.AttackSkill1;
        }

        _playerInput.PlayerInput = new PlayerInput
        {
            Keyboard = playerKeyboard
        };
    }
}