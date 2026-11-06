using UnityEngine;

namespace ClockworkCourier
{
    [CreateAssetMenu(menuName = "Clockwork Courier/Game tuning")]
    public sealed class GameTuning : ScriptableObject
    {
        public float runSpeed = 5.8f;
        public float gravity = 27;
        public float jumpSpeed = 10.6f;
        public float secondJumpSpeed = 10.2f;
        public float coyoteWindow = 0.11f;
        public float inputBuffer = 0.12f;
        public float dashDuration = 0.17f;
        public float boostedDashSpeed = 19;
        public float basicDashSpeed = 13;
        public float swingDuration = 0.48f;
        public float parryWindow = 0.22f;
        public float passiveRecharge = 2.4f;
        private static GameTuning active;
        public static GameTuning Active
        {
            get
            {
                if (active == null) active = Resources.Load<GameTuning>("GameTuning");
                if (active == null) active = CreateInstance<GameTuning>();
                return active;
            }
        }
    }
}
