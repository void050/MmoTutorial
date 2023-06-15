#nullable enable
using Game;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    [Range(0.01f, 1f)] [SerializeField] private float _lerp;

    [Range(1f, 100f)] [SerializeField] private float _scaleMin;
    [Range(1f, 100f)] [SerializeField] private float _scaleMax;
    [Range(0, 1)] [SerializeField] private float _scale;
    [Range(0.01f, 1)] [SerializeField] private float _scrollSpeed;


    [SerializeField] private Transform _root = null!;

    public bool HasTarget { get; private set; }
    private Transform? _target;

    public void SetTarget(PlayerView playerView)
    {
        _target = playerView.transform;
        HasTarget = true;
        enabled = true;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            enabled = false;
            HasTarget = false;
            return;
        }

        _scale += Input.mouseScrollDelta.y * _scrollSpeed;
        _scale = Mathf.Clamp01(_scale);

        var cameraPosition = _root.position;
        Vector3 offset = _offset.normalized * ((1 - _scale) * (_scaleMax - _scaleMin) + _scaleMin);
        _root.position = Vector3.Lerp(cameraPosition, _target.position + offset, _lerp);
    }
}