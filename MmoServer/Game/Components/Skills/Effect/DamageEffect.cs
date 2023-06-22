namespace Game.Components.Skills;

public class DamageEffect : BaseEffect<HealthComponent>
{
    public DamageEffect(float damage)
    {
        Damage = damage;
    }

    private float Damage { get; }


    protected override void Apply(HealthComponent component)
    {
        component.TakeDamage(Damage);
    }
}