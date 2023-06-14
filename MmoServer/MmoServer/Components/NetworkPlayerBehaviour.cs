using Riptide;
using Shared;

namespace Game.Components;

public class NetworkPlayerBehaviour : Behaviour
{
    public ushort PlayerId;
    public Connection? Connection;
    private TransformBehaviour _transform = null!;

    protected override void Start()
    {
        _transform = GetRequiredBehaviour<TransformBehaviour>();
    }

    public PlayerSnapshot GetSnapshot()
    {
        return new PlayerSnapshot
        {
            PlayerId = PlayerId,
            Position = _transform.Position.ToClientVector2()
        };
    }
}