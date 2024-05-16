using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class HediffComp_LindwurmAcid : HediffComp
    {
        public HediffCompProperties_LindwurmAcid Props
        {
            get
            {
                return (HediffCompProperties_LindwurmAcid)this.props;
            }
        }
        public override bool CompShouldRemove
        {
            get
            {
                Pawn pawn = this.Pawn;
                if (pawn.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_LindwurmAcidImmune))
                {
                    return true;
                }
                return false;
            }
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
           

            if (this.Pawn.IsHashIntervalTick(Props.tickInterval))
            {
                Pawn pawn = this.Pawn;
                if (!pawn.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_LindwurmAcidImmune))
                {
                    List<Apparel> apparelList = pawn.apparel.WornApparel;
                    for (int i = 0; i < apparelList.Count; i++)
                    {
                        Thing apparel = apparelList[i];
                        if (apparel != null)
                        {
                            if (apparel.HitPoints <= Props.apparelDamagePerInterval)
                            {
                                apparel.HitPoints = 0;
                                apparel.Destroy(DestroyMode.Vanish);
                            }
                            else
                                apparel.HitPoints -= Props.apparelDamagePerInterval;
                        }

                    }
                }
            }

        }
    }
}