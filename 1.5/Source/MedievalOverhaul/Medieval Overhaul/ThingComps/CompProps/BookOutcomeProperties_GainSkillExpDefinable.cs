using RimWorld;
using System;
using System.Collections.Generic;

namespace MedievalOverhaul
{
    public class GainMultiplier
    {
        public SkillDef skillDef;
        public float gainMultiplier;
    }

    public class BookOutcomeProperties_GainSkillExpDefinable : BookOutcomeProperties_GainSkillExp
    {
        public List<GainMultiplier> skillGains;
        public int? maxSkillLevel;
    }

    public class BookOutcomeDoerGainSkillExpDefinable : BookOutcomeDoerGainSkillExp
    {

    }
}
