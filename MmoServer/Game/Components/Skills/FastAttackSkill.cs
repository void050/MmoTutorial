using Shared;

namespace Game.Components.Skills;

public class FastAttackSkill : SkillBehaviour
{
    protected override PlayerKeyboard UseKey => PlayerKeyboard.AttackSkill1;

    protected override void Start()
    {
        base.Start();
        Init(
            new ApplyToRadius(GameObject, GameConfig.FastAttackRadius),
            new DamageEffect(GameConfig.FastAttackDamage),
            GameConfig.FastAttackCooldown);
    }
}