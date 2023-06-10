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

    public override string ToString()
    {
        return GetType().Name;
    }
}