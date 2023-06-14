namespace Game;

public class Behaviour
{
    public GameObject GameObject { get; internal set; }

    protected internal virtual void Start()
    {
    }

    protected internal virtual void Update(float deltaTime)
    {
    }

    public T? GetBehaviour<T>() where T : Behaviour
    {
        return GameObject.GetBehaviour<T>();
    }

    public T GetRequiredBehaviour<T>() where T : Behaviour
    {
        return GameObject.GetRequiredBehaviour<T>();
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}