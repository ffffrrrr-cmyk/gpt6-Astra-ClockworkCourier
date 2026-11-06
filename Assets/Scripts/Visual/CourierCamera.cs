using UnityEngine;

namespace ClockworkCourier
{
    public sealed class CourierCamera : MonoBehaviour
    {
        public Transform target;
        private Vector3 velocity;
        private float shake;
        public void Shake(float amount) { shake = amount; }
        private Vector3 Desired()
        {
            return new Vector3(Mathf.Clamp(target.position.x + 2.8f, 8, 203), Mathf.Max(4.7f, target.position.y + 3.3f), -17);
        }
        public void Snap()
        {
            if (target == null) return;
            transform.position = Desired();
            transform.rotation = Quaternion.Euler(8, 0, 0);
            velocity = Vector3.zero;
        }
        private void LateUpdate()
        {
            if (target == null || Time.timeScale == 0) return;
            transform.position = Vector3.SmoothDamp(transform.position, Desired(), ref velocity, 0.24f);
            if (shake > 0 && GameSession.Instance.ScreenShake)
            {
                transform.position += new Vector3(Mathf.Sin(Time.time * 83), Mathf.Cos(Time.time * 73), 0) * shake * 0.3f;
                shake -= Time.deltaTime;
            }
        }
    }
}
