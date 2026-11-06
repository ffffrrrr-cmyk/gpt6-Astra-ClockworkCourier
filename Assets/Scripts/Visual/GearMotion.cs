using UnityEngine;

namespace ClockworkCourier
{
    public sealed class GearMotion : MonoBehaviour
    {
        public float speed = 16;
        private void Update() { transform.Rotate(0, 0, speed * Time.deltaTime); }
    }
}
