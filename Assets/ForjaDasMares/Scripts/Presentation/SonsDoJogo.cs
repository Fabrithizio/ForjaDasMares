using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForjaDasMares
{
    // The four volumes of the settings, read in one place. Every sound of the game goes through one of them,
    // so nothing plays "loose" when the player turns a volume down to 0%.
    public static class Volumes
    {
        public const string Geral = "CampaignMasterVolume", Musica = "CampaignMusicVolume", Ambiente = "CampaignAmbienceVolume", Efeitos = "CampaignEffectsVolume";
        public static float De(string chave) => Mathf.Clamp01(PlayerPrefs.GetFloat(chave, chave == Geral ? .8f : chave == Musica ? .7f : chave == Ambiente ? .65f : .8f));
    }

    // A looping sound (ambience, music) that follows its volume while it plays: lowering it in the settings
    // takes effect at once, not only when the next area loads.
    public sealed class VolumeAoVivo : MonoBehaviour
    {
        AudioSource fonte; string chave; float ganho, relogio;
        public static void Ligar(AudioSource fonte, string chave, float ganho)
        {
            var v = fonte.GetComponent<VolumeAoVivo>(); if (v == null) v = fonte.gameObject.AddComponent<VolumeAoVivo>();
            v.fonte = fonte; v.chave = chave; v.ganho = ganho; v.relogio = 0; fonte.volume = ganho * Volumes.De(chave);
        }
        void Update()
        {
            if ((relogio -= Time.unscaledDeltaTime) > 0 || fonte == null) return;
            relogio = .2f; fonte.volume = ganho * Volumes.De(chave);
        }
    }

    // Sounds chosen by Fabricio (Jogos/Som), kept in Resources/Som. A name with variations ("Espada_Golpe_1", _2...)
    // plays one of them at random, so repeated blows don't sound the same.
    public static class SonsDoJogo
    {
        static readonly Dictionary<string, AudioClip[]> cache = new Dictionary<string, AudioClip[]>();
        // Told by the world (it knows the place and the mount): which floor the steps are on.
        public static System.Func<bool> Montado;
        public static bool NaCidade;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset() => cache.Clear();

        public static AudioClip Um(string nome)
        {
            if (!cache.TryGetValue(nome, out var clips))
            {
                var list = new List<AudioClip>();
                var single = Resources.Load<AudioClip>("Som/Efeitos/" + nome);
                if (single != null) list.Add(single);
                for (int i = 1; i <= 8; i++) { var c = Resources.Load<AudioClip>("Som/Efeitos/" + nome + "_" + i); if (c != null) list.Add(c); }
                cache[nome] = clips = list.ToArray();
            }
            return clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];
        }

        // Plays one of the variations (with a small change of pitch), or the old sound if there is none.
        public static void Tocar(string nome, float volume, AudioClip antigo = null, float pitch = 1)
        {
            var clip = Um(nome);
            CombatAudioPool.Play(clip != null ? clip : antigo, volume, pitch * (clip != null ? Random.Range(.94f, 1.06f) : 1));
        }
    }

    // The music of the game: one track at a time, with a soft crossfade when it changes (entering another place)
    // and when it reaches its end (so the loop has no hard cut). It survives the change of scene, so the same music
    // keeps playing while you walk between areas that share it. The volume follows the settings live.
    public sealed class TrilhaSonora : MonoBehaviour
    {
        const float Cruzar = 2.5f;
        static TrilhaSonora atual;
        AudioSource[] vozes; int ativa; float ganho = .35f;
        string MusicKey => Volumes.Musica;

        public static void Tocar(string nome, float ganho = .35f)
        {
            var clip = Resources.Load<AudioClip>("Som/Musicas/" + nome);
            if (clip == null) return;
            if (atual == null)
            {
                var go = new GameObject("TrilhaSonora"); DontDestroyOnLoad(go);
                atual = go.AddComponent<TrilhaSonora>();
            }
            atual.ganho = ganho;
            atual.Trocar(clip);
        }
        public static bool Tocando => atual != null;

        void Awake()
        {
            vozes = new AudioSource[2];
            for (int i = 0; i < 2; i++) { var s = gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; s.loop = false; s.spatialBlend = 0; s.volume = 0; s.priority = 0; vozes[i] = s; }
        }
        float Alvo => ganho * Volumes.De(MusicKey);

        void Trocar(AudioClip clip)
        {
            if (vozes[ativa].clip == clip && vozes[ativa].isPlaying) return; // same music: keeps going
            ativa = 1 - ativa;
            var nova = vozes[ativa]; nova.clip = clip; nova.time = 0; nova.volume = 0; nova.Play();
        }

        void Update()
        {
            var nova = vozes[ativa]; var velha = vozes[1 - ativa];
            float alvo = Alvo, passo = Time.unscaledDeltaTime / Cruzar;
            // Near the end: start it again on the other voice and cross the two (no hard cut at the loop).
            if (nova.clip != null && nova.isPlaying && nova.clip.length > Cruzar * 3 && nova.time > nova.clip.length - Cruzar)
            {
                ativa = 1 - ativa; velha = nova; nova = vozes[ativa];
                nova.clip = velha.clip; nova.time = 0; nova.volume = 0; nova.Play();
            }
            nova.volume = Mathf.MoveTowards(nova.volume, alvo, passo * Mathf.Max(alvo, .01f));
            if (velha.isPlaying)
            {
                velha.volume = Mathf.MoveTowards(velha.volume, 0, passo * Mathf.Max(alvo, .01f));
                if (velha.volume <= 0) velha.Stop();
            }
        }
    }
}
