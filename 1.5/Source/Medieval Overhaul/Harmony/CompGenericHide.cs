using Verse;

namespace MedievalOverhaul
{
    public class CompGenericHide : ThingComp
    {
        public ThingDef pawnSource;
        public int leatherAmount;
        public int marketValue;

        public override bool AllowStackWith(Thing other)
        {
            var comp = other.TryGetComp<CompGenericHide>();
            if (comp?.pawnSource != this.pawnSource)
            {
                return false;
            }
            return base.AllowStackWith(other);
        }

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
                otherComp.pawnSource = this.pawnSource;
                otherComp.leatherAmount = this.leatherAmount;
                otherComp.marketValue = this.marketValue;
                if (piece is HideGeneric hideGeneric)
                {
                    hideGeneric.drawColorOverride = this.pawnSource.race.leatherDef.graphicData.color;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref pawnSource, "pawnSource");
            Scribe_Values.Look(ref leatherAmount, "leatherAmount");
            Scribe_Values.Look(ref marketValue, "MarketValue");
        }
        public override string TransformLabel(string label) => pawnSource == null ? label : pawnSource.label + " " + label;
    }

}
