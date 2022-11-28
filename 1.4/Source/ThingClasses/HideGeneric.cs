using UnityEngine;
using Verse;

namespace DankPyon
{
    public class HideGeneric : ThingWithComps
    {
        public Color? drawColorOverride;
        public override Color DrawColor
        {
            get
            {
                if (drawColorOverride.HasValue)
                {
                    return this.drawColorOverride.Value;
                }
                return base.DrawColor;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref drawColorOverride, "drawColorOverride");
        }
    }

}
