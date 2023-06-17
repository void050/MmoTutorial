namespace Game.Components.Skills;

public class DamageEffect : BaseEffect<HealthBehaviour>
{
    public DamageEffect(float damage)
    {
        Damage = damage;
    }

    private float Damage { get; }


    protected override void Apply(HealthBehaviour behaviour)
    {
        behaviour.TakeDamage(Damage);
    }
}