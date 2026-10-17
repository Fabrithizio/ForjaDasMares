using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ForjaDasMares
{
    // Local presentation only. Colliders, original materials and visibility flags stay unchanged.
    internal sealed class CameraOcclusion
    {
        sealed class Piece
        {
            public Renderer renderer;
            public Material[] original, translucent;
            public float lastSeen;
            public bool applied;
        }
        readonly List<Piece> pieces = new List<Piece>(32);
        readonly Dictionary<Renderer, Piece> lookup = new Dictionary<Renderer, Piece>();
        readonly Dictionary<Collider,Renderer[]> groups=new Dictionary<Collider,Renderer[]>();
        readonly Dictionary<Material, Material> materials = new Dictionary<Material, Material>();
        readonly RaycastHit[] hits = new RaycastHit[128];
        Material template;
        float opacity = -1f;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public void Update(Vector3 camera, Vector3 player, int mask, float radius, float delay, float now, float alpha)
        {
            alpha = Mathf.Clamp(alpha, .15f, .75f);
            if (!Mathf.Approximately(alpha, opacity))
            {
                opacity = alpha;
                foreach (var pair in materials) SetOpacity(pair.Key, pair.Value);
            }
            Trace(camera, player + Vector3.up * .45f, mask, radius, now);
            Trace(camera, player + Vector3.up * 1.5f, mask, radius, now);
            for (int i = pieces.Count - 1; i >= 0; i--)
            {
                var piece = pieces[i];
                if (piece.renderer == null) { lookup.Remove(piece.renderer); pieces.RemoveAt(i); continue; }
                if (piece.applied && now - piece.lastSeen > delay)
                { piece.renderer.sharedMaterials = piece.original; piece.applied = false; }
            }
        }
        void Trace(Vector3 camera, Vector3 focus, int mask, float radius, float now)
        {
            Vector3 ray = focus - camera; float length = ray.magnitude;
            if (length < .01f) return;
            int count = Physics.SphereCastNonAlloc(camera, radius, ray / length, hits, length, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider; hits[i] = default;
                if(!groups.TryGetValue(collider,out var renderers))
                {renderers=collider.GetComponentsInChildren<Renderer>();if(renderers.Length==0){var parent=collider.GetComponentInParent<Renderer>();renderers=parent!=null?new[]{parent}:System.Array.Empty<Renderer>();}groups.Add(collider,renderers);}
                foreach(var renderer in renderers)Fade(renderer,now);
            }
        }
        void Fade(Renderer renderer,float now)
        {
                if (renderer == null || renderer.forceRenderingOff || !renderer.enabled) return;
                if (!lookup.TryGetValue(renderer, out var piece))
                {
                    var original = renderer.sharedMaterials;
                    var faded = new Material[original.Length];
                    for (int j = 0; j < original.Length; j++) faded[j] = Transparent(original[j]);
                    piece = new Piece { renderer = renderer, original = original, translucent = faded };
                    lookup.Add(renderer, piece); pieces.Add(piece);
                }
                piece.lastSeen = now;
                if (!piece.applied) { renderer.sharedMaterials = piece.translucent; piece.applied = true; }
        }
        Material Transparent(Material source)
        {
            if (source == null) return null;
            if (materials.TryGetValue(source, out var cached)) return cached;
            if (template == null) template = Resources.Load<Material>("CameraOcclusionTransparent");
            if (template == null) { Debug.LogError("Missing Resources/CameraOcclusionTransparent material."); return source; }
            // The referenced URP/Lit template retains transparent shader variants in builds.
            var result = new Material(template) { name = source.name + "_CameraTranslucent", hideFlags = HideFlags.HideAndDontSave };
            if (source.shader == template.shader) result.CopyPropertiesFromMaterial(source);
            result.shaderKeywords = template.shaderKeywords;
            result.SetFloat("_Surface", 1); result.SetFloat("_Blend", 0);
            result.SetFloat("_AlphaClip", 0); result.SetFloat("_BlendModePreserveSpecular", 0);
            result.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            result.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            result.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            result.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            result.SetFloat("_ZWrite", 0); result.SetOverrideTag("RenderType", "Transparent");
            result.renderQueue = (int)RenderQueue.Transparent;
            result.SetShaderPassEnabled("ShadowCaster", false); result.SetShaderPassEnabled("DepthOnly", false);
            result.SetShaderPassEnabled("DepthNormals", false); result.SetShaderPassEnabled("DepthNormalsOnly", false);
            SetOpacity(source, result); materials.Add(source, result); return result;
        }
        void SetOpacity(Material source, Material result)
        {
            Color color = source.HasProperty(BaseColor) ? source.GetColor(BaseColor) : source.color;
            color.a = opacity; result.SetColor(BaseColor, color);
        }
        public void Restore()
        {
            foreach (var piece in pieces) if (piece.renderer != null && piece.applied) piece.renderer.sharedMaterials = piece.original;
            pieces.Clear(); lookup.Clear();groups.Clear();
            foreach (var pair in materials) if (pair.Value != null) Object.Destroy(pair.Value);
            materials.Clear(); opacity = -1f;
        }
    }
}
