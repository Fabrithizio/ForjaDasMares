using UnityEngine;

namespace ForjaDasMares.Networking
{
    // Presentation only: listens to the existing interaction, never grants rewards or damage.
    public sealed class WorldInteractionFeedback : MonoBehaviour
    {
        [SerializeField] WorldClickTarget target;
        [SerializeField] Transform movingPart;
        [SerializeField] Renderer[] highlights;
        [SerializeField] Color emission = new Color(.12f, .8f, .85f);
        [SerializeField, Min(.1f)] float duration = .85f;
        Vector3 restingScale;
        Quaternion restingRotation;
        MaterialPropertyBlock properties;
        MaterialPropertyBlock[] originalProperties;
        float remaining;
        public bool IsPlaying => remaining > 0;
        public int PlayCount { get; private set; }

        public void Configure(WorldClickTarget source, Transform moving, Renderer[] renderers)
        {
            if (Application.isPlaying && isActiveAndEnabled && target != null) target.Used.RemoveListener(Play);
            target = source; movingPart = moving; highlights = renderers;
            if (!Application.isPlaying) return;
            CaptureRestState();
            if (isActiveAndEnabled && target != null) target.Used.AddListener(Play);
        }

        void Awake()
        {
            properties = new MaterialPropertyBlock();
            CaptureRestState();
        }
        void CaptureRestState()
        {
            if (movingPart != null) { restingScale = movingPart.localScale; restingRotation = movingPart.localRotation; }
            if (highlights == null) return;
            originalProperties = new MaterialPropertyBlock[highlights.Length];
            for (int i = 0; i < highlights.Length; i++)
            {
                originalProperties[i] = new MaterialPropertyBlock();
                if (highlights[i] != null) highlights[i].GetPropertyBlock(originalProperties[i]);
            }
        }
        void OnEnable() { if (target != null) target.Used.AddListener(Play); }
        void OnDisable()
        {
            if (target != null) target.Used.RemoveListener(Play);
            remaining = 0;
            Restore();
        }
        public void Play() { remaining = duration; PlayCount++; enabled = true; }
        void Update()
        {
            if (remaining <= 0) return;
            remaining = Mathf.Max(0, remaining - Time.deltaTime);
            float pulse = Mathf.Sin((1 - remaining / duration) * Mathf.PI);
            if (movingPart != null)
            {
                movingPart.localScale = restingScale * (1 + pulse * .12f);
                movingPart.localRotation = restingRotation * Quaternion.Euler(0, pulse * 12, 0);
            }
            if (highlights != null) foreach (var renderer in highlights)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(properties);
                properties.SetColor("_EmissionColor", emission * pulse * .8f);
                renderer.SetPropertyBlock(properties);
            }
            if (remaining == 0) Restore();
        }
        void Restore()
        {
            if (movingPart != null) { movingPart.localScale = restingScale; movingPart.localRotation = restingRotation; }
            if (highlights == null || properties == null) return;
            for (int i = 0; i < highlights.Length; i++)
                if (highlights[i] != null) highlights[i].SetPropertyBlock(originalProperties != null && i < originalProperties.Length ? originalProperties[i] : null);
        }
    }
}
