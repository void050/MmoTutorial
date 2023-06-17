using Game.Components.Skills;
using Shared;

namespace Game.Components;

public class NetworkPlayerBehaviour : Behaviour
{
    public ushort PlayerId;
    public byte ViewId = 1;
    private TransformBehaviour _transform = null!;
    private HealthBehaviour _health = null!;
    private FastAttackSkill _fastAttackSkill = null!;
    private HeavyAttackSkill _heavyAttackSkill = null!;

    protected override void Start()
    {
        _transform = GetRequiredBehaviour<TransformBehaviour>();
        _health = GetRequiredBehaviour<HealthBehaviour>();
        _fastAttackSkill = GetRequiredBehaviour<FastAttackSkill>();
        _heavyAttackSkill = GetRequiredBehaviour<HeavyAttackSkill>();
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
            ActiveSkill = activeSkill
        };
    }
}