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
        public new BookOutcomeProperties_GainResearchDefinable Props
        {
            get
            {
                return (BookOutcomeProperties_GainResearchDefinable)this.props;
            }
        }
        public override void OnBookGenerated(Pawn author = null)
        {
            var researchList = Props.researchProjects;
            float qualityValue = this.GetBaseValue();
            foreach (var proj in ((BookOutcomeProperties_GainResearchDefinable)Props).researchProjects)
            {
                values[proj.researchProject] = qualityValue * proj.gainMultiplier;
                if (researchList.Count > 1)
                {
                    values[proj.researchProject] *= 0.75f;
                }
            }
            

        }
        //public override void OnReadingTick(Pawn reader, float factor)
        //{
        //    foreach (KeyValuePair<ResearchProjectDef, float> tuple in this.values)
        //    {
        //        ResearchProjectDef researchProjectDef;
        //        float num;
        //        tuple.Deconstruct(out researchProjectDef, out num);
        //        ResearchProjectDef researchProjectDef2 = researchProjectDef;
        //        float num2 = num;
        //        if (this.IsProjectVisible(researchProjectDef2) && !researchProjectDef2.IsFinished)
        //        {
        //            Find.ResearchManager.AddProgress(researchProjectDef2, num2 * factor, null);
        //        }
        //    }
        //}
        //public override float GetBaseValue()
        //{
        //    return QualityResearchExpTick.Evaluate((float)base.Quality);
        //   // return BookUtility.GetResearchExpForQuality(base.Quality);
        //}
        //private static readonly SimpleCurve QualityResearchExpTick = new SimpleCurve
        //{
        //    {
        //        new CurvePoint(0f, 0.008f),
        //        true
        //    },
        //    {
        //        new CurvePoint(1f, 0.012f),
        //        true
        //    },
        //    {
        //        new CurvePoint(2f, 0.016f),
        //        true
        //    },
        //    {
        //        new CurvePoint(3f, 0.02f),
        //        true
        //    },
        //    {
        //        new CurvePoint(4f, 0.024f),
        //        true
        //    },
        //    {
        //        new CurvePoint(5f, 0.028f),
        //        true
        //    },
        //    {
        //        new CurvePoint(6f, 0.032f),
        //        true
        //    }
        //};
    }
    
}
