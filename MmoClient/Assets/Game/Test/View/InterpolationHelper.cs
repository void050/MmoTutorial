using Shared;
using UnityEngine;

namespace Game
{
	public class InterpolationHelper
	{
		private const float MaxDistanceToLerp = 2f;
		private const float ServerDeltaTime = NetworkConfig.SnapshotInterval;
		private readonly float _maxPositionExtapolation;
		private readonly float _maxRotationExtapolation;

		private Vector3 _previousPosition;
		private Vector3 _actualPosition;
		private Vector3 _viewPosition;
		private Quaternion _previousRotation;
		private Quaternion _actualRotation;
		private Quaternion _viewRotation;
		private float _time;
		private float _rotationTime;
		public Vector3 PreviousPosition => _previousPosition;
		public Vector3 ActualPosition => _actualPosition;
		public Quaternion ActualRotation => _actualRotation;

		public Vector3 ViewPosition => _viewPosition;
		public Quaternion ViewRotation => _viewRotation;


		public InterpolationHelper(Vector3 positionValue,
				Quaternion rotationValue,
				float maxPositionExtapolation = 1,
				float maxRotationExtapolation = 1)
		{
			_actualPosition = positionValue;
			_actualRotation = rotationValue;
			_maxPositionExtapolation = maxPositionExtapolation;
			_maxRotationExtapolation = maxRotationExtapolation;
		}

		public void SetRotation(Quaternion rotation)
		{
			_previousRotation = _viewRotation;
			_actualRotation = rotation;
			_rotationTime = 0;
		}

		public void SetPosition(Vector3 position)
		{
			_previousPosition = _viewPosition;
			_actualPosition = position;
			_time = 0;

			Vector2 previousPosition2D = new(_previousPosition.x, _previousPosition.z);
			Vector2 acutalPosition2D = new(_actualPosition.x, _actualPosition.z);

			//Ignore Y because it's jump, or fall and can be lerped pretty
			if ((previousPosition2D - acutalPosition2D).magnitude > MaxDistanceToLerp)
			{
				_previousPosition = _actualPosition;
			}
			_time = 0;
		}

		public void Update(float deltaTime)
		{
			_time += deltaTime;
			_rotationTime += deltaTime;
			float way = _time / ServerDeltaTime;
			float positionWay = Mathf.Clamp(way, 0, _maxPositionExtapolation);
			float rotationWay = Mathf.Clamp(_rotationTime/ ServerDeltaTime, 0, _maxRotationExtapolation);
			_viewPosition = Vector3.LerpUnclamped(_previousPosition, _actualPosition, positionWay);
			_viewRotation = Quaternion.Slerp(_previousRotation, _actualRotation, rotationWay);
		}
	}
}