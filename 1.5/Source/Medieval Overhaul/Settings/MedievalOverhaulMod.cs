using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    //public class MedievalOverhaulMod : Mod
    //{
    //    public static MedievalOverhaulSettings settings;
    //    public MedievalOverhaulMod(ModContentPack pack) : base(pack)
    //    {
    //        settings = GetSettings<MedievalOverhaulSettings>();
    //    }
    //    public override void DoSettingsWindowContents(Rect inRect)
    //    {
    //        base.DoSettingsWindowContents(inRect);
    //        settings.DoSettingsWindowContents(inRect);
    //    }
    //    public override string SettingsCategory()
    //    {
    //        return this.Content.Name;
    //    }
    //}

    //public class MedievalOverhaulSettings : ModSettings
    //{
    //    public bool enableMultiTileMeleeAttacks;
    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        Scribe_Values.Look(ref enableMultiTileMeleeAttacks, "enableMultiTileMeleeAttacks");
    //    }
    //    public void DoSettingsWindowContents(Rect inRect)
    //    {
    //        Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
    //        var ls = new Listing_Standard();
    //        ls.Begin(rect);
    //        ls.CheckboxLabeled("MO.EnableMultiTileMeleeAttacks".Translate(), ref enableMultiTileMeleeAttacks);
    //        ls.End();
    //    }
    //}
}
