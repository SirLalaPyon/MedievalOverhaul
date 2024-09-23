using RimWorld;
using Verse;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class CompRefuelableCustom : ThingComp, IThingGlower
    {
        public static int HashInterval = 250;

        private readonly static float ticksPerDay = 60000f / HashInterval;
        public CompProperties_RefuelableCustom Props
        {
            get
            {
                return (CompProperties_RefuelableCustom)this.props;
            }
        }
        private ThingFilter allowedFuelFilter;
        public ThingFilter AllowedFuelFilter => this.allowedFuelFilter;
        private float ConsumptionRatePerTick
        {
            get
            {
                return this.Props.fuelConsumptionRate / ticksPerDay;
            }
        }
        public float TargetFuelLevel
        {
            get
            {
                if (this.configuredTargetFuelLevel >= 0f)
                {
                    return this.configuredTargetFuelLevel;
                }
                if (this.Props.targetFuelLevelConfigurable)
                {
                    return this.Props.initialConfigurableTargetFuelLevel;
                }
                return this.Props.fuelCapacity;
            }
            set
            {
                this.configuredTargetFuelLevel = Mathf.Clamp(value, 0f, this.Props.fuelCapacity);
            }
        }
        public float Fuel
        {
            get
            {
                return this.fuel;
            }
        }
        public float FuelPercentOfTarget
        {
            get
            {
                return this.fuel / this.TargetFuelLevel;
            }
        }
        public float FuelPercentOfMax
        {
            get
            {
                return this.fuel / this.Props.fuelCapacity;
            }
        }
        public bool IsFull
        {
            get
            {
                return this.HasFuel && this.TargetFuelLevel - this.fuel < 1f;
            }
        }
        public bool HasFuel
        {
            get
            {
                return this.fuel > 0f && this.fuel >= this.Props.minimumFueledThreshold;
            }
        }
        public bool ShouldAutoRefuelNow
        {
            get
            {
                return this.FuelPercentOfTarget <= this.Props.autoRefuelPercent && !this.IsFull && this.TargetFuelLevel > 0f && this.ShouldAutoRefuelNowIgnoringFuelPct;
            }
        }
        public bool ShouldAutoRefuelNowIgnoringFuelPct
        {
            get
            {
                return !this.parent.IsBurning() && (this.flickComp == null || this.flickComp.SwitchIsOn) && this.parent.Map.designationManager.DesignationOn(this.parent, DesignationDefOf.Flick) == null && this.parent.Map.designationManager.DesignationOn(this.parent, DesignationDefOf.Deconstruct) == null;
            }
        }
        public bool ShouldBeLitNow()
        {
            return this.HasFuel;
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.allowAutoRefuel = this.Props.initialAllowAutoRefuel;
            this.fuel = this.Props.fuelCapacity * this.Props.initialFuelPercent;
            this.flickComp = this.parent.GetComp<CompFlickable>();
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.fuel, "fuel", 0f, false);
            Scribe_Values.Look<float>(ref this.configuredTargetFuelLevel, "configuredTargetFuelLevel", -1f, false);
            Scribe_Values.Look<bool>(ref this.allowAutoRefuel, "allowAutoRefuel", false, false);
            Scribe_Deep.Look<ThingFilter>(ref this.allowedFuelFilter, "allowedFuelFilter");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && !this.Props.showAllowAutoRefuelToggle)
            {
                this.allowAutoRefuel = this.Props.initialAllowAutoRefuel;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.allowedFuelFilter != null)
                return;
            this.allowedFuelFilter = new ThingFilter();
            var parentComp = this.parent.GetComp<CompRefuelableCustom>().Props;
            this.allowedFuelFilter.CopyAllowancesFrom(parentComp.fuelFilter);
            var disallowedDefault = parentComp?.defaultIngredientFilter?.disallowedThingDefs;
            if (disallowedDefault != null && disallowedDefault.Count > 0)
            {
                for (int i = 0; i < disallowedDefault.Count; i++)
                {
                    this.allowedFuelFilter.allowedDefs.Remove(disallowedDefault[i]);
                }
            }
            var disallowedCategoryList = parentComp?.defaultIngredientFilter?.disallowedCategories;
            if (disallowedCategoryList != null && disallowedCategoryList.Count > 0)
            {
                for (int i = 0; i < disallowedCategoryList.Count; i++)
                {
                    ThingCategoryDef disallowedCategory = DefDatabase<ThingCategoryDef>.GetNamed(disallowedCategoryList[i], true);
                    if (disallowedCategory != null)
                    {
                        this.allowedFuelFilter.SetAllow(disallowedCategory, false, null, null);
                    }
                }

            }
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (!this.allowAutoRefuel)
            {
                this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.ForbiddenRefuel);
            }
            else if (!this.HasFuel && this.Props.drawOutOfFuelOverlay)
            {
                this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.OutOfFuel);
            }
            if (this.Props.drawFuelGaugeInMap)
            {
                GenDraw.FillableBarRequest r = default;
                r.center = this.parent.DrawPos + Vector3.up * 0.1f;
                r.size = CompRefuelableCustom.FuelBarSize;
                r.fillPercent = this.FuelPercentOfMax;
                r.filledMat = CompRefuelableCustom.FuelBarFilledMat;
                r.unfilledMat = CompRefuelableCustom.FuelBarUnfilledMat;
                r.margin = 0.15f;
                Rot4 rotation = this.parent.Rotation;
                rotation.Rotate(RotationDirection.Clockwise);
                r.rotation = rotation;
                GenDraw.DrawFillableBar(r);
            }
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (this.Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars)
            {
                return;
            }
            if (mode != DestroyMode.Vanish && previousMap != null && this.Props.fuelFilter.AllowedDefCount == 1 && this.Props.initialFuelPercent == 0f)
            {
                ThingDef thingDef = this.Props.fuelFilter.AllowedThingDefs.First<ThingDef>();
                int i = GenMath.RoundRandom(1f * this.fuel);
                while (i > 0)
                {
                    Thing thing = ThingMaker.MakeThing(thingDef, null);
                    thing.stackCount = Mathf.Min(i, thingDef.stackLimit);
                    i -= thing.stackCount;
                    GenPlace.TryPlaceThing(thing, this.parent.Position, previousMap, ThingPlaceMode.Near, null, null, default);
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            if (this.Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars)
            {
                return string.Empty;
            }
            string text = string.Concat(
            [
                this.Props.FuelLabel,
                ": ",
                this.fuel.ToStringDecimalIfSmall(),
                " / ",
                this.Props.fuelCapacity.ToStringDecimalIfSmall()
            ]);
            if (!this.Props.consumeFuelOnlyWhenUsed && this.HasFuel)
            {
                int numTicks = (int)(this.fuel / this.Props.fuelConsumptionRate * 60000f);
                text = text + " (" + numTicks.ToStringTicksToPeriod(true, false, true, true, false) + ")";
            }
            if (!this.HasFuel && !this.Props.outOfFuelMessage.NullOrEmpty())
            {
                string arg = (this.parent.def.building != null && this.parent.def.building.IsTurret) ? ("CannotShoot".Translate() + ": " + this.Props.outOfFuelMessage).Resolve() : this.Props.outOfFuelMessage;
                text += string.Format("\n{0} ({1}x {2})", arg, this.GetFuelCountToFullyRefuel(), this.Props.fuelFilter.AnyAllowedDef.label);
            }
            if (this.Props.targetFuelLevelConfigurable)
            {
                text += "\n" + "ConfiguredTargetFuelLevel".Translate(this.TargetFuelLevel.ToStringDecimalIfSmall());
            }
            return text;
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (this.parent.def.building != null && this.parent.def.building.IsTurret)
            {
                TaggedString taggedString = "RearmCostExplanation".Translate();
                if (this.Props.factorByDifficulty)
                {
                    taggedString += " (" + "RearmCostExplanationDifficulty".Translate() + ")";
                }
                taggedString += ".";
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "RearmCost".Translate(), GenLabel.ThingLabel(this.Props.fuelFilter.AnyAllowedDef, null, this.GetFuelCountToFullyRefuel()).CapitalizeFirst(), taggedString, 3171, null, null, false, false);
            }
            yield break;
        }
        public override void CompTick()
        {
            if (this.parent.IsHashIntervalTick(HashInterval))
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                if (!this.Props.consumeFuelOnlyWhenUsed && (this.flickComp == null || this.flickComp.SwitchIsOn) && (!this.Props.consumeFuelOnlyWhenPowered || (comp != null && comp.PowerOn)) && !this.Props.externalTicking)
                {
                    this.ConsumeFuel(this.ConsumptionRatePerTick);
                }
                if (this.Props.fuelConsumptionPerTickInRain > 0f && this.parent.Spawned && this.parent.Map.weatherManager.RainRate > 0.4f && !this.parent.Map.roofGrid.Roofed(this.parent.Position) && !this.Props.externalTicking)
                {
                    this.ConsumeFuel(this.Props.fuelConsumptionPerTickInRain);
                }
            }
        }

        public void ConsumeFuel(float amount)
        {
            if (this.fuel <= 0f)
            {
                return;
            }
            this.fuel -= amount;
            if (this.fuel <= 0f)
            {
                this.fuel = 0f;
                if (this.Props.destroyOnNoFuel)
                {
                    this.parent.Destroy(DestroyMode.Vanish);
                }
                this.parent.BroadcastCompSignal("RanOutOfFuel");
            }
        }
        public void Refuel(List<Thing> fuelThings)
        {
            if (this.Props.atomicFueling)
            {
                if (fuelThings.Sum((Thing t) => t.stackCount) < this.GetFuelCountToFullyRefuel())
                {
                    Log.ErrorOnce("Error refueling; not enough fuel available for proper atomic refuel", 19586442);
                    return;
                }
            }
            int fullFuelCount = this.GetFuelCountToFullyRefuel();
            while (fullFuelCount > 0 && fuelThings.Count > 0)
            {
                Thing thing = fuelThings.Pop<Thing>();
                float fuelValue = thing.def?.GetModExtension<FuelValueProperty>()?.fuelValue ?? 1f;
                int maxFuelNeededFromStack = Mathf.CeilToInt(fullFuelCount / fuelValue);
                int amountToFuel = Mathf.Min(maxFuelNeededFromStack, thing.stackCount);
                this.Refuel((float)amountToFuel * fuelValue);
                thing.SplitOff(amountToFuel).Destroy(DestroyMode.Vanish);
                fullFuelCount -= (int)(amountToFuel * fuelValue);
            }
        }

        public void Refuel(float amount)
        {
            this.fuel += amount * this.Props.FuelMultiplierCurrentDifficulty;
            if (this.fuel > this.Props.fuelCapacity)
            {
                this.fuel = this.Props.fuelCapacity;
            }
            this.parent.BroadcastCompSignal("Refueled");
        }
        public void Notify_UsedThisTick()
        {
            if (this.parent.IsHashIntervalTick(HashInterval))
            {
                this.ConsumeFuel(this.ConsumptionRatePerTick);
            }
        }
        public int GetFuelCountToFullyRefuel()
        {
            if (this.Props.atomicFueling)
            {
                return Mathf.CeilToInt(this.Props.fuelCapacity / this.Props.FuelMultiplierCurrentDifficulty);
            }
            return Mathf.Max(Mathf.CeilToInt((this.TargetFuelLevel - this.fuel) / this.Props.FuelMultiplierCurrentDifficulty), 1);
        }
        public int GetFuelCountToFullyRefuel(Thing thing)
        {
            float fuelValue = thing.def?.GetModExtension<FuelValueProperty>()?.fuelValue ?? 1f;
            return Mathf.CeilToInt(GetFuelCountToFullyRefuel() / fuelValue);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars)
            {
                yield break;
            }
            if (this.Props.targetFuelLevelConfigurable)
            {
                yield return new Command_SetTargetFuelLevelCustom
                {
                    refuelable = this,
                    defaultLabel = "CommandSetTargetFuelLevel".Translate(),
                    defaultDesc = "CommandSetTargetFuelLevelDesc".Translate(),
                    icon = CompRefuelableCustom.SetTargetFuelLevelCommand
                };
            }
            if (this.Props.showFuelGizmo && Find.Selector.SingleSelectedThing == this.parent)
            {
                yield return new Gizmo_RefuelableStatusCustom
                {
                    refuelable = this
                };
            }
            if (this.Props.showAllowAutoRefuelToggle)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandToggleAllowAutoRefuel".Translate(),
                    defaultDesc = "CommandToggleAllowAutoRefuelDesc".Translate(),
                    hotKey = KeyBindingDefOf.Command_ItemForbid,
                    icon = (this.allowAutoRefuel ? TexCommand.ForbidOff : TexCommand.ForbidOn),
                    isActive = (() => this.allowAutoRefuel),
                    toggleAction = delegate ()
                    {
                        this.allowAutoRefuel = !this.allowAutoRefuel;
                    }
                };
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Set fuel to 0",
                    action = delegate ()
                    {
                        this.fuel = 0f;
                        this.parent.BroadcastCompSignal("Refueled");
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Set fuel to 0.1",
                    action = delegate ()
                    {
                        this.fuel = 0.1f;
                        this.parent.BroadcastCompSignal("Refueled");
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Fuel -20%",
                    action = delegate ()
                    {
                        this.ConsumeFuel(this.Props.fuelCapacity * 0.2f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Set fuel to max",
                    action = delegate ()
                    {
                        this.fuel = this.Props.fuelCapacity;
                        this.parent.BroadcastCompSignal("Refueled");
                    }
                };
            }
            yield break;
        }
        private float fuel;

        private float configuredTargetFuelLevel = -1f;

        public bool allowAutoRefuel = true;

        private CompFlickable flickComp;

        public const string RefueledSignal = "Refueled";

        public const string RanOutOfFuelSignal = "RanOutOfFuel";

        private static readonly Texture2D SetTargetFuelLevelCommand = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel", true);

        private static readonly Vector2 FuelBarSize = new (1f, 0.2f);

        private static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f), false);

        private static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);

    }
}
