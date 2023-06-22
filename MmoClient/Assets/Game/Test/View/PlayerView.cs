#nullable enable
using Microlight.MicroBar;
using Shared;
using UnityEngine;

namespace Game
{
    public class PlayerView : MonoBehaviour
    {
        private const PlayerKeyboard AnyMovement =
            PlayerKeyboard.Up | PlayerKeyboard.Down | PlayerKeyboard.Left | PlayerKeyboard.Right;

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
        private PlayerKeyboard _keyboard;
        private Quaternion _rotation;
        private Quaternion _targetRotation;

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
            _keyboard = snapshot.Keyboard;
            Vector3 position = snapshot.Position.ToVector3();
            float health = snapshot.Health.Unpack(PlayerSnapshot.MinHealth, PlayerSnapshot.MaxHealth);
            LastUpdated = updateTime;
            const float minHealthToShowBar = 0.99f;
            _healthBar.gameObject.SetActive(health < minHealthToShowBar);
            _healthBar.UpdateHealthBar(health);
            _interpolationHelper.SetPosition(position);


            if ((snapshot.ActiveSkill & ActiveSkill.AttackSkill1) != 0)
            {
                _animator.SetTrigger(Skill1AnimatorProperty);
            }

            if ((snapshot.ActiveSkill & ActiveSkill.AttackSkill2) != 0)
            {
                _animator.SetTrigger(Skill2AnimatorProperty);
            }
        }

        private Vector2 GetMovementDirection(PlayerKeyboard keyboard)
        {
            Vector2 movementDirection = Vector2.zero;

            if ((keyboard & PlayerKeyboard.Up) != 0)
            {
                movementDirection += new Vector2(0, 1);
            }

            if ((keyboard & PlayerKeyboard.Down) != 0)
            {
                movementDirection += new Vector2(0, -1);
            }

            if ((keyboard & PlayerKeyboard.Left) != 0)
            {
                movementDirection += new Vector2(-1, 0);
            }

            if ((keyboard & PlayerKeyboard.Right) != 0)
            {
                movementDirection += new Vector2(1, 0);
            }

            float length = movementDirection.magnitude;
            if (length > 0)
            {
                movementDirection /= length;
            }

            return movementDirection;
        }

        public void Update()
        {
            _interpolationHelper.Update(Time.deltaTime);
            _root.position = _interpolationHelper.ViewPosition;

            if ((_keyboard & AnyMovement) != 0)
            {
                Vector2 movementDirection = GetMovementDirection(_keyboard);
                float angle = Mathf.Atan2(movementDirection.x, movementDirection.y) * Mathf.Rad2Deg;
                _targetRotation = Quaternion.Euler(0, angle, 0);
            }

            //rotation from animation
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Hurricane kick"))
            {
                const float angleSpeed = 10f;
                _rotation = Quaternion.RotateTowards(_rotation, _targetRotation, angleSpeed);
                _root.rotation = _rotation;
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