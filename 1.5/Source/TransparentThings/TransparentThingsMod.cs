using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TransparentThings
{
    public class TransparentThingsMod : Mod
    {
        public static TransparentThingsSettings settings;

        public TransparentThingsMod(ModContentPack pack)
          : base(pack)
        {
            TransparentThingsMod.settings = this.GetSettings<TransparentThingsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            TransparentThingsMod.settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => Core.hasTransparentTrees || Core.hasTransparentRoofs ? (string)"TT.TransparentThings".Translate() : "";
    }
}
