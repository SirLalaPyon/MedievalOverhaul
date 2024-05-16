using NAudio.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace MedievalOverhaul
{
    public class CompGenericHide : ThingComp
    { 
        public int leatherAmount;
        public ThingDef leatherType;
        public int marketValue;
        public float massValue;

        public CompProperties_GenericHide Props
        {
            get
            {
                return (CompProperties_GenericHide)this.props;
            }
        }
        //public override bool AllowStackWith(Thing other)
        //{
        //    var comp = other.TryGetComp<CompGenericHide>();
        //    if (comp?.pawnSource != this.pawnSource)
        //    {
        //        return false;
        //    }
        //    return base.AllowStackWith(other);
        //}

        /// <summary>
        /// Ensures information inside variables is retained after splitting off from a stack
        /// </summary>
        /// <param name="piece">
        /// Is the thing that was split off
        /// </param>
        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            CompGenericHide otherComp = piece.TryGetComp<CompGenericHide>();
            if (otherComp != null)
            {
                otherComp.leatherAmount = this.leatherAmount;
                otherComp.marketValue = this.marketValue;
                //if (piece is HideGeneric hideGeneric)
                //{
                //    hideGeneric.drawColorOverride = this.pawnSource.race.leatherDef.graphicData.color;
                //}
            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            base.PreAbsorbStack(otherStack, count);
            CompGenericHide otherComp = otherStack.TryGetComp<CompGenericHide>();
            otherComp.leatherAmount = ((this.leatherAmount * this.parent.stackCount) + (otherComp.leatherAmount * otherComp.parent.stackCount)) / (this.parent.stackCount + otherStack.stackCount);
            this.leatherAmount = otherComp.leatherAmount;
            var leatherCost = this.Props.leatherType.GetStatValueAbstract(StatDefOf.MarketValue);
            otherComp.marketValue = (int)((int)(leatherAmount * leatherCost) * 0.8f);
            this.marketValue = otherComp.marketValue;
            otherComp.massValue = (leatherAmount * this.Props.leatherType.GetStatValueAbstract(StatDefOf.Mass));
            this.massValue = otherComp.massValue;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref leatherAmount, "leatherAmount");
            Scribe_Defs.Look(ref leatherType, "leatherType");
            Scribe_Values.Look(ref marketValue, "MarketValue");
        }


    }
}
