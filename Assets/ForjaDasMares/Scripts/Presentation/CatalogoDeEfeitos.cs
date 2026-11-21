using UnityEngine;

namespace ForjaDasMares
{
    // Effects of the Cartoon FX pack by name, so the game can use them at runtime (filled by EfeitosInstaller).
    public sealed class CatalogoDeEfeitos : ScriptableObject
    {
        [System.Serializable] public struct Item { public string chave; public GameObject prefab; }
        public Item[] itens = new Item[0];
        public GameObject Find(string key)
        {
            foreach (var i in itens) if (i.chave == key) return i.prefab;
            return null;
        }
    }
}
