using Shared;

namespace Game.Components;

public class NetworkPlayerBehaviour : Behaviour
{
    public ushort PlayerId;
    public byte ViewId = 1;
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
            Position = _transform.Position.ToClientVector2(),
            ViewId = ViewId
        };
    }
}