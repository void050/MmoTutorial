#nullable enable
using Microlight.MicroBar;
using Shared;
using UnityEngine;

namespace Game
{
    public class PlayerView : MonoBehaviour
    {
        private InterpolationHelper _interpolationHelper = null!;
        [SerializeField] private Transform _root = null!;
        [SerializeField] private Animator _animator = null!;
        [SerializeField] private MicroBar _healthBar = null!;
        private Vector3 _lastPosition;
        private static readonly int SpeedAnimatorProperty = Animator.StringToHash("Speed");
        private static readonly int Skill1AnimatorProperty = Animator.StringToHash("Skill1");
        private static readonly int Skill2AnimatorProperty = Animator.StringToHash("Skill2");
        private float _speed;
        public float LastUpdated { get; private set; }

        public void Initialize(Vector3 position, float updateTime)
        {
            LastUpdated = updateTime;
            _interpolationHelper = new InterpolationHelper(position, Quaternion.identity);
            _root = transform;
            _lastPosition = position;
            _healthBar.Initialize(1f);
        }

        public void Synchronize(PlayerSnapshot snapshot, float updateTime)
        {
            Vector3 position = snapshot.Position.ToVector3();
            float health = snapshot.Health.Unpack(PlayerSnapshot.MinHealth, PlayerSnapshot.MaxHealth);
            LastUpdated = updateTime;
            const float minHealthToShowBar = 0.99f;
            _healthBar.gameObject.SetActive(health < minHealthToShowBar);
            _healthBar.UpdateHealthBar(health);
            _interpolationHelper.SetPosition(position);
            var previousPosition = _interpolationHelper.PreviousPosition;
            if (position != previousPosition)
            {
                var lookRotation = Quaternion.LookRotation(position - previousPosition);
                _interpolationHelper.SetRotation(lookRotation);
            }

            if ((snapshot.ActiveSkill & ActiveSkill.AttackSkill1) != 0)
            {
                _animator.SetTrigger(Skill1AnimatorProperty);
            }

            if ((snapshot.ActiveSkill & ActiveSkill.AttackSkill2) != 0)
            {
                _animator.SetTrigger(Skill2AnimatorProperty);
            }
        }

        public void Update()
        {
            _interpolationHelper.Update(Time.deltaTime);
            _root.position = _interpolationHelper.ViewPosition;
            Vector3 viewRotationEuler = _interpolationHelper.ViewRotation.eulerAngles;
            //rotation from animation
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Hurricane kick"))
            {
                Vector3 correctRotation = new Vector3(0, viewRotationEuler.y, 0);
                _root.eulerAngles = correctRotation;
            }

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