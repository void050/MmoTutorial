#nullable enable
using UnityEngine;

namespace Game
{
    public class PlayerView : MonoBehaviour
    {
        private InterpolationHelper _interpolationHelper = null!;
        [SerializeField] private Transform _root = null!;
        [SerializeField] private Animator _animator = null!;
        private Vector3 _lastPosition;
        private static readonly int SpeedAnimatorProperty = Animator.StringToHash("Speed");
        private float _speed;

        public void Initialize(Vector3 position)
        {
            _interpolationHelper = new InterpolationHelper(position, Quaternion.identity);
            _root = transform;
            _lastPosition = position;
        }

        public void SetPosition(Vector3 position)
        {
            _interpolationHelper.SetPosition(position);
            var previousPosition = _interpolationHelper.PreviousPosition;
            if (position != previousPosition)
            {
                var lookRotation = Quaternion.LookRotation(position - previousPosition);
                _interpolationHelper.SetRotation(lookRotation);
            }
        }

        public void Update()
        {
            _interpolationHelper.Update(Time.deltaTime);
            _root.position = _interpolationHelper.ViewPosition;
            Vector3 viewRotationEuler = _interpolationHelper.ViewRotation.eulerAngles;
            Vector3 correctRotation = new Vector3(0, viewRotationEuler.y, 0);
            _root.eulerAngles = correctRotation;
            UpdateSpeedAnimationProperty();
            _lastPosition = _interpolationHelper.ViewPosition;
        }


        private void UpdateSpeedAnimationProperty()
        {
            Vector3 velocity = (_interpolationHelper.ViewPosition - _lastPosition) / Time.deltaTime;
            velocity.y = 0;
            float speed = velocity.magnitude;
            _speed = Mathf.Lerp(_speed, speed, 0.1f);

            _animator.SetFloat(SpeedAnimatorProperty, _speed / 4);
        }
    }
}