using System.Text;
using Framework.Physics;

namespace Game;

public class GameObject
{
    public readonly GameObjectSystem GameObjectSystem;
    public readonly PhysicsWorld PhysicsWorld;

    public bool IsAlive { get; internal set; } = true;
    public string Name { get; set; }
    private readonly List<Behaviour> _behaviours = new();
    private readonly List<Component> _components = new();

    public GameObject(GameObjectSystem gameObjectSystem, PhysicsWorld physicsWorld, string name, params Component[] components)
    {
        GameObjectSystem = gameObjectSystem;
        PhysicsWorld = physicsWorld;
        Name = name;
        foreach (var component in components)
        {
            AddComponent(component);
        }

        GameObjectSystem.AddGameObject(this);
    }

    public void AddComponent<T>(T component) where T : Component
    {
        component.GameObject = this;
        component.PhysicsWorld = PhysicsWorld;
        component.GameObjectSystem = GameObjectSystem;
        _components.Add(component);
        if (component is Behaviour behaviour)
        {
            _behaviours.Add(behaviour);
        }
    }

    internal void Start()
    {
        foreach (var behaviour in _behaviours)
        {
            try
            {
                behaviour.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in Start() of {behaviour.GetType().Name}: {e.Message}");
            }
        }
    }

    internal void Update(float deltaTime)
    {
        foreach (var behaviour in _behaviours)
        {
            try
            {
                behaviour.Update(deltaTime);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in Update() of {behaviour.GetType().Name}: {e.Message}");
            }
        }
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var component in _components)
        {
            if (component is T typedComponent)
            {
                return typedComponent;
            }
        }

        return null;
    }

    public T GetRequiredComponent<T>() where T : Component
    {
        var component = GetComponent<T>();
        if (component == null)
        {
            throw new Exception($"Component of type {typeof(T)} not found on GameObject {Name}");
        }

        return component;
    }


    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Name);
        sb.AppendLine($": {_behaviours.Count} components");
        foreach (var behaviour in _behaviours)
        {
            sb.AppendLine(behaviour.ToString());
        }

        return sb.ToString();
    }
}