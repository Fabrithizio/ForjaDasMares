using System;
using System.IO;
using UnityEngine;

namespace ForjaDasMares
{
    // One character per account: the equipped weapon defines how the player fights.
    [Serializable]
    public sealed class CharacterRecord
    {
        public string id;
        public string name;
        public string createdUtc;
        // Visual base until a real appearance editor exists: 0 Brasa, 1 Vento, 2 Prisma, 3 Baluarte.
        public int look;
        // Starting character: 0 Expedicionário, 1 Expedicionária. Style (skins, accessories) comes later from play.
        public int gender;
    }

    [Serializable]
    public sealed class PlayerAccount
    {
        public int version = 1;
        public string accountId;
        public string provider = "local";
        public string createdUtc;
        public string lastLoginUtc;
        public CharacterRecord character = new CharacterRecord();
        public bool HasCharacter => character != null && !string.IsNullOrEmpty(character.id);
    }

    // Where the game writes saves. Today: files on this device.
    // Later: a cloud version (Unity Cloud Save) with these same methods, so the rest of the game stays the same.
    public interface ISaveStore
    {
        string Read(string key);
        void Write(string key, string json);
    }

    public sealed class LocalSaveStore : ISaveStore
    {
        readonly string root;
        public LocalSaveStore(string folder) { root = folder; }
        string PathOf(string key) => Path.Combine(root, key);
        public string Read(string key)
        {
            string path = PathOf(key);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        public void Write(string key, string json)
        {
            string path = PathOf(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, json);
            File.Copy(temporary, path, true);
            File.Delete(temporary);
        }
    }

    // How the player is identified. Today: an anonymous account created on this device, with no form.
    // Later: Unity Authentication (anonymous first, then Google Play Games / Apple / Steam linked to the same player).
    public interface IAccountProvider
    {
        string ProviderName { get; }
        string SignIn(string knownAccountId);
    }

    public sealed class LocalAccountProvider : IAccountProvider
    {
        public string ProviderName => "local";
        public string SignIn(string knownAccountId) =>
            Guid.TryParseExact(knownAccountId, "N", out _) ? knownAccountId : Guid.NewGuid().ToString("N");
    }

    // The only place that knows where the account and the character are saved.
    public static class AccountService
    {
        const string ActiveKey = "Forja.Account.Active";
        const string AccountFile = "conta.json";
        public const int MaxNameLength = 16;
        public static readonly string[] Looks = { "Brasa", "Vento", "Prisma", "Baluarte" };
        public static readonly string[] LookWeapons = { "Espada", "Arco", "Cajado", "Lâmina e escudo" };

        static PlayerAccount current;
        static ISaveStore store;
        static IAccountProvider provider = new LocalAccountProvider();

        static string Root => Path.Combine(Application.persistentDataPath, "Contas");
        static string Now => DateTime.UtcNow.ToString("o");

        // Reads the saved account without creating one.
        public static PlayerAccount Peek()
        {
            if (current != null) return current;
            string id = PlayerPrefs.GetString(ActiveKey, "");
            if (!Guid.TryParseExact(id, "N", out _)) return null;
            var candidateStore = new LocalSaveStore(Path.Combine(Root, id));
            string json = candidateStore.Read(AccountFile);
            if (string.IsNullOrEmpty(json)) return null;
            PlayerAccount loaded;
            try { loaded = JsonUtility.FromJson<PlayerAccount>(json); }
            catch (ArgumentException) { return null; }
            if (loaded == null || loaded.accountId != id) return null;
            current = loaded;
            store = candidateStore;
            return current;
        }

        // Enters the account, creating an anonymous one the first time the game opens.
        public static PlayerAccount SignIn()
        {
            if (Peek() == null)
            {
                string id = provider.SignIn(PlayerPrefs.GetString(ActiveKey, ""));
                current = new PlayerAccount { accountId = id, provider = provider.ProviderName, createdUtc = Now };
                store = new LocalSaveStore(Path.Combine(Root, id));
                PlayerPrefs.SetString(ActiveKey, id);
                PlayerPrefs.Save();
            }
            current.lastLoginUtc = Now;
            Save();
            return current;
        }

        // Accounts saved on this device (to switch while testing; later the login does this).
        public static PlayerAccount[] LocalAccounts()
        {
            var list = new System.Collections.Generic.List<PlayerAccount>();
            if (!Directory.Exists(Root)) return list.ToArray();
            foreach (var folder in Directory.GetDirectories(Root))
            {
                string file = Path.Combine(folder, AccountFile);
                if (!File.Exists(file)) continue;
                try { var account = JsonUtility.FromJson<PlayerAccount>(File.ReadAllText(file)); if (account != null && !string.IsNullOrEmpty(account.accountId)) list.Add(account); }
                catch (ArgumentException) { }
            }
            list.Sort((a, b) => string.CompareOrdinal(b.lastLoginUtc ?? "", a.lastLoginUtc ?? ""));
            return list.ToArray();
        }

        public static void UseAccount(string accountId)
        {
            current = null; store = null;
            PlayerPrefs.SetString(ActiveKey, accountId); PlayerPrefs.Save();
            SignIn();
        }

        public static void NewAccount() => UseAccount(Guid.NewGuid().ToString("N"));

        public static string ShortIdOf(string accountId) => string.IsNullOrEmpty(accountId) || accountId.Length < 8 ? accountId : accountId.Substring(0, 4).ToUpperInvariant() + "-" + accountId.Substring(4, 4).ToUpperInvariant();

        public static bool HasCharacter => Peek()?.HasCharacter == true;
        public static CharacterRecord Character => HasCharacter ? current.character : null;

        // Folder with everything that belongs to the character: inventory, gold, level and story.
        public static string CharacterFolder =>
            HasCharacter ? Path.Combine(Root, current.accountId, "personagem-" + current.character.id) : null;

        public static string ValidateName(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length < 2) return "Digite um nome com pelo menos duas letras.";
            if (name.Length > MaxNameLength) return "Use no máximo " + MaxNameLength + " caracteres.";
            foreach (char c in name)
                if (!char.IsLetterOrDigit(c) && c != ' ' && c != '-' && c != '\'')
                    return "Use letras, números, espaço, hífen ou apóstrofo.";
            return null;
        }

        public static readonly string[] Genders = { "Expedicionário", "Expedicionária" };
        public static CharacterRecord CreateCharacter(string name, int look, int gender)
        {
            var created = CreateCharacter(name, look);
            if (created == null) return null;
            created.gender = Mathf.Clamp(gender, 0, Genders.Length - 1);
            Save();
            return created;
        }

        public static CharacterRecord CreateCharacter(string name, int look)
        {
            if (ValidateName(name) != null) return null;
            SignIn();
            current.character = new CharacterRecord
            {
                id = Guid.NewGuid().ToString("N"),
                name = name.Trim(),
                look = Mathf.Clamp(look, 0, Looks.Length - 1),
                createdUtc = Now
            };
            Save();
            return current.character;
        }

        public static void DeleteCharacter()
        {
            if (!HasCharacter) return;
            string folder = CharacterFolder;
            current.character = new CharacterRecord();
            Save();
            try { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
            catch (IOException exception) { Debug.LogWarning("Pasta do personagem não foi apagada: " + exception.Message); }
            catch (UnauthorizedAccessException exception) { Debug.LogWarning("Pasta do personagem não foi apagada: " + exception.Message); }
        }

        public static string ShortId => current != null && current.accountId != null && current.accountId.Length >= 8
            ? current.accountId.Substring(0, 4).ToUpperInvariant() + "-" + current.accountId.Substring(4, 4).ToUpperInvariant()
            : "";

        static void Save()
        {
            if (current == null || store == null) return;
            store.Write(AccountFile, JsonUtility.ToJson(current, true));
        }
    }
}
