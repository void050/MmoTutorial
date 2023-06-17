namespace Game;

public class GameObjectSystem
{
    private readonly List<GameObject> _gameObjects = new();
    private readonly List<GameObject> _toAddGameObjects = new();
    private readonly List<GameObject> _iterateGameObjects = new();


    internal void AddGameObject(GameObject gameObject)
    {
        lock (_toAddGameObjects)
        {
            _toAddGameObjects.Add(gameObject);
        }
    }

    public void Update(float deltaTime)
    {
        _iterateGameObjects.Clear();
        lock (_toAddGameObjects)
        {
            _iterateGameObjects.AddRange(_toAddGameObjects);
            _toAddGameObjects.Clear();
        }

        _gameObjects.AddRange(_iterateGameObjects);
        foreach (var iterateGameObject in _iterateGameObjects)
        {
            iterateGameObject.Start();
        }

        _iterateGameObjects.Clear();
        _iterateGameObjects.AddRange(_gameObjects);

        foreach (var gameObject in _iterateGameObjects)
        {
            gameObject.Update(deltaTime);
        }
    }

    public void GetAll<T>(List<T> result) where T : Behaviour
    {
        foreach (var gameObject in _gameObjects)
        {
            var behaviour = gameObject.GetBehaviour<T>();
            if (behaviour != null)
            {
                result.Add(behaviour);
            }
        }
    }

    internal void Destroy(GameObject gameObject)
    {
        _gameObjects.Remove(gameObject);
        gameObject.IsAlive = false;
    }
}