using System.Text;

namespace Game;

public class GameObject
{
    public readonly GameObjectSystem GameObjectSystem;

    public bool IsAlive { get; internal set; } = true;
    public string Name { get; set; }
    private readonly List<Behaviour> _behaviours = new();

    public GameObject(GameObjectSystem gameObjectSystem, string name, params Behaviour[] behaviours)
    {
        GameObjectSystem = gameObjectSystem;
        Name = name;
        foreach (var behaviour in behaviours)
        {
            AddComponent(behaviour);
        }

        GameObjectSystem.AddGameObject(this);
    }

    public void AddComponent<T>(T behaviour) where T : Behaviour
    {
        behaviour.GameObject = this;
        _behaviours.Add(behaviour);
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

    public T? GetBehaviour<T>() where T : Behaviour
    {
        foreach (var behaviour in _behaviours)
        {
            if (behaviour is T typedBehaviour)
            {
                return typedBehaviour;
            }
        }

        return null;
    }

    public T GetRequiredBehaviour<T>() where T : Behaviour
    {
        var behaviour = GetBehaviour<T>();
        if (behaviour == null)
        {
            throw new Exception($"Behaviour of type {typeof(T)} not found on GameObject {Name}");
        }

        return behaviour;
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