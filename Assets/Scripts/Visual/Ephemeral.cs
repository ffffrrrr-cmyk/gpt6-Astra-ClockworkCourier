using UnityEngine;

namespace ClockworkCourier
{
    public sealed class Ephemeral : MonoBehaviour
    {
        public Vector3 velocity;
        public float remaining = 0.45f;
        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
            velocity += Vector3.down * 8 * Time.deltaTime;
            remaining -= Time.deltaTime;
            transform.localScale *= Mathf.Pow(0.1f, Time.deltaTime);
            if (remaining <= 0) Destroy(gameObject);
        }
    }
}
