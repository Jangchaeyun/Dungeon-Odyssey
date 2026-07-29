using DungeonOdyssey.Core;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    /// <summary>
    /// 프로시저럴 SFX + 씬 앰비언스.
    /// </summary>
    public static class CombatAudio
    {
        private static AudioSource _source;
        private static AudioSource _ambience;
        private static string _ambKey;

        private static float SfxScale =>
            GameManager.Instance?.CurrentSave != null
                ? Mathf.Clamp01(GameManager.Instance.CurrentSave.sfxVolume)
                : 1f;

        private static void OneShot(AudioClip clip, float volume) =>
            Source.PlayOneShot(clip, volume * SfxScale);

        private static AudioSource Source
        {
            get
            {
                if (_source != null)
                {
                    return _source;
                }

                var go = new GameObject("CombatAudio");
                Object.DontDestroyOnLoad(go);
                _source = go.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.spatialBlend = 0f;
                return _source;
            }
        }

        private static AudioSource Ambience
        {
            get
            {
                if (_ambience != null)
                {
                    return _ambience;
                }

                var go = Source.gameObject;
                _ambience = go.AddComponent<AudioSource>();
                _ambience.playOnAwake = false;
                _ambience.loop = true;
                _ambience.spatialBlend = 0f;
                _ambience.volume = 0.22f;
                return _ambience;
            }
        }

        public static void PlayTownAmbience() => SetAmbience("town", MakeDrone(8f, 55f, 90f, 0.12f), 0.18f);

        public static void PlayDungeonAmbience() => SetAmbience("dungeon", MakeDrone(10f, 40f, 70f, 0.16f), 0.26f);

        public static void StopAmbience()
        {
            if (_ambience != null)
            {
                _ambience.Stop();
            }

            _ambKey = null;
        }

        private static void SetAmbience(string key, AudioClip clip, float volume)
        {
            if (_ambKey == key && Ambience.isPlaying)
            {
                return;
            }

            _ambKey = key;
            Ambience.clip = clip;
            Ambience.volume = volume * AudioListener.volume;
            Ambience.Play();
        }

        public static void Swing() => OneShot(MakeWhoosh(0.12f, 420f, 180f), 0.45f);
        public static void HitFlesh()
        {
            OneShot(MakeThud(0.08f, 90f, 0.7f), 0.7f);
            OneShot(MakeNoiseBurst(0.05f, 0.35f), 0.35f);
        }

        public static void HitCrit()
        {
            OneShot(MakeThud(0.1f, 70f, 0.9f), 0.85f);
            OneShot(MakeWhoosh(0.08f, 800f, 400f), 0.4f);
        }

        public static void PlayerHurt() => OneShot(MakeThud(0.1f, 110f, 0.55f), 0.6f);

        public static void MonsterBite()
        {
            OneShot(MakeNoiseBurst(0.07f, 0.5f), 0.55f);
            OneShot(MakeThud(0.06f, 140f, 0.5f), 0.5f);
        }

        public static void Whiff() => OneShot(MakeWhoosh(0.1f, 300f, 120f), 0.25f);

        public static void Kill()
        {
            OneShot(MakeThud(0.14f, 55f, 1f), 0.8f);
            OneShot(MakeWhoosh(0.15f, 200f, 60f), 0.35f);
        }

        public static void LevelUp()
        {
            OneShot(MakeWhoosh(0.18f, 520f, 980f), 0.5f);
            OneShot(MakeThud(0.1f, 220f, 0.45f), 0.4f);
        }

        public static void Coin() => OneShot(MakeWhoosh(0.07f, 900f, 1400f), 0.35f);

        public static void Heal()
        {
            OneShot(MakeWhoosh(0.14f, 360f, 720f), 0.4f);
            OneShot(MakeThud(0.08f, 180f, 0.35f), 0.3f);
        }

        public static void Portal()
        {
            OneShot(MakeWhoosh(0.22f, 180f, 640f), 0.55f);
            OneShot(MakeNoiseBurst(0.12f, 0.25f), 0.3f);
        }

        public static void Defeat()
        {
            OneShot(MakeThud(0.2f, 48f, 1f), 0.85f);
            OneShot(MakeWhoosh(0.2f, 160f, 40f), 0.4f);
        }

        public static void UiClick() => OneShot(MakeThud(0.04f, 320f, 0.4f), 0.35f);

        public static void Locked()
        {
            OneShot(MakeThud(0.08f, 70f, 0.6f), 0.45f);
            OneShot(MakeNoiseBurst(0.04f, 0.25f), 0.25f);
        }

        public static void DoorUnlock()
        {
            OneShot(MakeThud(0.07f, 160f, 0.5f), 0.4f);
            OneShot(MakeWhoosh(0.1f, 240f, 480f), 0.3f);
        }

        public static void DoorOpen()
        {
            OneShot(MakeThud(0.06f, 110f, 0.45f), 0.38f);
            OneShot(MakeWhoosh(0.14f, 180f, 420f), 0.32f);
            OneShot(MakeNoiseBurst(0.03f, 0.2f), 0.18f);
        }

        public static void RoomEnter() => OneShot(MakeWhoosh(0.16f, 120f, 280f), 0.28f);

        public static void ShopOpen()
        {
            OneShot(MakeThud(0.05f, 280f, 0.35f), 0.3f);
            OneShot(MakeWhoosh(0.08f, 500f, 700f), 0.25f);
        }

        public static void Save()
        {
            OneShot(MakeWhoosh(0.1f, 600f, 900f), 0.3f);
            OneShot(MakeThud(0.05f, 400f, 0.3f), 0.25f);
        }

        private static AudioClip MakeWhoosh(float dur, float startHz, float endHz)
        {
            var rate = 22050;
            var n = Mathf.CeilToInt(dur * rate);
            var clip = AudioClip.Create("whoosh", n, 1, rate, false);
            var data = new float[n];
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)n;
                var hz = Mathf.Lerp(startHz, endHz, t);
                var env = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.3f);
                data[i] = Mathf.Sin(2f * Mathf.PI * hz * (i / (float)rate)) * env * 0.55f;
                data[i] += (Random.value * 2f - 1f) * 0.08f * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeThud(float dur, float hz, float punch)
        {
            var rate = 22050;
            var n = Mathf.CeilToInt(dur * rate);
            var clip = AudioClip.Create("thud", n, 1, rate, false);
            var data = new float[n];
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)n;
                var env = Mathf.Exp(-t * 18f) * punch;
                data[i] = Mathf.Sin(2f * Mathf.PI * hz * (i / (float)rate)) * env;
                data[i] += Mathf.Sin(2f * Mathf.PI * (hz * 0.5f) * (i / (float)rate)) * env * 0.5f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeNoiseBurst(float dur, float volume)
        {
            var rate = 22050;
            var n = Mathf.CeilToInt(dur * rate);
            var clip = AudioClip.Create("noise", n, 1, rate, false);
            var data = new float[n];
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)n;
                var env = Mathf.Exp(-t * 22f);
                data[i] = (Random.value * 2f - 1f) * env * volume;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeDrone(float dur, float lowHz, float highHz, float volume)
        {
            var rate = 22050;
            var n = Mathf.CeilToInt(dur * rate);
            var clip = AudioClip.Create("drone", n, 1, rate, false);
            var data = new float[n];
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)rate;
                var env = 0.65f + 0.35f * Mathf.Sin(t * 0.7f);
                var a = Mathf.Sin(2f * Mathf.PI * lowHz * t) * 0.55f;
                var b = Mathf.Sin(2f * Mathf.PI * highHz * t) * 0.25f;
                var c = (Mathf.PerlinNoise(t * 0.4f, 0.2f) * 2f - 1f) * 0.08f;
                data[i] = (a + b + c) * env * volume;
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
