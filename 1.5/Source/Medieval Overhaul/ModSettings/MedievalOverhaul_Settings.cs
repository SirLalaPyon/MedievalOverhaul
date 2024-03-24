using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Verse;

namespace MedievalOverhaul
{
    public class MedievalOverhaul_Settings : ModSettings
    {
        public static List<ThingDef> AllLeatherAnimals = new List<ThingDef>();
        public static Dictionary<string, bool> settingMode = new();

        // Debug Mode
        public bool debugMode = false;

        // Production Chains
        public bool leatherChain = true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref leatherChain, "leatherChain", true);
            Scribe_Values.Look(ref debugMode, "debugMode", false);
            Scribe_Collections.Look(ref settingMode, "settingMode", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                FieldInfo[] fields = GetType().GetFields();
                settingMode ??= new Dictionary<string, bool>();
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(bool))
                    {
                        bool value = (bool)field.GetValue(this);
                        settingMode.SetOrAdd(field.Name, value);
                    }
                }
            }
            
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled((string)"DankPyon_Settings_LeatherChain".Translate(), ref this.leatherChain);
            listingStandard.End();
        }

        


    }
}
