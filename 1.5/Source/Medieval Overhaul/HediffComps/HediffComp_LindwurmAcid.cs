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
                List<Apparel> apparelList = pawn.apparel.WornApparel;
                for (int i = 0; i < apparelList.Count; i++)
                {
                    Thing apparel = apparelList[i];
                    if (apparel != null && apparel.def.GetModExtension<ApparelExtension_ImmuneLindwurmAcid>() == null)
                    {
                        if (apparel.HitPoints <= Props.apparelDamagePerInterval)
                        {
                            apparel.HitPoints = 0;
                            apparel.Destroy(DestroyMode.Vanish);
                           // Find.LetterStack.ReceiveLetter(this.Props.letterLabelOnDisappear.Formatted(base.Pawn.Named("PAWN")), this.Props.letterTextOnDisappear.Formatted(base.Pawn.Named("PAWN")), LetterDefOf.PositiveEvent, base.Pawn, null, null, null, null, 0, true);
                        }
                        else
                            apparel.HitPoints -= Props.apparelDamagePerInterval;
                    }

                }
            }
            
        }
    }
}