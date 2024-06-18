using RimWorld;
using System.Collections.Generic;

namespace MedievalOverhaul
{
    public class GainMultiplier
    {
        public SkillDef skillDef;
        public float gainMultiplier;
    }

    public class BookOutcomeProperties_GainSkillExpDefinable : BookOutcomeDoerGainSkillExp
    {
        public List<GainMultiplier> skillGains;
        public int? maxSkillLevel;
    }
}
