using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class DefGenerator_GenerateImpliedDefs_PreResolve
    {

        public static void Postfix()
        {
            //Do this first because I need this list to contain all possible animals regardless of whether they get converted
            GeneratorUtility.MakeListOfShearables();
            foreach (ThingDef def in ThingDefGenerator_Hide.ImpliedHideDefs())
            {
                DefGenerator.AddImpliedDef(def);
            }
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
        }
    }
}
