using UnityEngine;

namespace ClockworkCourier
{
    public sealed class WorldInteractable : MonoBehaviour
    {
        public enum Kind { Checkpoint, DoubleJump, Battery, ShortcutWest, ShortcutEast, Terminal, Tutorial }
        public Kind kind;
        public int checkpoint;
        public string identifier;
        public string message;
        public string Prompt { get; private set; }
        public bool InRange { get; private set; }
        private bool shown;
        private float pulse;

        private void Update()
        {
            var session = GameSession.Instance;
            if (session == null || session.Phase != RunPhase.Playing || session.Hero == null) { InRange = false; return; }
            Vector3 difference = session.Hero.transform.position - transform.position;
            InRange = Mathf.Abs(difference.x) < 1.7f && Mathf.Abs(difference.y) < 2;
            pulse += Time.deltaTime;
            if (kind == Kind.Battery)
            {
                transform.GetChild(0).localRotation = Quaternion.Euler(0, pulse * 65, 0);
                if (InRange) { session.Collect(identifier, transform.position); gameObject.SetActive(false); }
                return;
            }
            if (kind == Kind.Tutorial)
            {
                if (InRange && !shown) { session.Notify(message, 6); shown = true; }
                return;
            }
            Prompt = kind switch
            {
                Kind.Checkpoint => "E  /  记录中继站并补满能源",
                Kind.DoubleJump => "E  /  安装双级擒纵器",
                Kind.ShortcutWest => "E  /  解锁工坊高架捷径",
                Kind.ShortcutEast => "E  /  解锁升降井捷径",
                Kind.Terminal => session.WardenDefeated ? "E  /  送达能源芯" : "核心封锁中 · 先击败守门体",
                _ => ""
            };
            if (!InRange || !Input.GetKeyDown(KeyCode.E)) return;
            switch (kind)
            {
                case Kind.Checkpoint: session.Checkpoint(checkpoint); break;
                case Kind.DoubleJump: session.UnlockJump(); gameObject.SetActive(false); break;
                case Kind.ShortcutWest: session.Progress.shortcutWest = true; OpenShortcut("West shortcut gate"); break;
                case Kind.ShortcutEast: session.Progress.shortcutEast = true; OpenShortcut("East shortcut gate"); break;
                case Kind.Terminal: session.Finish(); break;
            }
        }

        private void OpenShortcut(string gateName)
        {
            var gate = GameObject.Find(gateName);
            if (gate != null)
            {
                gate.SetActive(false);
                WorldBuilder.ShortcutBridge(transform.parent, kind == Kind.ShortcutWest ? 46 : 121);
            }
            if (!GameSession.Instance.Challenge) SaveStore.Write(GameSession.Instance.Progress);
            GameSession.Instance.Notify("捷径已解锁 · 地面快速通道已打开", 4);
            SoundDesign.Play("checkpoint");
        }
    }
}
