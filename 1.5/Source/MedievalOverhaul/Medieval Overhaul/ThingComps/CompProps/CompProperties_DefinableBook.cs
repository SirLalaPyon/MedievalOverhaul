using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class GainMultiplier
    {
        public SkillDef skillDef;
        public float gainMultiplier;
    }
    public class CompProperties_DefinableBook : CompProperties_Book
    {
        public IntRange? qualityRange;
        public List<SkillDef> trainableSkills;
        public List<GainMultiplier> skillGainMultipliers;
        public int? maxSkillLevel;
    }
}
