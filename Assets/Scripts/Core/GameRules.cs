using System;
using System.Collections.Generic;

namespace ClockworkCourier
{
    public enum RunPhase { Menu, Playing, Paused, Dead, Complete }
    public enum EnemyKind { Patrol, Turret, Drone, Warden }

    [Serializable]
    public sealed class ProgressData
    {
        public int version = 1;
        public int checkpoint;
        public bool doubleJump;
        public bool shortcutWest;
        public bool shortcutEast;
        public bool completed;
        public float bestTime;
        public float elapsed;
        public int salvage;
        public List<string> collected = new List<string>();

        public bool Valid()
        {
            return version == 1 && checkpoint >= 0 && checkpoint <= 5 && salvage >= 0 &&
                !float.IsNaN(elapsed) && !float.IsInfinity(elapsed) && elapsed >= 0 &&
                !float.IsNaN(bestTime) && !float.IsInfinity(bestTime) && bestTime >= 0 && collected != null;
        }
    }

    public sealed class VitalState
    {
        public const int MaximumHealth = 6;
        public int Health { get; private set; } = MaximumHealth;
        public float Energy { get; private set; } = 100;
        public float InvulnerableUntil { get; private set; }
        public float DashReadyAt { get; private set; }
        public bool Dead => Health <= 0;

        public bool Damage(int amount, float now)
        {
            if (Dead || now < InvulnerableUntil || amount <= 0) return false;
            Health = Math.Max(0, Health - amount);
            InvulnerableUntil = now + 1.15f;
            return true;
        }

        public bool Spend(float amount)
        {
            if (amount < 0 || Energy < amount) return false;
            Energy -= amount;
            return true;
        }

        public bool Dash(float now)
        {
            if (Dead || now < DashReadyAt) return false;
            DashReadyAt = now + 0.8f;
            return true;
        }

        public void Recharge(float amount) { Energy = Math.Clamp(Energy + amount, 0, 100); }
        public void Heal(int amount) { Health = Math.Clamp(Health + amount, 0, MaximumHealth); }
        public void Reset(float now = 0, float protection = 0)
        {
            Health = MaximumHealth;
            Energy = 100;
            InvulnerableUntil = now + protection;
            DashReadyAt = 0;
        }
    }

    public sealed class HitLedger
    {
        private readonly HashSet<object> targets = new HashSet<object>();
        public void BeginSwing() { targets.Clear(); }
        public bool TryHit(object target) { return targets.Add(target); }
    }

    public static class Campaign
    {
        public static readonly float[] CheckpointX = { 3, 35, 67, 102, 137, 171 };
        public static readonly string[] Regions = { "01  /  齿轮工坊", "02  /  升降井", "03  /  钟面核心" };
        public static int Region(float position) { return position < 64 ? 0 : position < 133 ? 1 : 2; }
        public static bool Collect(ProgressData progress, string identifier)
        {
            if (progress.collected.Contains(identifier)) return false;
            progress.collected.Add(identifier);
            progress.salvage++;
            return true;
        }
    }
}
