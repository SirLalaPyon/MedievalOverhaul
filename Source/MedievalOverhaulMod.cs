using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;
using Verse.Noise;

namespace DankPyon
{
    public class MedievalOverhaulMod : Mod
    {
        public static MedievalOverhaulSettings settings;
        public MedievalOverhaulMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<MedievalOverhaulSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
    }

    public class MedievalOverhaulSettings : ModSettings
    {
        public bool enableTreeTransparency;
        public bool enableMultiTileMeleeAttacks;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableTreeTransparency, "enableTreeTransparency");
            Scribe_Values.Look(ref enableMultiTileMeleeAttacks, "enableMultiTileMeleeAttacks");
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            var ls = new Listing_Standard();
            ls.Begin(rect);
            ls.CheckboxLabeled("MO.EnableTreeTransparency".Translate(), ref enableTreeTransparency);
            ls.CheckboxLabeled("MO.EnableMultiTileMeleeAttacks".Translate(), ref enableMultiTileMeleeAttacks);
            ls.End();
        }
    }
}
