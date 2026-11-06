using UnityEngine;

namespace ClockworkCourier
{
    public sealed class PressHazard : MonoBehaviour
    {
        public float offset;
        public Transform piston;
        public Transform lamp;
        private float clock;
        public bool Crushing { get; private set; }
        private void Update()
        {
            if (GameSession.Instance.Phase != RunPhase.Playing) return;
            clock += Time.deltaTime;
            float phase = (clock + offset) % 4.4f;
            float height = phase < 2.6f ? 3.8f : phase < 2.9f ? Mathf.Lerp(3.8f, 0.65f, (phase - 2.6f) / 0.3f) : phase < 3.4f ? 0.65f : Mathf.Lerp(0.65f, 3.8f, (phase - 3.4f));
            piston.localPosition = new Vector3(0, height, 0);
            lamp.gameObject.SetActive(phase > 2 && phase < 3.5f);
            Crushing = height < 1.55f;
            var hero = GameSession.Instance.Hero;
            if (Crushing && Mathf.Abs(hero.transform.position.x - transform.position.x) < 0.95f && hero.transform.position.y < 2.7f) hero.ReceiveDamage(2, transform.position + Vector3.up * 2);
        }
    }
}
