using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StandaloneSettlementPreference
{
    internal class SettlementPreference_ModSettings : ModSettings
    {
        private static SettlementPreference_ModSettings _instance;
        public bool StandaloneSettlementPreference_EnableSettlementPreference = true;
        public bool StandaloneSettlementPreference_Logging = false;
        public bool StandaloneSettlementPreference_LoggingExtended = false;
        public int StandaloneSettlementPreference_Iterations = 50;

        public static bool Enable_SettlementPreference => SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_EnableSettlementPreference;

        public static bool Enable_Logging => SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_Logging;

        public static bool Enable_LoggingExtra => SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_LoggingExtended;

        public static int Count_MaxIterations => SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_Iterations;

        public static void ResetSettings()
        {
            SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_EnableSettlementPreference = true;
            SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_Logging = false;
            SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_LoggingExtended = false;
            SettlementPreference_ModSettings._instance.StandaloneSettlementPreference_Iterations = 50;
        }

        public SettlementPreference_ModSettings() => SettlementPreference_ModSettings._instance = this;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.StandaloneSettlementPreference_EnableSettlementPreference, "StandaloneSettlementPreference_EnableSettlementPreference", true);
            Scribe_Values.Look<bool>(ref this.StandaloneSettlementPreference_Logging, "StandaloneSettlementPreference_Logging");
            Scribe_Values.Look<bool>(ref this.StandaloneSettlementPreference_LoggingExtended, "StandaloneSettlementPreference_LoggingExtended");
            Scribe_Values.Look<int>(ref this.StandaloneSettlementPreference_Iterations, "StandaloneSettlementPreference_Iterations", 50);
        }
    }
}
