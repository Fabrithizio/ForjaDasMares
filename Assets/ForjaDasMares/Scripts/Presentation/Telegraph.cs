using UnityEngine;
namespace ForjaDasMares
{
    public sealed class Telegraph : MonoBehaviour
    {
        Mesh circle, line, cone;
        MeshFilter filter;
        Renderer body;
        AudioSource audio;
        AudioClip cue;
        MaterialPropertyBlock properties;
        readonly Vector3[] vertices = new Vector3[34];
        readonly int[] triangles = new int[96];
        Object owner;
        Color color;
        float remaining;
        public bool HasSound => cue != null;
        public void Prepare(Mesh round, Mesh rectangle, AudioClip warning)
        {
            circle = round; line = rectangle; cue = warning; filter = GetComponent<MeshFilter>(); body = GetComponent<Renderer>(); audio = GetComponent<AudioSource>(); properties = new MaterialPropertyBlock();
            cone = new Mesh { name = "Original_Pooled_Warning_Cone" }; cone.MarkDynamic();
            for (int i = 0; i < 32; i++) { triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = i + 2; }
        }
        public bool OwnedBy(Object value) => gameObject.activeSelf && owner == value;
        public void Show(TelegraphShape shape, Vector3 point, Vector3 direction, float radius, float length, float arc, float duration, Color tint, Object source, bool sound)
        {
            owner = source; color = tint; remaining = Mathf.Max(0.05f, duration); point.y = 0.035f;
            direction.y = 0; if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;
            transform.SetPositionAndRotation(point, Quaternion.LookRotation(direction));
            if (shape == TelegraphShape.Cone)
            {
                vertices[0] = Vector3.zero;
                for (int i = 0; i <= 32; i++) { float a = (-arc * 0.5f + arc * i / 32) * Mathf.Deg2Rad; vertices[i + 1] = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)); }
                cone.vertices = vertices; cone.triangles = triangles; cone.RecalculateBounds(); filter.sharedMesh = cone;
                transform.localScale = new Vector3(radius, 1, radius);
            }
            else if (shape == TelegraphShape.Line)
            { filter.sharedMesh = line; transform.position += direction.normalized * length * 0.5f; transform.localScale = new Vector3(radius * 2, 0.025f, length); }
            else { filter.sharedMesh = circle; transform.localScale = new Vector3(radius * 2, 0.015f, radius * 2); }
            gameObject.SetActive(true); Paint(); if (sound) audio.PlayOneShot(cue);
        }
        void Update() { remaining -= Time.deltaTime; if (remaining <= 0 || owner == null) Hide(); else Paint(); }
        void Paint() { properties.SetColor("_BaseColor", Color.Lerp(color, Color.yellow, Mathf.PingPong(Time.time * 5, 0.4f))); body.SetPropertyBlock(properties); }
        public void Hide() => gameObject.SetActive(false);
        void OnDisable() { owner = null; if (audio != null) audio.Stop(); }
        void OnDestroy() { if (cone != null) Destroy(cone); }
    }
}
