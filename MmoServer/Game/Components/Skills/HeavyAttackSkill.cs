using Shared;

namespace Game.Components.Skills;

public class HeavyAttackSkill: SkillBehaviour
{
    protected override PlayerKeyboard UseKey => PlayerKeyboard.AttackSkill2;

    protected override void Start()
    {
        base.Start();
        Init(
            new ApplyToRadius(GameObject, GameConfig.HeavyAttackRadius),
            new DamageEffect(GameConfig.HeavyAttackDamage),
            GameConfig.HeavyAttackCooldown);
    }
}