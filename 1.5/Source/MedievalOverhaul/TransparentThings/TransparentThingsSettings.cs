using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TransparentThings
{
    public class TransparentThingsSettings : ModSettings
    {
        public bool enableTreeTransparency = false;
        public bool enableRoofTransparency = false;
        public bool makeRoofsSelectable = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.enableTreeTransparency, "enableTreeTransparency", false);
            Scribe_Values.Look<bool>(ref this.enableRoofTransparency, "enableRoofTransparency", false);
            Scribe_Values.Look<bool>(ref this.makeRoofsSelectable, "makeRoofsSelectable", true);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            if (Core.hasTransparentTrees)
                listingStandard.CheckboxLabeled((string)"TT.EnableTreeTransparency".Translate(), ref this.enableTreeTransparency);
            if (Core.hasTransparentRoofs)
            {
                listingStandard.CheckboxLabeled((string)"TT.EnableRoofTransparency".Translate(), ref this.enableRoofTransparency);
                listingStandard.CheckboxLabeled((string)"TT.MakeRoofsSelectable".Translate(), ref this.makeRoofsSelectable);
            }
            listingStandard.End();
        }
    }
}
