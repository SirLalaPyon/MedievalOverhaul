using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class DamageWorker_Daze : DamageWorker
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageWorker.DamageResult result = new DamageWorker.DamageResult();
            Pawn pawn = victim as Pawn;
            if (pawn != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, pawn, null);
                hediff.Severity = dinfo.Amount;
                pawn.health.AddHediff(hediff, null, new DamageInfo?(dinfo), null);
            }
            return result;
        }
    }
}
