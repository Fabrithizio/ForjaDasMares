using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
namespace ForjaDasMares.Editor
{
    public static class ProgressionInstaller
    {
        public const string DungeonScene = "Assets/ForjaDasMares/Scenes/ProgressionDungeon.unity";
        public const string ArenaScene = "Assets/ForjaDasMares/Scenes/ProgressionArena.unity";
        public const string CatalogPath = "Assets/ForjaDasMares/Data/Progression/Catalog.asset";
        const string Root = "Assets/ForjaDasMares/Data/Progression/";
        [MenuItem("Forja das Mares/Install progression variants")]
        public static void Apply()
        {
            Directory.CreateDirectory(Root); AssetDatabase.Refresh(); var catalog = CreateCatalog(); catalog.Validate();
            string inputPath = "Assets/ForjaDasMares/Input/ProgressionControls.inputactions";
            if (!File.Exists(inputPath))
            {
                var input = ScriptableObject.CreateInstance<InputActionAsset>(); var map = input.AddActionMap("Menus");
                var action = map.AddAction("Inventory", InputActionType.Button); action.AddBinding("<Keyboard>/i"); action.AddBinding("<Gamepad>/start");
                action = map.AddAction("Close", InputActionType.Button); action.AddBinding("<Keyboard>/escape"); action.AddBinding("<Gamepad>/buttonEast");
                File.WriteAllText(inputPath, input.ToJson()); UnityEngine.Object.DestroyImmediate(input); AssetDatabase.ImportAsset(inputPath, ImportAssetOptions.ForceSynchronousImport);
            }
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            Install(DungeonInstaller.ScenePath, DungeonScene, catalog, controls); Install(ArenaBuilder.ScenePath, ArenaScene, catalog, controls);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(DungeonScene, true), new EditorBuildSettingsScene(ArenaScene, true), new EditorBuildSettingsScene(DungeonInstaller.ScenePath, true), new EditorBuildSettingsScene(ArenaBuilder.ScenePath, true) };
            PlayerSettings.bundleVersion = "0.5.0"; PlayerSettings.Android.bundleVersionCode = 5; AssetDatabase.SaveAssets(); Debug.Log("PROGRESSION_INSTALL_OK");
        }
        static T Asset<T>(string name, Action<T> configure) where T : ScriptableObject
        { string path = Root + name + ".asset"; var asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) return asset; asset = ScriptableObject.CreateInstance<T>(); configure(asset); AssetDatabase.CreateAsset(asset, path); return asset; }
        static AttackDefinition Attack(string id, int damage, float range, float windup, float recovery, float arc, float knock, bool projectile = false, float speed = 17, float splash = 0, ControlEffect control = ControlEffect.None, int count = 1, float spread = 0)
        { return Asset<AttackDefinition>(id, a => { a.damage = damage; a.range = range; a.windup = windup; a.recovery = recovery; a.arc = arc; a.knockback = knock; a.shape = projectile ? AttackShape.Projectile : AttackShape.Arc; a.projectileSpeed = speed; a.projectileRadius = .14f; a.splashRadius = splash; a.control = control; a.controlDuration = control == ControlEffect.Root ? .35f : 1.1f; a.slowMultiplier = .6f; a.projectileCount = count; a.projectileSpread = spread; a.color = new Color(.65f, .85f, 1); }); }
        static EquipmentDefinition Weapon(string id, string name, string description, WeaponCategory category, HeroAffinity affinity, AttackDefinition[] attacks, bool twoHanded, Vector3 scale, Color color, bool twin = false, params StatModifier[] bonuses)
        { return Asset<EquipmentDefinition>(id, w => { w.itemId = id; w.displayName = name; w.description = description; w.category = category; w.slot = category == WeaponCategory.Shield ? EquipmentSlot.OffHand : EquipmentSlot.MainHand; w.compatibleHeroes = affinity; w.attacks = attacks; w.twoHanded = twoHanded; w.allowsOffHand = !twoHanded && category != WeaponCategory.Shield; w.twinStrike = twin; w.bonuses = bonuses; w.visualScale = scale; w.color = color; w.frontalBlock = category == WeaponCategory.Shield ? .25f : 0; }); }
        static EquipmentDefinition Gear(string id, string name, string description, EquipmentSlot slot, params StatModifier[] bonuses)
        { return Asset<EquipmentDefinition>(id, g => { g.itemId = id; g.displayName = name; g.description = description; g.slot = slot; g.category = WeaponCategory.None; g.bonuses = bonuses; }); }
        static ProgressionCatalog CreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(CatalogPath); if (catalog != null) return catalog;
            var sword = new[] { Attack("sword-cut", 24, 2.7f, .1f, .32f, 110, 2), Attack("sword-overhand", 28, 2.8f, .14f, .4f, 80, 3) };
            var axe = new[] { Attack("axe-cleave", 33, 2.6f, .14f, .5f, 175, 3.5f) };
            var hammer = new[] { Attack("hammer-impact", 42, 2.2f, .3f, .7f, 360, 6) };
            var bow = new[] { Attack("bow-arrow", 20, 12, .08f, .36f, 1, 0, true, 23) };
            var daggers = new[] { Attack("dagger-left", 11, 1.8f, .055f, .28f, 75, .4f), Attack("dagger-right", 12, 1.8f, .06f, .26f, 100, .4f) };
            var staff = new[] { Attack("staff-frost", 19, 10, .12f, .58f, 1, 0, true, 12, 1.2f, ControlEffect.Slow) };
            var grimoire = new[] { Attack("grimoire-fan", 9, 8, .16f, .58f, 1, 0, true, 14, 0, ControlEffect.None, 3, 18) };
            var magic = new[] { Attack("magic-orb", 17, 9, .18f, .85f, 1, 0, true, 10, 1.5f, ControlEffect.Root) };
            var items = new[]
            {
                Weapon("cais-blade", "Lâmina do Cais", "Cortes alternados; aceita escudo. Guerreiro e Cavaleiro Rúnico.", WeaponCategory.Sword, HeroAffinity.Warrior | HeroAffinity.RuneKnight, sword, false, new Vector3(.15f,.15f,1.2f), Color.gray, false, new StatModifier(ItemStat.Damage,.04f)),
                Weapon("fenda-axe", "Machado das Fendas", "Arco largo, golpe lento, duas mãos. Guerreiro.", WeaponCategory.Axe, HeroAffinity.Warrior, axe, true, new Vector3(.6f,.15f,1), new Color(.8f,.4f,.2f), false, new StatModifier(ItemStat.Damage,.03f)),
                Weapon("basalto-hammer", "Martelo de Basalto", "Impacto circular próximo e forte empurrão. Duas mãos.", WeaponCategory.Hammer, HeroAffinity.Warrior | HeroAffinity.RuneKnight, hammer, true, new Vector3(.8f,.4f,.85f), Color.darkGray, false, new StatModifier(ItemStat.Defense,4)),
                Weapon("vento-bow", "Arco do Vento Oco", "Flecha rápida de longo alcance; duas mãos. Patrulheiro.", WeaponCategory.Bow, HeroAffinity.Ranger, bow, true, new Vector3(.12f,1.1f,.15f), Color.green, false, new StatModifier(ItemStat.CriticalChance,.03f)),
                Weapon("vidro-daggers", "Presas de Vidro", "Dois golpes intencionais por ação, curto alcance. Patrulheiro; outra adaga pode ocupar a secundária.", WeaponCategory.Daggers, HeroAffinity.Ranger, daggers, false, new Vector3(.12f,.12f,.65f), Color.cyan, true, new StatModifier(ItemStat.AttackSpeed,.04f)),
                Weapon("sal-staff", "Cajado do Sal Azulado", "Projétil lento com explosão pequena e lentidão. Arcanista.", WeaponCategory.Staff, HeroAffinity.Arcanist, staff, true, new Vector3(.12f,1.4f,.12f), Color.blue, false, new StatModifier(ItemStat.ElementalDamage,.06f)),
                Weapon("centelha-book", "Códice das Três Centelhas", "Três projéteis em leque: cada um encerra ao primeiro impacto. Arcanista.", WeaponCategory.Grimoire, HeroAffinity.Arcanist, grimoire, false, new Vector3(.4f,.5f,.16f), Color.magenta, false, new StatModifier(ItemStat.CooldownReduction,.05f)),
                Weapon("laje-shield", "Broquel de Laje", "Secundária de espada: bloqueia 25% do dano frontal, sem invulnerabilidade.", WeaponCategory.Shield, HeroAffinity.Warrior | HeroAffinity.RuneKnight, Array.Empty<AttackDefinition>(), false, new Vector3(.15f,1,.8f), Color.gray, false, new StatModifier(ItemStat.Defense,10)),
                Weapon("mare-orb", "Orbe da Maré Morta", "Projétil lento, explosão maior e raiz breve; menor cadência. Arcanista.", WeaponCategory.MagicWeapon, HeroAffinity.Arcanist, magic, false, Vector3.one * .45f, new Color(.5f,.3f,.95f), false, new StatModifier(ItemStat.ElementalDamage,.08f)),
                Gear("brasa-armor", "Manto da Brasa Fria", "Vida e defesa moderadas, sem cura ao equipar.", EquipmentSlot.Armor, new StatModifier(ItemStat.Health,20), new StatModifier(ItemStat.Defense,12)),
                Gear("estrada-charm", "Pingente da Estrada", "Movimento e crítico, sem bônus cumulativos por reequipar.", EquipmentSlot.Accessory, new StatModifier(ItemStat.MoveSpeed,.05f), new StatModifier(ItemStat.CriticalChance,.04f)),
                Gear("retorno-seal", "Selo de Retorno", "Cooldown menor e recuperação lenta limitada. Não revive.", EquipmentSlot.Artifact, new StatModifier(ItemStat.CooldownReduction,.08f), new StatModifier(ItemStat.Recovery,.2f))
            };
            return Asset<ProgressionCatalog>("Catalog", c =>
            {
                c.equipment = items; c.starterItems = Array.ConvertAll(items, i => i.itemId);
                c.heroes = new[] { new HeroEquipmentRule { heroId = "warrior", affinity = HeroAffinity.Warrior }, new HeroEquipmentRule { heroId = "ranger", affinity = HeroAffinity.Ranger, allowSecondaryWeapon = true }, new HeroEquipmentRule { heroId = "arcanist", affinity = HeroAffinity.Arcanist }, new HeroEquipmentRule { heroId = "rune-knight", affinity = HeroAffinity.RuneKnight } };
                int[] weights = { 55,30,10,4,1 }; float[] powers = { 1,1.08f,1.16f,1.25f,1.35f }; Color[] colors = { Color.white, new Color(.35f,1,.5f), new Color(.4f,.7f,1), new Color(.85f,.45f,1), new Color(1,.8f,.2f) };
                c.rarities = new RarityRule[5]; for (int i=0;i<5;i++) c.rarities[i] = new RarityRule { rarity=(ItemRarity)i, weight=weights[i], power=powers[i], affixCount=Math.Min(i,3), color=colors[i] };
                c.affixes = new[] { Affix(ItemStat.Health,5,18), Affix(ItemStat.Damage,.02f,.07f), Affix(ItemStat.Defense,3,10), Affix(ItemStat.MoveSpeed,.01f,.04f), Affix(ItemStat.CriticalChance,.02f,.05f), Affix(ItemStat.AttackSpeed,.02f,.06f), Affix(ItemStat.CooldownReduction,.02f,.06f), Affix(ItemStat.ElementalDamage,.02f,.07f), Affix(ItemStat.Recovery,.1f,.3f) };
                c.enemy = Reward("Inimigo",12,2,0,.18f); c.room = Reward("Sala",15,4,0,0); c.chest = Reward("Baú",10,10,1,0); c.boss = Reward("Chefe",150,30,2,0,ItemRarity.Rare); c.completion = Reward("Vitória",90,20,1,0,ItemRarity.Rare);
                c.legendaryEcho = Attack("legendary-echo",6,2.8f,0,.1f,360,0,false,17,0,ControlEffect.Slow);
            });
        }
        static AffixRule Affix(ItemStat stat,float minimum,float maximum) => new AffixRule { stat=stat,minimum=minimum,maximum=maximum };
        static RewardRule Reward(string id,int xp,int coins,int items,float chance,ItemRarity minimum=ItemRarity.Common) => new RewardRule { rewardId=id,experience=xp,coins=coins,guaranteedItems=items,itemChance=chance,minimumRarity=minimum };
        static void Install(string source,string target,ProgressionCatalog catalog,InputActionAsset controls)
        {
            if (File.Exists(target)) { Debug.Log("PROGRESSION_SCENE_EXISTS " + target); return; }
            var scene = EditorSceneManager.OpenScene(source); var heroes = UnityEngine.Object.FindFirstObjectByType<HeroRoster>(); var motor=heroes.GetComponent<PlayerMotor>(); var combat=heroes.GetComponent<HeroCombat>();
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ForjaDasMares/Materials/Hero_Steel.mat");
            var main=Visual(heroes.transform,"EquippedMain_Placeholder",new Vector3(.55f,1,.3f),PrimitiveType.Cube,mat); var secondary=Visual(heroes.transform,"EquippedSecondary_Placeholder",new Vector3(-.55f,1,.3f),PrimitiveType.Cube,mat);
            var equipment=heroes.gameObject.AddComponent<HeroEquipment>(); equipment.Configure(heroes,motor,combat,main,secondary);
            var root=new GameObject("ProgressionSystems"); var petRoot=new GameObject("SupportCompanion_Lume"); var pet=petRoot.AddComponent<SupportCompanion>(); pet.Configure(heroes.transform,heroes.Health,catalog,1<<9); Visual(petRoot.transform,"Lume_Placeholder",Vector3.zero,PrimitiveType.Sphere,mat).transform.localScale=Vector3.one*.35f;
            var dungeon=UnityEngine.Object.FindFirstObjectByType<DungeonSession>(); if(dungeon!=null) dungeon.ConfigureArenaScene("ProgressionArena");
            var bridge=root.AddComponent<ProgressionSceneBridge>(); bridge.Configure(catalog,heroes,equipment,UnityEngine.Object.FindFirstObjectByType<EncounterController>(),UnityEngine.Object.FindFirstObjectByType<ArenaSession>(),dungeon,pet,heroes.GetComponent<ArenaInput>());
            var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>(); canvas.gameObject.AddComponent<ProgressionHud>().Configure(bridge,canvas,heroes.GetComponent<ArenaInput>(),controls);
            if(dungeon==null)
            {
                var button=new GameObject("OpenProgressionDungeon",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button)); button.transform.SetParent(canvas.transform,false); var rect=(RectTransform)button.transform; rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1); rect.anchoredPosition=new Vector2(15,-215); rect.sizeDelta=new Vector2(190,40); button.GetComponent<UnityEngine.UI.Image>().color=new Color(.15f,.3f,.4f); UnityEditor.Events.UnityEventTools.AddPersistentListener(button.GetComponent<UnityEngine.UI.Button>().onClick,bridge.OpenDungeon);
                var label=new GameObject("Label",typeof(RectTransform),typeof(UnityEngine.UI.Text)); label.transform.SetParent(button.transform,false); var text=label.GetComponent<UnityEngine.UI.Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=18; text.text="MASMORRA"; text.alignment=TextAnchor.MiddleCenter; text.raycastTarget=false; var lr=(RectTransform)label.transform; lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one; lr.offsetMin=lr.offsetMax=Vector2.zero;
            }
            EditorSceneManager.SaveScene(scene,target);
        }
        static MeshRenderer Visual(Transform parent,string name,Vector3 position,PrimitiveType shape,Material material)
        { var go=GameObject.CreatePrimitive(shape); go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=position; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); var renderer=go.GetComponent<MeshRenderer>(); renderer.sharedMaterial=material; return renderer; }
        [MenuItem("Forja das Mares/Open progression dungeon")] public static void Open() { if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; EditorSceneManager.OpenScene(DungeonScene); }
    }
}
