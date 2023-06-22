namespace Game;

public abstract class Behaviour : Component
{
    protected internal virtual void Start()
    {
    }

    protected internal virtual void OnDestroy()
    {
    }

    protected internal virtual void Update(float deltaTime)
    {
    }
}