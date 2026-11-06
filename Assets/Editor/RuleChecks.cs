using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ClockworkCourier.Editor
{
    public static class RuleChecks
    {
        [MenuItem("Clockwork Courier/Run rule checks")]
        public static void Run()
        {
            var results = new List<string>();
            var ledger = new HitLedger();
            ledger.BeginSwing();
            Check(ledger.TryHit(42) && !ledger.TryHit(42) && ledger.TryHit(43), "One hit per target per swing", results);
            ledger.BeginSwing();
            Check(ledger.TryHit(42), "Next swing can hit again", results);
            var health = new VitalState();
            Check(health.Damage(1, 5) && !health.Damage(1, 5.1f) && health.Health == 5, "Invulnerability prevents duplicate damage", results);
            Check(health.Damage(9, 7) && health.Dead && !health.Damage(1, 10), "Death is terminal until reset", results);
            health.Reset();
            Check(health.Health == 6 && health.Energy == 100 && !health.Dead, "Checkpoint resets health and energy", results);
            Check(health.Dash(1) && !health.Dash(1.2f) && health.Dash(1.81f), "Dash cooldown", results);
            Check(health.Spend(100) && !health.Spend(1) && health.Dash(3), "Empty energy still permits basic dash", results);
            health.Recharge(500);
            Check(health.Energy == 100, "Energy clamped to maximum", results);
            var progress = new ProgressData { doubleJump = true, checkpoint = 3, shortcutWest = true };
            Check(Campaign.Collect(progress, "enemy-a") && !Campaign.Collect(progress, "enemy-a") && progress.salvage == 1, "Retry cannot duplicate rewards", results);
            var restored = SaveStore.Decode(JsonUtility.ToJson(progress));
            Check(restored.doubleJump && restored.checkpoint == 3 && restored.shortcutWest && restored.collected.Contains("enemy-a"), "Progress round trip", results);
            bool rejected = false;
            try { SaveStore.Decode("{\"version\":99,\"checkpoint\":999}"); } catch { rejected = true; }
            Check(rejected, "Invalid save rejected", results);
            bool malformed = false;
            try { SaveStore.Decode("not-json"); } catch { malformed = true; }
            Check(malformed, "Corrupt save rejected", results);
            Check(UnityEngine.Object.FindObjectsByType<CourierController>().Length == 1, "Scene has exactly one courier", results);
            Check(UnityEngine.Object.FindObjectsByType<GameSession>().Length == 1, "Scene has exactly one session", results);
            Check(Resources.Load<GameTuning>("GameTuning") != null, "Tuning asset available to runtime", results);
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                    if (component == null) throw new InvalidOperationException("Missing script in scene " + root.name);
            results.Add("PASS | Scene has no missing behaviour references");
            foreach (float position in Campaign.CheckpointX)
            {
                bool floor = Physics.Raycast(new Vector3(position, 3, 0), Vector3.down, 4, 1 << 8, QueryTriggerInteraction.Ignore);
                Check(floor, "Checkpoint has safe floor at " + position, results);
            }
            Directory.CreateDirectory("Validation");
            File.WriteAllLines("Validation/rule-checks.txt", results);
            Debug.Log("U2_RULE_CHECKS_PASSED: " + results.Count);
        }

        private static void Check(bool condition, string name, List<string> results)
        {
            if (!condition) throw new InvalidOperationException("U2 rule check failed: " + name);
            results.Add("PASS | " + name);
        }
    }
}
