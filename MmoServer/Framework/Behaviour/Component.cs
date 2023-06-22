using Framework.Physics;

namespace Game;

public abstract class Component
{
    public GameObject GameObject { get; internal set; } = null!;
    public GameObjectSystem GameObjectSystem { get; internal set; } = null!;
    public PhysicsWorld PhysicsWorld { get; internal set; } = null!;


    protected void Destroy(GameObject gameObject)
    {
        gameObject.GameObjectSystem.Destroy(gameObject);
    }

    protected T? GetComponent<T>() where T : Component
    {
        return GameObject.GetComponent<T>();
    }

    protected T GetRequiredComponent<T>() where T : Component
    {
        return GameObject.GetRequiredComponent<T>();
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}