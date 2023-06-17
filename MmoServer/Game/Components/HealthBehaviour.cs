namespace Game.Components;

public class HealthBehaviour : Behaviour
{
    public float MaxHealth;
    public float Health { get; private set; }

    public HealthBehaviour(float health, float maxHealth)
    {
        Health = health;
        MaxHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            Destroy(GameObject);
        }
    }
}