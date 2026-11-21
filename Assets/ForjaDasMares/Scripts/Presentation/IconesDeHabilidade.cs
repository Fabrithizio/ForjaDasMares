using System.Collections.Generic;
using UnityEngine;

namespace ForjaDasMares
{
    // Picture of each ability (Resources/Icones/Habilidades/<id>.png), shown on the combat bar and in the backpack.
    // Drawings from game-icons.net (CC BY 3.0, by Lorc, Delapouite and sbed), coloured per element.
    public static class IconesDeHabilidade
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        public static Sprite De(AbilityDefinition a)
        {
            if (a == null || string.IsNullOrEmpty(a.scriptId)) return null;
            if (cache.TryGetValue(a.scriptId, out var s)) return s;
            var t = Resources.Load<Texture2D>("Icones/Habilidades/" + a.scriptId);
            s = t != null ? Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), 100) : null;
            cache[a.scriptId] = s;
            return s;
        }
    }
}
