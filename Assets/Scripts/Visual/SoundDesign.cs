using UnityEngine;
using System.Collections.Generic;

namespace ClockworkCourier
{
    public sealed class SoundDesign : MonoBehaviour
    {
        private static SoundDesign instance;
        private AudioSource source;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private float ambientClock;
        private int ambientNote;
        private void Awake()
        {
            instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            string[] names = { "jump", "land", "swing", "dash", "guard", "parry", "hurt", "death", "pickup", "checkpoint", "shot", "hit", "note" };
            float[] frequencies = { 380, 90, 190, 300, 660, 1040, 110, 70, 880, 550, 240, 150, 220 };
            for (int index = 0; index < names.Length; index++) clips[names[index]] = Synthesize(names[index], frequencies[index], names[index] == "checkpoint" ? 0.8f : 0.22f);
        }
        private static AudioClip Synthesize(string name, float frequency, float duration)
        {
            const int rate = 22050;
            var samples = new float[Mathf.CeilToInt(duration * rate)];
            for (int index = 0; index < samples.Length; index++)
            {
                float time = (float)index / rate;
                float envelope = Mathf.Min(1, time * 150) * Mathf.Pow(1 - time / duration, 2);
                float glide = name == "jump" || name == "pickup" ? 1 + time * 2 : 1 - time * 0.7f;
                samples[index] = (Mathf.Sin(time * frequency * glide * Mathf.PI * 2) + 0.3f * Mathf.Sin(time * frequency * 2.01f * Mathf.PI * 2)) * envelope * 0.24f;
            }
            var clip = AudioClip.Create(name + " procedural", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
        public static void Play(string name)
        {
            if (instance != null && instance.clips.TryGetValue(name, out var clip)) instance.source.PlayOneShot(clip, 0.65f);
        }
        private void Update()
        {
            if (GameSession.Instance.Phase != RunPhase.Playing) return;
            ambientClock += Time.deltaTime;
            if (ambientClock < 1.2f) return;
            ambientClock = 0;
            float[] pitches = { 0.5f, 0.75f, 1, 0.67f, 0.5f, 0.89f, 0.75f, 0.67f };
            source.pitch = pitches[ambientNote++ % pitches.Length];
            source.PlayOneShot(clips["note"], 0.12f);
            source.pitch = 1;
        }
    }
}
