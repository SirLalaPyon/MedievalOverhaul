using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ESCP_FuelExtension
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main() => new Harmony("com.ESCP_FuelExtension").PatchAll(Assembly.GetExecutingAssembly());
    }
}
