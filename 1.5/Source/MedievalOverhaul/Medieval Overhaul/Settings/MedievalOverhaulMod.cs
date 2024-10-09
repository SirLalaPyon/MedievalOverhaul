using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class MedievalOverhaulSettings : Mod
    {
        public static MedievalOverhaul_Settings settings;
        public string version = "1.5.0.1";
        public MedievalOverhaulSettings(ModContentPack content) : base(content)
        {
            settings = GetSettings<MedievalOverhaul_Settings>();
            Harmony harmony = new Harmony(id: "medievalOverhaul");
            harmony.PatchAll();
            Log.Message("Medieval Overhaul v"+ version);
        }

        public override string SettingsCategory()
        {
            return "MedievalOverhaul.ModNameShort".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
        }

    }
}
