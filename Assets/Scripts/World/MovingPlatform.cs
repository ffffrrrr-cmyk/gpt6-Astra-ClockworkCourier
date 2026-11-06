using UnityEngine;

namespace ClockworkCourier
{
    public sealed class MovingPlatform : MonoBehaviour
    {
        public Vector3 travel = new Vector3(0, 3.3f, 0);
        public float period = 5;
        private Vector3 origin;
        private float clock;
        private void Start() { origin = transform.position; }
        private void Update()
        {
            if (GameSession.Instance.Phase != RunPhase.Playing) return;
            Vector3 prior = transform.position;
            clock += Time.deltaTime;
            transform.position = origin + travel * (0.5f - 0.5f * Mathf.Cos(clock * Mathf.PI * 2 / period));
            var hero = GameSession.Instance.Hero;
            float width = GetComponent<BoxCollider>().size.x;
            bool onTop = Mathf.Abs(hero.transform.position.x - prior.x) < width / 2 + 0.15f && Mathf.Abs(hero.transform.position.y - (prior.y + 0.2f)) < 0.3f;
            Physics.SyncTransforms();
            if (onTop) hero.Carry(transform.position - prior);
        }
    }
}
