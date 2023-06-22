using Shared;

namespace Game.Components.Skills;

public abstract class SkillBehaviour : Behaviour
{
    protected abstract PlayerKeyboard UseKey { get; }
    private PlayerInputComponent _playerInput = null!;
    private float _cooldown;
    private float _currentCooldown;

    private ISkillApplyTo _applyTo = null!;
    private ISkillEffect _effect = null!;

    private bool _skillUsed;

    public bool SkillUsed
    {
        get
        {
            var skillUsed = _skillUsed;
            _skillUsed = false;
            return skillUsed;
        }
        private set => _skillUsed = value;
    }


    protected override void Start()
    {
        _playerInput = GameObject.GetRequiredComponent<PlayerInputComponent>();
    }

    protected void Init(ISkillApplyTo applyTo, ISkillEffect effect, float cooldown)
    {
        _cooldown = cooldown;
        _applyTo = applyTo;
        _effect = effect;
    }

    protected override void Update(float deltaTime)
    {
        if (_currentCooldown > 0)
        {
            _currentCooldown -= deltaTime;
            return;
        }

        if (_playerInput.PlayerInput.Keyboard.HasFlag(UseKey))
        {
            _currentCooldown = _cooldown;
            _applyTo.ApplyTo(_effect);
            SkillUsed = true;
        }
    }
}