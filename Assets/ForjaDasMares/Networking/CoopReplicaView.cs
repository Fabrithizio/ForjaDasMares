using System.Collections.Generic;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    public sealed class CoopReplicaView : MonoBehaviour
    {
        EnemyPool enemies; CombatEffectPool effects; TelegraphPool warnings;
        Vector3[] positions; Quaternion[] rotations; bool[] active;
        readonly List<EffectFrame> cached = new List<EffectFrame>(96);
        readonly List<GameObject> proxies = new List<GameObject>(96);
        readonly List<Telegraph> marks = new List<Telegraph>(96);
        readonly HashSet<int> seen = new HashSet<int>();
        MaterialPropertyBlock color;
        public void Configure(EnemyPool registry, CombatEffectPool shots, TelegraphPool signs)
        {
            enemies = registry; effects = shots; warnings = signs;
            positions = new Vector3[registry.Capacity]; rotations = new Quaternion[registry.Capacity]; active = new bool[registry.Capacity];
            color = new MaterialPropertyBlock();
            int total = shots.Projectiles.Length + shots.Zones.Length + signs.Items.Length;
            for (int i = 0; i < total; i++) { cached.Add(new EffectFrame { slot = i }); proxies.Add(null); marks.Add(null); }
        }
        public void CaptureEffects(List<EffectFrame> output)
        {
            int slot = 0;
            foreach (var shot in effects.Projectiles)
            {
                if (shot.gameObject.activeSelf) { var value = cached[slot]; value.kind = 0; Transform(value, shot.transform); value.color = shot.Tint; output.Add(value); }
                slot++;
            }
            foreach (var zone in effects.Zones)
            {
                if (zone.gameObject.activeSelf) { var value = cached[slot]; value.kind = 1; Transform(value, zone.transform); value.color = zone.Tint; output.Add(value); }
                slot++;
            }
            foreach (var warning in warnings.Items)
            {
                if (warning.gameObject.activeSelf)
                {
                    var value = cached[slot]; value.kind = 2; value.shape = (int)warning.Shape; value.position = warning.Point; value.direction = warning.Direction;
                    value.radius = warning.Radius; value.length = warning.Length; value.arc = warning.Arc; value.remaining = warning.Remaining; value.color = warning.Tint; output.Add(value);
                }
                slot++;
            }
        }
        static void Transform(EffectFrame frame, Transform root) { frame.position = root.position; frame.direction = root.forward; frame.scale = root.localScale; }
        public void Apply(WorldFrame frame)
        {
            System.Array.Clear(active, 0, active.Length);
            foreach (var value in frame.enemies)
            {
                if (value.slot < 0 || value.slot >= enemies.Capacity) continue;
                var actor = enemies.Actors[value.slot]; bool fresh = !actor.gameObject.activeSelf;
                actor.SetReplica(enemies.DefinitionAt(value.slot), value.hp, value.maximum, (EnemyState)value.state,value.spawnWarning,value.opening);
                (actor.GetComponent<CombatAilments>()??actor.gameObject.AddComponent<CombatAilments>()).Replica(value.ailments);
                active[value.slot] = true; positions[value.slot] = value.position; rotations[value.slot] = Quaternion.Euler(0, value.yaw, 0);
                if (fresh || (actor.transform.position - value.position).sqrMagnitude > 16) actor.transform.SetPositionAndRotation(value.position, rotations[value.slot]);
            }
            for (int i = 0; i < active.Length; i++) if (!active[i] && enemies.Actors[i].gameObject.activeSelf) enemies.Actors[i].gameObject.SetActive(false);
            seen.Clear();
            foreach (var value in frame.effects)
            {
                if (value.slot < 0 || value.slot >= proxies.Count) continue; seen.Add(value.slot);
                if (value.kind == 2)
                {
                    bool fresh = marks[value.slot] == null || !marks[value.slot].gameObject.activeSelf;
                    if (fresh) marks[value.slot] = warnings.Acquire((TelegraphShape)value.shape, value.position, value.direction, value.radius, value.length, value.arc, Mathf.Max(.05f, value.remaining), value.color, this, true);
                    else marks[value.slot].Show((TelegraphShape)value.shape, value.position, value.direction, value.radius, value.length, value.arc, Mathf.Max(.05f, value.remaining), value.color, this, false);
                    continue;
                }
                if (proxies[value.slot] == null)
                {
                    var go = GameObject.CreatePrimitive(value.kind == 0 ? PrimitiveType.Sphere : PrimitiveType.Cylinder); go.name = "NetworkVisualEffect_" + value.slot;
                    Destroy(go.GetComponent<Collider>()); go.transform.SetParent(transform); go.GetComponent<Renderer>().sharedMaterial = effects.SharedMaterial; proxies[value.slot] = go;
                }
                var proxy = proxies[value.slot];bool restart=!proxy.activeSelf||(proxy.transform.position-value.position).sqrMagnitude>4; proxy.SetActive(true);
                if(value.kind==0&&effects.ProjectileTrailMaterial!=null){var trail=proxy.GetComponent<PooledProjectileTrail>()??proxy.AddComponent<PooledProjectileTrail>();trail.Configure(effects.ProjectileTrailMaterial,value.color,value.scale.x*.5f,restart);} proxy.transform.SetPositionAndRotation(value.position, Quaternion.LookRotation(value.direction)); proxy.transform.localScale = value.scale;
                color.SetColor("_BaseColor", value.color); proxy.GetComponent<Renderer>().SetPropertyBlock(color);
                if(value.kind==1)(proxy.GetComponent<MagicZoneMist>()??proxy.AddComponent<MagicZoneMist>()).Configure(value.scale.x*.5f,value.color);
            }
            for (int i = 0; i < proxies.Count; i++) if (!seen.Contains(i)) { if (proxies[i] != null) proxies[i].SetActive(false); if (marks[i] != null) marks[i].Hide(); }
        }
        void LateUpdate()
        {
            float blend = 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime);
            for (int i = 0; i < active.Length; i++) if (active[i])
            { var root = enemies.Actors[i].transform; root.SetPositionAndRotation(Vector3.Lerp(root.position, positions[i], blend), Quaternion.Slerp(root.rotation, rotations[i], blend)); }
        }
        public void Clear()
        { foreach (var proxy in proxies) if (proxy != null) proxy.SetActive(false); foreach (var mark in marks) if (mark != null) mark.Hide(); System.Array.Clear(active, 0, active.Length); }
    }
}
