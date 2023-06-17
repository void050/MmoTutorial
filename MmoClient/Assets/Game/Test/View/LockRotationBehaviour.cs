using UnityEngine;

namespace Game
{
    public class LockRotationBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform _transform;
        

        private void LateUpdate()
        {
            _transform.rotation = Quaternion.identity;
        }
    }
}