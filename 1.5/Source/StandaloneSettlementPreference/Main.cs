using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StandaloneSettlementPreference
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main() => new Harmony("com.ESCP_RaceTools").PatchAll(Assembly.GetExecutingAssembly());
    }
}
