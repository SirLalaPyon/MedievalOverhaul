using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
	public class CompProperties_RefuelableStat : CompProperties
    {
		public StatDef stat;
		public string fuelLevelIconPath;

        private Texture2D fuelLevelIcon;

        public Texture2D FuelLevelIcon
		{
			get
			{
				if (fuelLevelIcon == null)
					fuelLevelIcon = ContentFinder<Texture2D>.Get(fuelLevelIconPath.NullOrEmpty() ? "UI/Commands/SetTargetFuelLevel" : fuelLevelIconPath);
				return fuelLevelIcon;
			}
		}
        public CompProperties_RefuelableStat()
		{
			compClass = typeof(CompRefuelableStat);
		}
        public string FuelLabel
        {
            get
            {
                if (this.fuelLabel.NullOrEmpty())
                {
                    return "Fuel".TranslateSimple();
                }
                return this.fuelLabel;
            }
        }
        public string FuelGizmoLabel
        {
            get
            {
                if (this.fuelGizmoLabel.NullOrEmpty())
                {
                    return "Fuel".TranslateSimple();
                }
                return this.fuelGizmoLabel;
            }
        }
        public Texture2D FuelIcon
        {
            get
            {
                if (this.fuelIcon == null)
                {
                    if (!this.fuelIconPath.NullOrEmpty())
                    {
                        this.fuelIcon = ContentFinder<Texture2D>.Get(this.fuelIconPath, true);
                    }
                    else
                    {
                        ThingDef thingDef;
                        if (this.fuelFilter.AnyAllowedDef != null)
                        {
                            thingDef = this.fuelFilter.AnyAllowedDef;
                        }
                        else
                        {
                            thingDef = ThingDefOf.Chemfuel;
                        }
                        this.fuelIcon = thingDef.uiIcon;
                    }
                }
                return this.fuelIcon;
            }
        }
        public float FuelMultiplierCurrentDifficulty
        {
            get
            {
                if (this.factorByDifficulty)
                {
                    Storyteller storyteller = Find.Storyteller;
                    if (storyteller?.difficulty != null)
                    {
                        return this.fuelMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
                    }
                }
                return this.fuelMultiplier;
            }
        }
        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            this.fuelFilter.ResolveReferences();
        }
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string text in base.ConfigErrors(parentDef))
            {
                yield return text;
            }
            if (this.destroyOnNoFuel && this.initialFuelPercent <= 0f)
            {
                yield return "Refuelable component has destroyOnNoFuel, but initialFuelPercent <= 0";
            }
            if ((!this.consumeFuelOnlyWhenUsed || this.fuelConsumptionPerTickInRain > 0f) && parentDef.tickerType != TickerType.Normal)
            {
                yield return string.Format("Refuelable component set to consume fuel per tick, but parent tickertype is {0} instead of {1}", parentDef.tickerType, TickerType.Normal);
            }
            yield break;
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry statDrawEntry in base.SpecialDisplayStats(req))
            {
                yield return statDrawEntry;
            }
            if (((ThingDef)req.Def).building.IsTurret)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "ShotsBeforeRearm".Translate(), ((int)this.fuelCapacity).ToString(), "ShotsBeforeRearmExplanation".Translate(), 3171, null, null, false, false);
            }
            yield break;
        }

        public float fuelConsumptionRate = 1f;

        public float fuelCapacity = 2f;

        public float initialFuelPercent;

        public float autoRefuelPercent = 0.3f;

        public float fuelConsumptionPerTickInRain;

        public ThingFilter fuelFilter;
        public ThingFilter defaultIngredientFilter;

        public bool destroyOnNoFuel;

        public bool consumeFuelOnlyWhenUsed;

        public bool consumeFuelOnlyWhenPowered;

        public bool showFuelGizmo;

        public bool initialAllowAutoRefuel = true;

        public bool showAllowAutoRefuelToggle;

        public bool allowRefuelIfNotEmpty = true;

        public bool fuelIsMortarBarrel;

        public bool targetFuelLevelConfigurable;

        public float initialConfigurableTargetFuelLevel;

        public bool drawOutOfFuelOverlay = true;

        public float minimumFueledThreshold;
        public bool drawFuelGaugeInMap;
        public bool atomicFueling;
        private readonly float fuelMultiplier = 1f;
        public bool factorByDifficulty;

        [MustTranslate]
        public string fuelLabel;

        [MustTranslate]
        public string fuelGizmoLabel;

        [MustTranslate]
        public string outOfFuelMessage;
        public string fuelIconPath;
        public bool externalTicking;
        private Texture2D fuelIcon;
    }
}