using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class DefGenerator_GenerateImpliedDefs_PreResolve
    {

        public static bool Prefix(bool hotReload)
        {

            HideUtility.MakeListOfAnimals();
            TimberUtility.MakeListOfTrees();
            foreach (ThingDef def in ThingDefGenerator_Hide.ImpliedHideDefs(hotReload))
            {
                DefGenerator.AddImpliedDef<ThingDef>(def, hotReload);
            }
            foreach (ThingDef def in ThingDefGenerator_Timber.ImpliedTreeDefs())
            {
                DefGenerator.AddImpliedDef<ThingDef>(def, hotReload);
            }
            return true;
        }
    }
}
