using RimWorld;
using StandaloneSettlementPreference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StandaloneSettlementPreference
{
    [StaticConstructorOnStartup]
    public static class TooltipStringInit
    {
        public static string General_SettlementPreference = TooltipStringInit.General_SettlementPreference_Init();

        public static string General_SettlementPreference_Init()
        {
            string str = "";
            List<string> mods = new List<string>();
            List<int> duplications = new List<int>();
            int other = 0;
            DefDatabase<FactionDef>.AllDefsListForReading.Where<FactionDef>((Func<FactionDef, bool>)(x => SettlementPreference.Get((Def)x) != null)).ToList<FactionDef>().ForEach((Action<FactionDef>)(def =>
            {
                if (def.modContentPack != null)
                {
                    if (!mods.Contains(def.modContentPack.Name))
                    {
                        mods.Add(def.modContentPack.Name);
                        duplications.Add(1);
                    }
                    else
                        duplications[duplications.Count - 1]++;
                }
                else
                    ++other;
            }));
            if (!mods.NullOrEmpty<string>())
            {
                for (int index = 0; index < mods.Count; ++index)
                    str = str + "\n - " + mods[index] + " (" + duplications[index].ToString() + " faction/s)";
            }
            if (str == "")
                str = "\n - None";
            if (other > 0)
                str = str + "\n - Other (" + other.ToString() + " faction/s)";
            return str;
        }
    }
}
