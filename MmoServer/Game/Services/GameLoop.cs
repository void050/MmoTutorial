using Shared;

namespace Game;

public class GameLoop
{
    private readonly GameObjectSystem _gameObjectSystem;

    public GameLoop(GameObjectSystem gameObjectSystem)
    {
        _gameObjectSystem = gameObjectSystem;
    }

    public async Task Run(CancellationToken ct)
    {
        float deltaTime = 1f / NetworkConfig.TickRate;
        while (!ct.IsCancellationRequested)
        {
            _gameObjectSystem.Update(deltaTime);
            await Task.Delay(TimeSpan.FromSeconds(deltaTime), ct);
        }
    }
}