using UnityEngine;

namespace ClockworkCourier
{
    public sealed class EnemyProjectile : MonoBehaviour
    {
        public Vector3 velocity;
        public EnemyActor owner;
        public bool wave;
        private float life = 4;
        private void Update()
        {
            var session = GameSession.Instance;
            if (session.Phase != RunPhase.Playing) return;
            Vector3 prior = transform.position;
            Vector3 next = prior + velocity * Time.deltaTime;
            Vector3 target = session.Hero.transform.position + Vector3.up * (wave ? 0.45f : 0.9f);
            Vector3 segment = next - prior;
            float projection = segment.sqrMagnitude < 0.000001f ? 0 : Mathf.Clamp01(Vector3.Dot(target - prior, segment) / segment.sqrMagnitude);
            if (Vector3.Distance(target, prior + segment * projection) < (wave ? 0.6f : 0.7f))
            {
                bool guarding = session.Hero.Guarding && Mathf.Sign((prior - velocity.normalized).x - session.Hero.transform.position.x) == session.Hero.Facing;
                bool damaged = session.Hero.ReceiveDamage(1, prior - velocity.normalized);
                if (!damaged && guarding && owner != null) owner.TakeHit(owner.kind == EnemyKind.Warden ? 2 : 3, target);
                Destroy(gameObject);
                return;
            }
            if (!wave && Physics.Linecast(prior, next, 1 << 8, QueryTriggerInteraction.Ignore)) { Destroy(gameObject); return; }
            transform.position = next;
            life -= Time.deltaTime;
            if (life <= 0) Destroy(gameObject);
        }
    }
}
