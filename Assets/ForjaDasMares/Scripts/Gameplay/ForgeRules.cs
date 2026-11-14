using UnityEngine;

namespace ForjaDasMares
{
    // Rules of the Forge (weapon strengthening, inspired by DDTank).
    // The numbers come from Resources/Forge/ForgeSettings (editable in the Inspector); the values below are only a fallback.
    // When there is a server, the roll must happen there; these rules stay the same.
    public static class ForgeRules
    {
        public const int MaxLevel = 12;
        public const int StoneLevels = 5;
        public const int MaxStonesPerTry = 4;

        static readonly float[,] FallbackTable =
        {
            { 25f, 40f, 70f, 100f, 100f }, // +1 to +3
            { 10f, 18f, 30f, 50f, 80f },   // +4 to +6
            { 0f, 7f, 13f, 22f, 35f },     // +7 to +9
            { 0f, 0f, 4f, 8f, 13f },       // +10 and +11
            { 0f, 0f, 0f, 2.5f, 5f },      // +12
        };
        static readonly int[] FallbackBands = { 3, 6, 9, 11, 12 };

        static ForgeSettings settings;
        static bool loaded;
        static ForgeSettings Settings
        {
            get
            {
                if (!loaded) { settings = Resources.Load<ForgeSettings>("Forge/ForgeSettings"); loaded = true; }
                return settings;
            }
        }

        public static float LuckBonus => Settings != null ? Mathf.Max(0, Settings.luckBonus) : 5f;
        public static int DropFromLevel => Settings != null ? Mathf.Clamp(Settings.dropFromLevel, 1, MaxLevel) : 7;
        public static int FusionCost => Settings != null ? Mathf.Clamp(Settings.fusionCost, 2, 99) : 4;
        static float DamagePerLevel => Settings != null ? Mathf.Max(0, Settings.damagePerLevel) : .05f;

        public static float StoneValue(int targetLevel, int stoneLevel)
        {
            if (targetLevel < 1 || targetLevel > MaxLevel || stoneLevel < 1 || stoneLevel > StoneLevels) return 0f;
            var data = Settings;
            if (data != null && data.bands != null && data.bands.Length > 0)
            {
                foreach (var band in data.bands)
                    if (band != null && targetLevel <= band.upToLevel)
                        return band.percentPerStone != null && band.percentPerStone.Length >= stoneLevel ? Mathf.Max(0, band.percentPerStone[stoneLevel - 1]) : 0f;
                return 0f;
            }
            for (int i = 0; i < FallbackBands.Length; i++)
                if (targetLevel <= FallbackBands[i]) return FallbackTable[i, stoneLevel - 1];
            return 0f;
        }

        public static float Chance(int currentLevel, int stoneLevel, int stoneCount, bool luck)
        {
            if (currentLevel >= MaxLevel || stoneCount < 1) return 0f;
            float value = StoneValue(currentLevel + 1, stoneLevel);
            if (value <= 0f) return 0f;
            return Mathf.Min(100f, value * Mathf.Clamp(stoneCount, 1, MaxStonesPerTry) + (luck ? LuckBonus : 0f));
        }

        public static bool DropsOnFailure(int currentLevel) => currentLevel >= DropFromLevel;

        // Visual version of a legendary: 1 from +0 to +7, 2 from +8 to +11, 3 (final) only at +12.
        public static int VisualVersion(int level) => level >= 12 ? 3 : level >= 8 ? 2 : 1;

        public static float DamageMultiplier(int level) => 1f + DamagePerLevel * Mathf.Clamp(level, 0, MaxLevel);
    }
}
