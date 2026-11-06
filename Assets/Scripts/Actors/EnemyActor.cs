using UnityEngine;

namespace ClockworkCourier
{
    public sealed class EnemyActor : MonoBehaviour
    {
        public EnemyKind kind;
        public string identifier;
        public float patrolRange = 2;
        public int health = 3;
        public int maximumHealth = 3;
        public bool Dead => health <= 0;
        public bool Telegraphing => windingUp;
        private Vector3 home;
        private float nextAction;
        private float windupUntil;
        private bool windingUp;
        private float stunUntil;
        private float facing = -1;
        private int pattern;
        private MechanicalRig rig;
        private Transform indicator;
        private Vector3 shotTarget;

        private void Start()
        {
            home = transform.position;
            rig = GetComponentInChildren<MechanicalRig>();
            indicator = transform.Find("Attack indicator");
            nextAction = Time.time + 1;
        }

        private void Update()
        {
            var session = GameSession.Instance;
            if (session == null || session.Phase != RunPhase.Playing || session.Hero == null || Dead) return;
            var hero = session.Hero;
            Vector3 delta = hero.transform.position - transform.position;
            facing = delta.x >= 0 ? 1 : -1;
            if (indicator != null) indicator.gameObject.SetActive(windingUp);
            if (Time.time < stunUntil) { if (rig != null) rig.movement = 0; return; }
            if (kind == EnemyKind.Warden && hero.transform.position.x < 177) return;
            if (windingUp)
            {
                if (rig != null) { rig.guarding = true; rig.movement = 0; }
                if (Time.time >= windupUntil)
                {
                    ExecuteAttack();
                    windingUp = false;
                    nextAction = Time.time + (kind == EnemyKind.Warden ? (health <= maximumHealth / 2 ? 0.9f : 1.4f) : 1.4f);
                }
                return;
            }
            float range = kind == EnemyKind.Turret ? 11 : kind == EnemyKind.Drone ? 8 : kind == EnemyKind.Warden ? 13 : 1.9f;
            if (Mathf.Abs(delta.x) < range && Mathf.Abs(delta.y) < (kind == EnemyKind.Drone ? 7 : 4) && Time.time >= nextAction)
            {
                windingUp = true;
                windupUntil = Time.time + (kind == EnemyKind.Warden ? 0.85f : 0.65f);
                shotTarget = hero.transform.position + Vector3.up * 0.9f;
                SoundDesign.Play("guard");
            }
            float move = 0;
            if (kind == EnemyKind.Patrol)
            {
                float destination = Mathf.Abs(delta.x) < 7 ? Mathf.Clamp(hero.transform.position.x, home.x - patrolRange, home.x + patrolRange) : home.x + Mathf.Sin(Time.time * 0.7f) * patrolRange;
                move = Mathf.Abs(destination - transform.position.x) > 0.2f ? Mathf.Sign(destination - transform.position.x) : 0;
                if (Mathf.Abs(delta.x) > 1.4f) transform.position += Vector3.right * move * Time.deltaTime * 1.65f;
            }
            if (kind == EnemyKind.Drone) transform.position = home + new Vector3(Mathf.Sin(Time.time * 0.9f) * patrolRange, Mathf.Sin(Time.time * 1.7f) * 0.6f, 0);
            if (rig != null)
            {
                rig.facing = facing;
                rig.movement = move;
                rig.guarding = false;
                rig.attack = 0;
            }
        }

        private void ExecuteAttack()
        {
            var session = GameSession.Instance;
            if (session.Hero == null) return;
            if (kind == EnemyKind.Patrol)
            {
                if (Vector3.Distance(session.Hero.transform.position, transform.position) < 2.15f)
                {
                    bool hurt = session.Hero.ReceiveDamage(1, transform.position);
                    if (!hurt && session.Hero.Guarding) TakeHit(2, session.Hero.transform.position);
                }
            }
            else if (kind == EnemyKind.Warden)
            {
                pattern++;
                if (pattern % 3 == 0)
                {
                    for (int index = 0; index < (health <= maximumHealth / 2 ? 5 : 3); index++)
                    {
                        float angle = -20 + index * 10;
                        Vector3 aim = Quaternion.Euler(0, 0, angle) * (shotTarget - (transform.position + Vector3.up * 1.6f)).normalized;
                        Fire(aim, 7, 1.6f, false);
                    }
                    GameSession.Instance.Notify("守门体 · 散射蓄能结束，靠近或跳跃穿隙", 1.8f);
                }
                else
                {
                    Fire(new Vector3(facing, 0, 0), health <= maximumHealth / 2 ? 10 : 7, 0.4f, true);
                    if (health <= maximumHealth / 2) Fire(new Vector3(-facing, 0, 0), 8, 0.4f, true);
                    Camera.main.GetComponent<CourierCamera>().Shake(0.18f);
                }
            }
            else Fire((shotTarget - (transform.position + Vector3.up * 0.6f)).normalized, kind == EnemyKind.Drone ? 6 : 8, 0.6f, false);
            if (rig != null) { rig.attack = 0.5f; rig.guarding = false; }
            SoundDesign.Play("shot");
        }

        private void Fire(Vector3 direction, float speed, float height, bool wave)
        {
            var bullet = ArtFactory.Part(transform.parent, wave ? "Ground shockwave" : "Clockwork bolt", transform.position + Vector3.up * height, wave ? new Vector3(0.35f, 0.65f, 0.3f) : Vector3.one * 0.22f, ArtFactory.Red);
            var shot = bullet.AddComponent<EnemyProjectile>();
            shot.velocity = direction * speed;
            shot.owner = this;
            shot.wave = wave;
        }

        public void TakeHit(int damage, Vector3 source)
        {
            if (Dead) return;
            health = Mathf.Max(0, health - damage);
            stunUntil = Time.time + (kind == EnemyKind.Warden ? 0.1f : 0.35f);
            if (kind != EnemyKind.Warden) windingUp = false;
            if (rig != null) rig.hurt = 0.22f;
            GameSession.Instance.Burst(transform.position + Vector3.up, ArtFactory.Brass, 10);
            SoundDesign.Play("hit");
            GameSession.Instance.ImpactPause();
            if (Dead)
            {
                if (kind == EnemyKind.Warden) GameSession.Instance.DefeatWarden();
                if (Campaign.Collect(GameSession.Instance.Progress, "enemy-" + identifier))
                {
                    GameSession.Instance.Hero.Vitals.Recharge(20);
                }
                gameObject.SetActive(false);
            }
        }
    }

}
