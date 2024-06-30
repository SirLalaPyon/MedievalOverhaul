using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class RequiredSchematic : DefModExtension
    {
        public ThingDef schematicDef;
    }

    public class ResearchGainMultiplier
    {
        public ResearchProjectDef researchProject;
        public float gainMultiplier;
    }

    public class BookOutcomeProperties_GainResearchDefinable : BookOutcomeProperties_GainResearch
    {
        public List<ResearchGainMultiplier> researchProjects;
        public override Type DoerClass => typeof(ReadingOutcomeDoerGainResearchDefinable);
    }

    public class ReadingOutcomeDoerGainResearchDefinable : ReadingOutcomeDoerGainResearch
    {
        public override void OnBookGenerated(Pawn author = null)
        {
            foreach (var proj in ((BookOutcomeProperties_GainResearchDefinable)Props).researchProjects)
            {
                values[proj.researchProject] = proj.gainMultiplier;
            }
        }
    }
}
