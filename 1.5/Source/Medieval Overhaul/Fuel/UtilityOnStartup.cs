using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class Utility_OnStartup
    {
        public static bool LWMFuelFilterIsEnabled = LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.Name == "LWM's Fuel Filter" || x.PackageId == "LWM.FuelFilter"));
    }
}
