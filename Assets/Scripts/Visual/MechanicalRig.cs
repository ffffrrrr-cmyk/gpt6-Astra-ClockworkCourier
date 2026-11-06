using UnityEngine;

namespace ClockworkCourier
{
    public sealed class MechanicalRig : MonoBehaviour
    {
        public Transform body;
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform leftArm;
        public Transform rightArm;
        public Transform weapon;
        public float facing = 1;
        public float movement;
        public bool airborne;
        public bool guarding;
        public bool defeated;
        public float attack;
        public float hurt;
        private float clock;

        private void Update()
        {
            if (body == null) return;
            clock += Time.deltaTime;
            float stride = Mathf.Sin(clock * 11) * Mathf.Clamp01(Mathf.Abs(movement)) * 36;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facing, transform.localScale.y, transform.localScale.z);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, facing > 0 ? -15 : 15, defeated ? 78 : 0), Time.deltaTime * 16);
            body.localPosition = new Vector3(0, 1.03f + Mathf.Sin(clock * 22) * Mathf.Abs(movement) * 0.025f, 0);
            body.localRotation = Quaternion.Euler(0, 0, hurt > 0 ? -18 : airborne ? -8 : 0);
            leftLeg.localRotation = Quaternion.Euler(0, 0, airborne ? -25 : stride);
            rightLeg.localRotation = Quaternion.Euler(0, 0, airborne ? 32 : -stride);
            leftArm.localRotation = Quaternion.Euler(0, 0, guarding ? -100 : -stride * 0.8f);
            rightArm.localRotation = Quaternion.Euler(0, 0, guarding ? 85 : attack > 0 ? Mathf.Lerp(120, -70, attack) : stride * 0.7f - 12);
            hurt = Mathf.Max(0, hurt - Time.deltaTime);
        }
    }

}
