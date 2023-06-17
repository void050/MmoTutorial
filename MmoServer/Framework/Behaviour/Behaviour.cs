namespace Game;

public abstract class Behaviour
{
    public GameObject GameObject { get; internal set; } = null!;

    protected internal virtual void Start()
    {
    }

    protected internal virtual void Update(float deltaTime)
    {
    }

    public void Destroy(GameObject gameObject)
    {
        gameObject.GameObjectSystem.Destroy(gameObject);
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