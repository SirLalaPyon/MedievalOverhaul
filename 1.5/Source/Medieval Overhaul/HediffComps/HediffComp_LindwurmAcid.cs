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

        public override void CompPostTick(ref float severityAdjustment)
        {
            Pawn pawn = this.Pawn;

            if (this.Pawn.IsHashIntervalTick(Props.tickInterval))
            {
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