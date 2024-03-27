using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using MedievalOverhaul.Wood;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class GenerateImpliedDefs_PreResolve_Postfix
    {

        public static void Postfix()
        {
            
            GeneratorUtility.MakeListOfAnimals();
            GeneratorUtilities.MakeListOfTrees();
            foreach (ThingDef def in ThingDefGenerator_Hide.ImpliedHideDefs())
            {
                DefGenerator.AddImpliedDef(def);
            }
            foreach (ThingDef def in ThingDefGenerator_Timber.ImpliedTreeDefs())
            {
                DefGenerator.AddImpliedDef(def);
            }
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
        }
    }
}
