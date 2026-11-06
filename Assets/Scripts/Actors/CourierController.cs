using UnityEngine;

namespace ClockworkCourier
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CourierController : MonoBehaviour
    {
        public VitalState Vitals { get; private set; } = new VitalState();
        public bool Grounded { get; private set; }
        public float Facing { get; private set; } = 1;
        public bool Guarding => Time.time < guardUntil;
        public float AttackProgress => attackUntil > Time.time ? 1 - (attackUntil - Time.time) / GameTuning.Active.swingDuration : 0;
        private CharacterController motor;
        private MechanicalRig rig;
        private readonly HitLedger ledger = new HitLedger();
        private readonly Collider[] hitBuffer = new Collider[24];
        private float vertical;
        private float horizontal;
        private float jumpQueuedAt = -10;
        private float lastGround = -10;
        private float dashUntil;
        private float dashSpeed;
        private float attackUntil;
        private float guardUntil;
        private float guardReady;
        private float recoilUntil;
        private float recoil;
        private int airJumps;
        private bool airDash;
        private bool inputLocked;
        private float landedAt;

        private void Awake()
        {
            motor = GetComponent<CharacterController>();
            rig = GetComponentInChildren<MechanicalRig>();
        }

        public void Teleport(Vector3 destination)
        {
            if (motor == null) motor = GetComponent<CharacterController>();
            motor.enabled = false;
            transform.position = destination;
            motor.enabled = true;
            vertical = 0;
            ClearInput();
            Vitals.Reset(Time.time, 2);
        }

        public void ClearInput() { horizontal = 0; jumpQueuedAt = -10; dashUntil = 0; attackUntil = 0; guardUntil = 0; }
        public void LockInputUntilRelease() { inputLocked = true; ClearInput(); }

        private void Update()
        {
            var session = GameSession.Instance;
            if (session == null || session.Phase != RunPhase.Playing) return;
            var tuning = GameTuning.Active;
            if (inputLocked)
            {
                if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.K) && !Input.GetMouseButton(0)) inputLocked = false;
                else return;
            }
            bool wasGrounded = Grounded;
            Grounded = motor.isGrounded || Physics.SphereCast(transform.position + Vector3.up * 0.24f, 0.22f, Vector3.down, out _, 0.27f, 1 << 8, QueryTriggerInteraction.Ignore);
            if (Grounded && vertical <= 0)
            {
                lastGround = Time.time;
                airJumps = 0;
                airDash = false;
                vertical = -2;
                if (!wasGrounded && Time.time > landedAt)
                {
                    SoundDesign.Play("land");
                    landedAt = Time.time + 0.2f;
                }
            }
            horizontal = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            if (Mathf.Abs(horizontal) > 0 && attackUntil <= Time.time && !Guarding) Facing = Mathf.Sign(horizontal);
            if (Input.GetKeyDown(KeyCode.Space)) jumpQueuedAt = Time.time;
            if (Time.time - jumpQueuedAt < tuning.inputBuffer && (Time.time - lastGround < tuning.coyoteWindow || (session.Progress.doubleJump && airJumps < 2)))
            {
                bool first = Time.time - lastGround < tuning.coyoteWindow;
                if (!first && airJumps == 0) airJumps = 1;
                airJumps++;
                vertical = first ? tuning.jumpSpeed : tuning.secondJumpSpeed;
                Grounded = false;
                lastGround = -10;
                jumpQueuedAt = -10;
                SoundDesign.Play("jump");
                if (!first) session.Burst(transform.position + Vector3.up * 0.3f, ArtFactory.Teal);
            }
            if (Input.GetKeyUp(KeyCode.Space) && vertical > 3) vertical *= 0.48f;
            if (Input.GetKeyDown(KeyCode.LeftShift) && !airDash && attackUntil <= Time.time && Vitals.Dash(Time.time))
            {
                dashUntil = Time.time + tuning.dashDuration;
                dashSpeed = Vitals.Spend(16) ? tuning.boostedDashSpeed : tuning.basicDashSpeed;
                if (!Grounded) airDash = true;
                SoundDesign.Play("dash");
            }
            if (Input.GetKeyDown(KeyCode.J) && attackUntil <= Time.time && dashUntil <= Time.time)
            {
                attackUntil = Time.time + tuning.swingDuration;
                ledger.BeginSwing();
                SoundDesign.Play("swing");
            }
            if (Input.GetKeyDown(KeyCode.K) && Time.time >= guardReady && attackUntil <= Time.time)
            {
                if (Vitals.Spend(10)) { guardUntil = Time.time + tuning.parryWindow; guardReady = Time.time + 0.55f; SoundDesign.Play("guard"); }
                else session.Notify("电量不足，先闪避；中继站或回收电池可补充电量。", 2);
            }
            float attackProgress = AttackProgress;
            if (attackProgress > 0.24f && attackProgress < 0.62f) Strike();
            float speed = horizontal * tuning.runSpeed * (attackUntil > Time.time ? 0.3f : Guarding ? 0.4f : 1);
            vertical -= tuning.gravity * Time.deltaTime;
            vertical = Mathf.Max(vertical, -25);
            if (dashUntil > Time.time) { speed = Facing * dashSpeed; vertical = 0; }
            if (recoilUntil > Time.time) speed = recoil;
            motor.Move(new Vector3(speed, vertical, 0) * Time.deltaTime);
            if ((motor.collisionFlags & CollisionFlags.Above) != 0 && vertical > 0) vertical = 0;
            if (transform.position.z != 0) transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            if (transform.position.y < -7) ReceiveDamage(9, transform.position + Vector3.left);
            Vitals.Recharge(Time.deltaTime * tuning.passiveRecharge);
            if (rig != null)
            {
                rig.facing = Facing;
                rig.movement = Grounded ? horizontal : 0;
                rig.airborne = !Grounded;
                rig.attack = attackProgress;
                rig.guarding = Guarding;
                rig.defeated = Vitals.Dead;
            }
        }

        private void Strike()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(Facing * 0.95f, 0.95f, 0), 1.05f, hitBuffer, ~0, QueryTriggerInteraction.Collide);
            for (int index = 0; index < count; index++)
            {
                var enemy = hitBuffer[index].GetComponentInParent<EnemyActor>();
                if (enemy != null && ledger.TryHit(enemy)) enemy.TakeHit(1, transform.position);
                var barrier = hitBuffer[index].GetComponentInParent<Breakable>();
                if (barrier != null && ledger.TryHit(barrier)) barrier.Hit();
            }
        }

        public bool ReceiveDamage(int amount, Vector3 source)
        {
            if (GameSession.Instance.Phase != RunPhase.Playing) return false;
            if (Guarding && Mathf.Sign(source.x - transform.position.x) == Facing && amount < 9)
            {
                Vitals.Recharge(26);
                GameSession.Instance.Burst(transform.position + Vector3.up, ArtFactory.Teal, 14);
                GameSession.Instance.Notify("精准格挡 · 电量回流", 1.2f);
                SoundDesign.Play("parry");
                return false;
            }
            if (!Vitals.Damage(amount, Time.time)) return false;
            recoil = Mathf.Sign(transform.position.x - source.x) * 5;
            recoilUntil = Time.time + 0.16f;
            vertical = 4;
            if (rig != null) rig.hurt = 0.28f;
            GameSession.Instance.Burst(transform.position + Vector3.up, ArtFactory.Brass);
            Camera.main.GetComponent<CourierCamera>().Shake(0.22f);
            SoundDesign.Play("hurt");
            if (Vitals.Dead) GameSession.Instance.Die();
            return true;
        }

        public void Carry(Vector3 displacement)
        {
            if (motor != null && motor.enabled && GameSession.Instance.Phase == RunPhase.Playing) motor.Move(displacement);
        }
    }
}
