using System;
using UnityEngine;

namespace ClockworkCourier
{
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }
        public RunPhase Phase { get; private set; } = RunPhase.Menu;
        public ProgressData Progress { get; private set; }
        public CourierController Hero { get; private set; }
        public bool ScreenShake { get; set; } = true;
        public float Volume { get; set; } = 0.65f;
        public string Toast { get; private set; }
        public float ToastUntil { get; private set; }
        public bool Challenge { get; private set; }
        public bool WardenDefeated { get; private set; }
        public int RunDeaths { get; private set; }
        public float RunClock { get; private set; }
        private GameObject world;
        private float saveAt;
        private bool initialized;
        private float hitStopUntil;
        private float deathFreezeAt;

        private void Awake()
        {
            Instance = this;
            if (Array.Exists(Environment.GetCommandLineArgs(), argument => argument == "-u2Smoke")) Application.runInBackground = true;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
            Volume = PlayerPrefs.GetFloat("U2.Volume", 0.65f);
            ScreenShake = PlayerPrefs.GetInt("U2.Shake", 1) == 1;
            AudioListener.volume = Volume;
            Progress = SaveStore.Load();
            gameObject.AddComponent<SoundDesign>();
            gameObject.AddComponent<GameHud>();
        }

        private void Start()
        {
            var existing = GameObject.Find("World");
            if (existing != null) { existing.SetActive(false); Destroy(existing); }
            world = WorldBuilder.Build(Progress);
            Hero = world.GetComponentInChildren<CourierController>();
            Hero.Teleport(new Vector3(Campaign.CheckpointX[Progress.checkpoint], 0.1f, 0));
            WorldBuilder.EnsurePresentation();
            var follow = Camera.main.GetComponent<CourierCamera>();
            if (follow == null) follow = Camera.main.gameObject.AddComponent<CourierCamera>();
            follow.target = Hero.transform;
            follow.Snap();
            FrameMenu();
            initialized = true;
            Time.timeScale = 0;
            if (SaveStore.LastWarning != null) Notify(SaveStore.LastWarning, 9);
            Debug.Log("U2 runtime: " + SystemInfo.operatingSystem + " | " + SystemInfo.processorType + " | " + SystemInfo.graphicsDeviceName + " | Unity " + Application.unityVersion);
            if (Array.Exists(Environment.GetCommandLineArgs(), argument => argument == "-u2Smoke")) gameObject.AddComponent<RuntimeSmoke>();
        }

        private void Update()
        {
            AudioListener.volume = Volume;
            if (!initialized) return;
            if (Phase == RunPhase.Dead)
            {
                Time.timeScale = Time.unscaledTime < deathFreezeAt ? 1 : 0;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) && !GameHud.ModalOpen)
            {
                if (Phase == RunPhase.Playing) Pause();
                else if (Phase == RunPhase.Paused) Resume();
            }
            if (Phase != RunPhase.Playing) return;
            Time.timeScale = Time.unscaledTime < hitStopUntil ? 0.08f : 1;
            Progress.elapsed += Time.deltaTime;
            RunClock += Time.deltaTime;
            if (Time.unscaledTime > saveAt && !Challenge)
            {
                saveAt = Time.unscaledTime + 20;
                SaveStore.Write(Progress);
            }
        }

        public void Begin(bool fresh, int checkpoint = -1)
        {
            Challenge = checkpoint >= 0;
            if (fresh) Progress = new ProgressData();
            if (Challenge)
            {
                Progress = JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(SaveStore.Load()));
                Progress.checkpoint = checkpoint;
                Progress.doubleJump = true;
            }
            WardenDefeated = false;
            RunDeaths = 0;
            RunClock = 0;
            Rebuild();
            Notify(Progress.doubleJump ? "能源芯已校准。向钟面核心前进。" : "A / D 移动 · 空格跳跃 · J 挥刃 · K 精准格挡", 7);
        }

        private void Rebuild()
        {
            Time.timeScale = 1;
            if (world != null) { world.SetActive(false); Destroy(world); }
            world = WorldBuilder.Build(Progress);
            Hero = world.GetComponentInChildren<CourierController>();
            Hero.Teleport(new Vector3(Campaign.CheckpointX[Progress.checkpoint], 0.1f, 0));
            var follow = Camera.main.GetComponent<CourierCamera>();
            Camera.main.orthographicSize = 5.9f;
            follow.target = Hero.transform;
            follow.Snap();
            Phase = RunPhase.Playing;
            saveAt = Time.unscaledTime + 20;
            Hero.LockInputUntilRelease();
        }

        public void Retry() { WardenDefeated = false; Rebuild(); }
        public void Pause() { Phase = RunPhase.Paused; Time.timeScale = 0; Hero.ClearInput(); }
        public void Resume() { Phase = RunPhase.Playing; Time.timeScale = 1; Hero.LockInputUntilRelease(); }
        public void Menu()
        {
            if (!Challenge) SaveStore.Write(Progress);
            PlayerPrefs.SetFloat("U2.Volume", Volume);
            PlayerPrefs.SetInt("U2.Shake", ScreenShake ? 1 : 0);
            PlayerPrefs.Save();
            Phase = RunPhase.Menu;
            Time.timeScale = 0;
            FrameMenu();
        }

        private void FrameMenu()
        {
            Camera.main.orthographicSize = 4;
            Camera.main.transform.position = new Vector3(Hero.transform.position.x - 3.1f, 3.25f, -17);
        }

        public void ImpactPause() { hitStopUntil = Time.unscaledTime + 0.045f; }

        public void Die()
        {
            if (Phase != RunPhase.Playing) return;
            RunDeaths++;
            Phase = RunPhase.Dead;
            deathFreezeAt = Time.unscaledTime + 0.65f;
            Time.timeScale = 1;
            SoundDesign.Play("death");
        }

        public void Checkpoint(int identifier)
        {
            if (identifier == Progress.checkpoint && Hero.Vitals.Health == VitalState.MaximumHealth) return;
            Progress.checkpoint = identifier;
            Hero.Vitals.Reset();
            if (!Challenge) SaveStore.Write(Progress);
            Notify("中继站已记录 · 生命与电量已补满", 4);
            SoundDesign.Play("checkpoint");
        }

        public void UnlockJump()
        {
            if (Progress.doubleJump) return;
            Progress.doubleJump = true;
            if (!Challenge) SaveStore.Write(Progress);
            Notify("获得双级擒纵器！空中再按空格二段跳，可返回高处支路。", 8);
            SoundDesign.Play("checkpoint");
        }

        public void Collect(string identifier, Vector3 position)
        {
            if (!Campaign.Collect(Progress, identifier)) return;
            Hero.Vitals.Recharge(28);
            Hero.Vitals.Heal(1);
            Burst(position, ArtFactory.Teal, 9);
            SoundDesign.Play("pickup");
            if (!Challenge) SaveStore.Write(Progress);
        }

        public void DefeatWarden()
        {
            WardenDefeated = true;
            Notify("守门体已停止。穿过钟面，把能源芯送入右侧中枢。", 7);
            SoundDesign.Play("checkpoint");
        }

        public void Finish()
        {
            if (Phase != RunPhase.Playing || !WardenDefeated) return;
            Progress.completed = true;
            float result = Challenge ? RunClock : Progress.elapsed;
            if (!Challenge && (Progress.bestTime <= 0 || result < Progress.bestTime)) Progress.bestTime = result;
            if (!Challenge) SaveStore.Write(Progress);
            Phase = RunPhase.Complete;
            Time.timeScale = 0;
        }

        public void Notify(string message, float duration = 4) { Toast = message; ToastUntil = Time.unscaledTime + duration; }
        public void Burst(Vector3 position, Material material, int count = 7)
        {
            if (world == null) return;
            if (!ScreenShake) count = Mathf.Max(2, count / 3);
            for (int index = 0; index < count; index++)
            {
                var spark = ArtFactory.Part(world.transform, "Impact fleck", position + Vector3.back * 0.3f, Vector3.one * UnityEngine.Random.Range(0.05f, 0.12f), material);
                spark.AddComponent<Ephemeral>().velocity = new Vector3(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(1f, 5f), -0.2f);
            }
        }

        private void OnApplicationQuit()
        {
            if (Progress != null && !Challenge) SaveStore.Write(Progress);
            Time.timeScale = 1;
        }
    }
}
