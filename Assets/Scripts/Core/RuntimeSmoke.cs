using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ClockworkCourier
{
    public sealed class RuntimeSmoke : MonoBehaviour
    {
        private readonly List<string> checks = new List<string>();
        private string output;
        private bool failed;
        private IEnumerator Start()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int destination = Array.IndexOf(arguments, "-u2Output");
            output = destination >= 0 && destination + 1 < arguments.Length ? arguments[destination + 1] : Application.persistentDataPath;
            Directory.CreateDirectory(output);
            yield return null;
            Check(GameSession.Instance.Phase == RunPhase.Menu, "Initial main menu");
            var session = GameSession.Instance;
            session.Begin(true);
            yield return null;
            yield return new WaitForSeconds(0.3f);
            Check(session.Hero != null && session.Phase == RunPhase.Playing, "Fresh game starts");
            Check(FindObjectsByType<EnemyActor>().Length >= 20, "Enemy roster instantiated");
            Capture("01-workshop.png");
            File.WriteAllText(Path.Combine(output, "performance-sample.txt"), "Status: ENVIRONMENT_LIMITED\nWindow presentation is suppressed in hidden/background validation. Update-loop timing is not a valid rendered FPS measurement.\nNo 1080p 60 FPS claim is made.\nActual Camera.Render screenshots verify offscreen rendering only.\nVisible-window gameplay performance and UI remain pending user testing.\n");
            float elapsed = session.Progress.elapsed;
            Vector3 position = session.Hero.transform.position;
            var lift = FindAnyObjectByType<MovingPlatform>();
            Vector3 liftPosition = lift.transform.position;
            session.Pause();
            yield return new WaitForSecondsRealtime(0.4f);
            Check(Mathf.Approximately(session.Progress.elapsed, elapsed), "Pause freezes game timer");
            Check(Vector3.Distance(position, session.Hero.transform.position) < 0.001f, "Pause freezes courier");
            Check(Vector3.Distance(liftPosition, lift.transform.position) < 0.001f, "Pause freezes lift");
            session.Resume();
            session.UnlockJump();
            session.Checkpoint(3);
            session.Retry();
            yield return null;
            yield return new WaitForSeconds(0.3f);
            Check(session.Progress.doubleJump, "Retry preserves double jump");
            Check(Mathf.Abs(session.Hero.transform.position.x - Campaign.CheckpointX[3]) < 0.1f, "Retry restores checkpoint position");
            Capture("02-lift-shaft.png");
            EnemyActor enemy = null;
            foreach (var candidate in FindObjectsByType<EnemyActor>()) if (candidate.kind == EnemyKind.Patrol) { enemy = candidate; break; }
            int salvage = session.Progress.salvage;
            enemy.TakeHit(100, session.Hero.transform.position);
            enemy.TakeHit(100, session.Hero.transform.position);
            Check(session.Progress.salvage == salvage + 1, "Enemy death rewarded once");
            session.Hero.Teleport(new Vector3(184, 0.1f, 0));
            Camera.main.GetComponent<CourierCamera>().Snap();
            yield return new WaitForSeconds(0.2f);
            Capture("03-clock-core.png");
            EnemyActor warden = null;
            foreach (var actor in FindObjectsByType<EnemyActor>()) if (actor.kind == EnemyKind.Warden) warden = actor;
            Check(warden != null, "Two-stage warden exists");
            if (warden != null) warden.TakeHit(100, session.Hero.transform.position);
            Check(session.WardenDefeated, "Warden unlocks delivery");
            session.Finish();
            Check(session.Phase == RunPhase.Complete && session.Progress.completed, "Delivery reaches completion state");
            var saved = SaveStore.Load();
            Check(saved.completed && saved.doubleJump, "Completion and ability persist on disk");
            File.WriteAllLines(Path.Combine(output, "runtime-checks.txt"), checks);
            File.WriteAllText(Path.Combine(output, "runtime-environment.txt"), "Unity: " + Application.unityVersion + "\nOS: " + SystemInfo.operatingSystem + "\nCPU: " + SystemInfo.processorType + "\nGPU: " + SystemInfo.graphicsDeviceName + "\nGraphics API: " + SystemInfo.graphicsDeviceType + "\nResolution: " + Screen.width + "x" + Screen.height + "\nScreenshot kind: actual Camera.Render output; excludes IMGUI overlays.\nGameplay feel and full traversal: not tested by user.\n");
            Debug.Log(failed ? "U2_RUNTIME_FAILED" : "U2_RUNTIME_PASSED");
            Application.Quit(failed ? 1 : 0);
        }

        private void Check(bool condition, string name)
        {
            checks.Add((condition ? "PASS" : "FAIL") + " | " + name);
            if (!condition) failed = true;
        }

        private void Capture(string name)
        {
            var camera = Camera.main;
            var target = new RenderTexture(1440, 900, 24);
            var prior = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(1440, 900, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1440, 900), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(output, name), image.EncodeToPNG());
            camera.targetTexture = previousTarget;
            RenderTexture.active = prior;
            target.Release();
            Destroy(target);
            Destroy(image);
        }
    }
}
