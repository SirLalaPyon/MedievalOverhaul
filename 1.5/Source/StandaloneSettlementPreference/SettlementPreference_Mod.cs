using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StandaloneSettlementPreference
{
    internal class SettlementPreference_Mod : Mod
    {
        private SettlementPreference_ModSettings settings;

        public SettlementPreference_Mod(ModContentPack contentPack)
          : base(contentPack)
        {
            this.settings = this.GetSettings<SettlementPreference_ModSettings>();
        }

        public override string SettingsCategory() => (string)"StandaloneSettlementPreference_ModName".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Gap();
            listingStandard.CheckboxLabeled((string)"StandaloneSettlementPreference_EnableSettlementPreference".Translate(), ref this.settings.StandaloneSettlementPreference_EnableSettlementPreference, (string)("StandaloneSettlementPreference_EnableSettlementPreferenceTooltip".Translate() + TooltipStringInit.General_SettlementPreference));
            listingStandard.Gap();
            listingStandard.Label((string)("StandaloneSettlementPreference_Iterations".Translate() + " (") + this.settings.StandaloneSettlementPreference_Iterations.ToString() + ")", tooltip: ((string)"ESCP_RaceTools_SettlementPreferenceIterationsTooltip".Translate()));
            this.settings.StandaloneSettlementPreference_Iterations = (int)Math.Round((double)listingStandard.Slider((float)this.settings.StandaloneSettlementPreference_Iterations, 10f, 5000f) / 10.0) * 10;
            listingStandard.Gap();
            if (Prefs.DevMode)
            {
                listingStandard.CheckboxLabeled((string)"StandaloneSettlementPreference_Logging".Translate(), ref this.settings.StandaloneSettlementPreference_Logging, (string)"StandaloneSettlementPreference_LoggingTooltip".Translate());
                listingStandard.Gap();
                listingStandard.CheckboxLabeled((string)"StandaloneSettlementPreference_LoggingExtended".Translate(), ref this.settings.StandaloneSettlementPreference_LoggingExtended, (string)"StandaloneSettlementPreference_LoggingExtendedTooltip".Translate());
                listingStandard.Gap();
            }
            listingStandard.GapLine();
            listingStandard.Gap();
            Rect rect = listingStandard.GetRect(30f);
            TooltipHandler.TipRegion(rect, (TipSignal)"StandaloneSettlementPreference_ResetSettings".Translate());
            if (Widgets.ButtonText(rect, (string)"StandaloneSettlementPreference_ResetSettings".Translate()))
                SettlementPreference_ModSettings.ResetSettings();
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
