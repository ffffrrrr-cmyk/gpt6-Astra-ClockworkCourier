using UnityEngine;

namespace ClockworkCourier
{
    public sealed class Breakable : MonoBehaviour
    {
        public int hits = 2;
        public void Hit()
        {
            hits--;
            GameSession.Instance.Burst(transform.position + Vector3.up * 0.6f, ArtFactory.Brass, 8);
            if (hits <= 0) gameObject.SetActive(false);
        }
    }
}
