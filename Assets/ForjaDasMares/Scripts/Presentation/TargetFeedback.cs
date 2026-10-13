using UnityEngine;

namespace ForjaDasMares
{
    public sealed class TargetFeedback : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] Renderer body;
        [SerializeField, Min(0.01f)] float flashDuration = 0.15f;
        [SerializeField] Color flashColor = Color.white;
        MaterialPropertyBlock properties;
        Color originalColor;
        float remaining;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public void Configure(Health targetHealth, Renderer targetBody) { health = targetHealth; body = targetBody; }
        void Awake() { properties = new MaterialPropertyBlock(); originalColor = body.sharedMaterial.GetColor(BaseColor); }
        void OnEnable() { health.Damaged += Flash; health.Changed += Refresh; }
        void OnDisable() { health.Damaged -= Flash; health.Changed -= Refresh; }
        void Flash() => remaining = flashDuration;
        void Update() { if (remaining > 0f) { remaining = Mathf.Max(0f, remaining - Time.deltaTime); Refresh(); } }
        void Refresh()
        {
            Color color = remaining > 0f ? flashColor : health.Current > 0 ? originalColor : originalColor * 0.3f;
            if (remaining <= 0f && health.IsAlive)
            {
                if (health.ControlMultiplier == 0f) color = Color.cyan;
                else if (health.ControlMultiplier < 1f) color = Color.Lerp(originalColor, Color.cyan, 0.6f);
                else if (health.ProtectionRemaining > 0) color = Color.Lerp(originalColor, Color.yellow, 0.5f);
            }
            body.GetPropertyBlock(properties); properties.SetColor(BaseColor, color); body.SetPropertyBlock(properties);
        }
    }
}
