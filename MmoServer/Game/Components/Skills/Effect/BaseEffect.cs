namespace Game.Components.Skills;

public abstract class BaseEffect<TBehaviour> : ISkillEffect where TBehaviour : Behaviour
{
    public void Apply(GameObject target)
    {
        var behaviour = target.GetBehaviour<TBehaviour>();
        if (behaviour != null)
        {
            Apply(behaviour);
        }
    }

    protected abstract void Apply(TBehaviour behaviour);
}