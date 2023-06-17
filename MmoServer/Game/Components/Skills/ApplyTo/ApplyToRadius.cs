using System.Numerics;

namespace Game.Components.Skills;

public class ApplyToRadius : ISkillApplyTo
{
    private readonly GameObject _source;
    private readonly TransformBehaviour _transform;
    private readonly List<TransformBehaviour> _objectsInRange = new();
    private readonly float _radius;

    public ApplyToRadius(GameObject source, float radius)
    {
        _radius = radius;
        _source = source;
        _transform = _source.GetRequiredBehaviour<TransformBehaviour>();
    }


    public void ApplyTo(ISkillEffect effect)
    {
        _objectsInRange.Clear();
        _source.GameObjectSystem.GetAll(_objectsInRange);
        foreach (var transformBehaviour in _objectsInRange)
        {
            if (transformBehaviour == _transform)
            {
                continue;
            }

            if (Vector2.Distance(_transform.Position, transformBehaviour.Position) < _radius)
            {
                effect.Apply(transformBehaviour.GameObject);
            }
        }
    }
}