using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Comp_TrollRegen : ThingComp
    {
        private bool fireFlag = false;
        public CompProperties_TrollRegen Props => (CompProperties_TrollRegen)this.props;
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.HasAttachment(ThingDefOf.Fire))
            {
                fireFlag = true;
            }
            if (this.parent.IsHashIntervalTick(Props.tickInterval) && !fireFlag)
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn.health != null)
                {
                    List<Hediff_Injury> injuryList = new List<Hediff_Injury>();
                    List<Hediff> injuryCheck = pawn.health.hediffSet.hediffs;
                    for (int i = 0; i < injuryCheck.Count; i++)
                    {
                        Hediff_Injury injury = injuryCheck[i] as Hediff_Injury;
                        if (injury != null)
                        {
                            Log.Message(injury);
                            injuryList.Add(injury);
                        }
                    }
                    if (injuryList.Count > 0)
                    {
                        Hediff_Injury hurt = injuryList.RandomElement();
                        hurt.Severity = hurt.Severity - Props.healAmount;
                    }
                }
            }
            if (this.parent.IsHashIntervalTick(Props.tickRegenBurn) && fireFlag)
            {
                if (!this.parent.HasAttachment(ThingDefOf.Fire))
                { 
                    fireFlag = false;
                }
            }
        }
    }
}