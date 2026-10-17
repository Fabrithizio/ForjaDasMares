using System;
namespace ForjaDasMares.Networking
{
    // Authority-side coordinator. Reuses durable ProfileService transactions; never accepts client reward amounts.
    public sealed class CoopRewardLedger
    {
        readonly ProgressionCatalog catalog;
        readonly ProfileService[] profiles = new ProfileService[4];
        readonly string[] runs = new string[4];
        readonly System.Collections.Generic.HashSet<string> completedEvents=new System.Collections.Generic.HashSet<string>();
        bool active;
        public bool Active => active;
        public string Error { get; private set; }
        public CoopRewardLedger(ProgressionCatalog data) { catalog = data ?? throw new ArgumentNullException(nameof(data)); catalog.Validate(); }
        public ProfileService Profile(int slot) => slot >= 0 && slot < profiles.Length ? profiles[slot] : null;
        public void Assign(int slot, ProfileService profile)
        {
            if (slot < 0 || slot >= profiles.Length) throw new ArgumentOutOfRangeException(nameof(slot));
            if (active && profiles[slot] != profile) throw new InvalidOperationException("Cannot replace a participant during an expedition.");
            profiles[slot] = profile;
        }
        public bool Begin(string[] heroIds, string location)
        {
            if (active || heroIds == null || heroIds.Length != profiles.Length) return false;
            Error = null;completedEvents.Clear();
            for (int i = 0; i < profiles.Length; i++)
            {
                if (profiles[i] == null) continue;
                if (catalog.Hero(heroIds[i]) == null) { Error = "Herói incompatível com o catálogo da autoridade."; return false; }
            }
            bool participating = false;
            for (int i = 0; i < profiles.Length; i++)
            {
                if (profiles[i] == null) continue;
                if (!profiles[i].BeginExpedition(heroIds[i], location))
                {
                    Error = profiles[i].Message;
                    // Earlier successful opens contain no rewards yet; close them durably.
                    for (int j = 0; j < i; j++) if (runs[j] != null) profiles[j].FinishExpedition(runs[j], false, "Preparação cancelada");
                    Array.Clear(runs, 0, runs.Length); return false;
                }
                runs[i] = profiles[i].Profile.expedition.runId; participating = true;
            }
            active = participating; return participating;
        }
        public bool Join(int slot,ProfileService profile,string heroId,string location)
        {
            if(!active||slot<0||slot>=4||profile==null||profiles[slot]!=null||runs[slot]!=null||catalog.Hero(heroId)==null)return false;
            if(!profile.BeginExpedition(heroId,location)){Error=profile.Message;return false;}
            profiles[slot]=profile;runs[slot]=profile.Profile.expedition.runId;return true;
        }
        public bool Grant(string eventId, RewardRule rule, int seed)
        {
            if (!active || string.IsNullOrWhiteSpace(eventId) || eventId.Length > 128) return false;
            if (!catalog.IsRewardRule(rule)) return false;
            if(completedEvents.Contains(eventId))return true;
            Error = null;
            for (int i = 0; i < profiles.Length; i++)
            {
                if (runs[i] == null) continue;
                var run = profiles[i].Profile.expedition;
                if (run == null || run.runId != runs[i]) { Error = "Expedição da autoridade não está disponível."; return false; }
                // Retrying after a disk failure must converge without duplicating already committed peers.
                if (run.events.Contains(eventId)) continue;
                int playerSeed = unchecked(seed * 397 ^ i * 7919);
                if (!profiles[i].Grant(runs[i], eventId, rule, playerSeed)) { Error = profiles[i].Message; return false; }
            }
            completedEvents.Add(eventId);return true;
        }
        public bool Finish(bool victory, string outcome)
        {
            if (!active) return false; Error = null; bool finished = true;
            for (int i = 0; i < profiles.Length; i++)
            {
                if (runs[i] == null) continue;
                var profile = profiles[i];
                if (profile.Profile.completedRuns.Contains(runs[i])) { runs[i] = null; continue; }
                if (profile.FinishExpedition(runs[i], victory, outcome)) runs[i] = null;
                else { Error = profile.Message; finished = false; }
            }
            if (finished) active = false;
            return finished;
        }
        public static string ParticipantPath(string directory, string identity)
        {
            if (!Guid.TryParseExact(identity, "N", out Guid id)) throw new ArgumentException("Invalid participant identity.", nameof(identity));
            return System.IO.Path.Combine(System.IO.Path.GetFullPath(directory), id.ToString("N"), "progress-v2.json");
        }
    }
}
