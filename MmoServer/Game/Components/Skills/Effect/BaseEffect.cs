namespace Game.Components.Skills;

public abstract class BaseEffect<TComponent> : ISkillEffect where TComponent : Component
{
    public void Apply(GameObject target)
    {
        var component = target.GetComponent<TComponent>();
        if (component != null)
        {
            Apply(component);
        }
    }

    protected abstract void Apply(TComponent behaviour);
}