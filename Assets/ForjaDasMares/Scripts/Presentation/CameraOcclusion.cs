using System.Collections.Generic;
using UnityEngine;

namespace ForjaDasMares
{
    // Local presentation only: never changes colliders, materials or Renderer.enabled.
    // Each game view owns one active ArenaCamera. Restore before disabling/replacing it.
    internal sealed class CameraOcclusion
    {
        struct Hidden { public Renderer renderer; public bool original; public float lastSeen; }
        readonly List<Hidden> hidden = new List<Hidden>(32);
        readonly RaycastHit[] hits = new RaycastHit[128];
        public void Update(Vector3 camera, Vector3 player, int mask, float radius, float delay, float now)
        {
            Trace(camera, player + Vector3.up * 0.45f, mask, radius, now);
            Trace(camera, player + Vector3.up * 1.5f, mask, radius, now);
            for (int i = hidden.Count - 1; i >= 0; i--)
            {
                var item = hidden[i];
                if (item.renderer != null && now - item.lastSeen <= delay) continue;
                if (item.renderer != null) item.renderer.forceRenderingOff = item.original;
                hidden.RemoveAt(i);
            }
        }
        void Trace(Vector3 camera, Vector3 focus, int mask, float radius, float now)
        {
            Vector3 ray = focus - camera; float length = ray.magnitude;
            if (length < 0.01f) return;
            int count = Physics.SphereCastNonAlloc(camera, radius, ray / length, hits, length, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider; hits[i] = default;
                var renderer = collider.GetComponent<Renderer>() ?? collider.GetComponentInParent<Renderer>();
                if (renderer == null) continue;
                int index = -1;
                for (int j = 0; j < hidden.Count; j++) if (hidden[j].renderer == renderer) { index = j; break; }
                if (index < 0) hidden.Add(new Hidden { renderer = renderer, original = renderer.forceRenderingOff, lastSeen = now });
                else { var item = hidden[index]; item.lastSeen = now; hidden[index] = item; }
                renderer.forceRenderingOff = true;
            }
        }
        public void Restore()
        {
            foreach (var item in hidden) if (item.renderer != null) item.renderer.forceRenderingOff = item.original;
            hidden.Clear();
        }
    }
}
