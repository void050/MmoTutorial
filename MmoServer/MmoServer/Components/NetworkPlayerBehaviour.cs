using Game.Components.Skills;
using Shared;

namespace Game.Components;

public class NetworkPlayerBehaviour : Behaviour
{
    public ushort PlayerId;
    public byte ViewId = 1;
    private TransformBehaviour _transform = null!;
    private HealthComponent _health = null!;
    private FastAttackSkill _fastAttackSkill = null!;
    private HeavyAttackSkill _heavyAttackSkill = null!;
    private PlayerInputComponent _inputComponent = null!;

    protected override void Start()
    {
        _transform = GetRequiredComponent<TransformBehaviour>();
        _health = GetRequiredComponent<HealthComponent>();
        _fastAttackSkill = GetRequiredComponent<FastAttackSkill>();
        _heavyAttackSkill = GetRequiredComponent<HeavyAttackSkill>();
        _inputComponent = GetRequiredComponent<PlayerInputComponent>();
    }

    public PlayerSnapshot GetSnapshot()
    {
        ActiveSkill activeSkill = default;
        activeSkill |= _fastAttackSkill.SkillUsed ? ActiveSkill.AttackSkill1 : 0;
        activeSkill |= _heavyAttackSkill.SkillUsed ? ActiveSkill.AttackSkill2 : 0;

        return new PlayerSnapshot
        {
            PlayerId = PlayerId,
            Position = _transform.Position.ToClientVector2(),
            ViewId = ViewId,
            Health = ByteFloat.FromFloat(_health.Health, 0, _health.MaxHealth),
            ActiveSkill = activeSkill,
            Keyboard = _inputComponent.PlayerInput.Keyboard
        };
    }
}