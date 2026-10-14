using UnityEngine;
namespace ForjaDasMares
{
    public enum TelegraphShape { Circle, Cone, Line }
    public sealed class TelegraphPool : MonoBehaviour
    {
        [SerializeField] Mesh circleMesh, lineMesh;
        [SerializeField] Material material;
        [SerializeField, Min(4)] int capacity = 24;
        Telegraph[] items;
        public Telegraph[] Items => items;
        AudioClip warning;
        public int ActiveCount { get { int count = 0; if (items != null) foreach (var item in items) if (item.gameObject.activeSelf) count++; return count; } }
        public void Configure(Mesh circle, Mesh line, Material shared) { circleMesh = circle; lineMesh = line; material = shared; }
        void Awake()
        {
            if (circleMesh == null || lineMesh == null || material == null) { Debug.LogError("Telegraph pool requires configured geometry/material.", this); enabled = false; return; }
            // Original synthesized cue: three short tones with smooth attack/release.
            const int rate = 22050; float[] samples = new float[rate / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate, pulse = t % 0.16f;
                float envelope = pulse < 0.1f ? Mathf.Sin(pulse / 0.1f * Mathf.PI) : 0;
                samples[i] = Mathf.Sin(t * Mathf.PI * 2 * (660 + Mathf.Floor(t / 0.16f) * 110)) * envelope * 0.3f;
            }
            warning = AudioClip.Create("Original_PvE_Warning", samples.Length, 1, rate, false); warning.SetData(samples, 0);
            items = new Telegraph[capacity];
            for (int i = 0; i < capacity; i++)
            {
                var go = new GameObject("Warning_" + i, typeof(MeshFilter), typeof(MeshRenderer), typeof(AudioSource)); go.transform.SetParent(transform);
                go.GetComponent<MeshRenderer>().sharedMaterial = material;
                AudioSource audio = go.GetComponent<AudioSource>(); audio.playOnAwake = false; audio.spatialBlend = 0; audio.volume = 0.18f;
                go.SetActive(false); items[i] = go.AddComponent<Telegraph>(); items[i].Prepare(circleMesh, lineMesh, warning);
            }
        }
        public Telegraph Acquire(TelegraphShape shape, Vector3 position, Vector3 direction, float radius, float length, float arc, float duration, Color color, Object owner, bool sound = true)
        {
            if (items == null) return null;
            foreach (var item in items) if (!item.gameObject.activeSelf) { item.Show(shape, position, direction, radius, length, arc, duration, color, owner, sound); return item; }
            return null;
        }
        public void ReleaseOwner(Object owner) { if (items != null) foreach (var item in items) if (item.OwnedBy(owner)) item.Hide(); }
        public void Clear() { if (items != null) foreach (var item in items) item.Hide(); }
        void OnDestroy() { if (warning != null) Destroy(warning); }
    }
}
