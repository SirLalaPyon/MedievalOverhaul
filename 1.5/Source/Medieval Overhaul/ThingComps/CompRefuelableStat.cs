using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class CompRefuelableStat : ThingComp, IThingGlower
    {
        public new CompProperties_RefuelableStat Props => props as CompProperties_RefuelableStat;
        public CompSlop stewComp;
        private ThingFilter allowedFuelFilter;
        public float TargetFuelLevel
        {
            get
            {
                if (this.configuredTargetNutritionLevel >= 0f)
                {
                    return this.configuredTargetNutritionLevel;
                }
                if (this.Props.targetFuelLevelConfigurable)
                {
                    return this.Props.initialConfigurableTargetFuelLevel;
                }
                return this.Props.fuelCapacity;
            }
            set
            {
                this.configuredTargetNutritionLevel = Mathf.Clamp(value, 0f, this.Props.fuelCapacity);
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
        public ThingFilter AllowedFuelFilter => this.allowedFuelFilter;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.allowedFuelFilter != null)
                return;
            this.allowedFuelFilter = new ThingFilter();
            this.allowedFuelFilter.CopyAllowancesFrom(this.parent.GetComp<CompRefuelableStat>().Props.fuelFilter);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.fuel, "nutritionFuelAmount", 0f, false);
            Scribe_Values.Look<float>(ref this.configuredTargetNutritionLevel, "configuredTargetNutritionLevel", -1f, false);
            Scribe_Deep.Look<ThingFilter>(ref this.allowedFuelFilter, "allowedNutritionFuelFilter");
            Scribe_Values.Look<bool>(ref this.allowAutoRefuel, "allowNutritionAutoRefuel", false, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && !this.Props.showAllowAutoRefuelToggle)
            {
                this.allowAutoRefuel = this.Props.initialAllowAutoRefuel;
            }
        }
        public void RegisterIngredient(Thing thing)
        {
            Building_SlopPot building = this.parent as Building_SlopPot;
            building.slopComp.ingredients.AddDistinct(thing.def);
        }

        public int GetFuelCountToFullyRefuel(Thing thing)
        {
            return Mathf.CeilToInt(GetFuelCountToFullyRefuel() / Props.stat.Worker.GetValue(thing, false));
        }
        public void ConsumeFuel(float amount)
        {
            if (this.Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars)
            {
                return;
            }
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
            int num = this.GetFuelCountToFullyRefuel(fuelThings[0]);
            while (num > 0 && fuelThings.Count > 0)
            {

                Thing thing = fuelThings.Pop<Thing>();
                float fuelAmount = Mathf.Min((float)thing.stackCount * thing.GetStatValue(Props.stat), Props.fuelCapacity - Fuel);
                int num2 = Mathf.Min(num, thing.stackCount);
                RegisterIngredient(thing);
                this.Refuel((float)fuelAmount);
                thing.SplitOff(num2).Destroy(DestroyMode.Vanish);
                num -= num2;
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

        public override string CompInspectStringExtra()
        {
            return null;
        }
        public int GetFuelCountToFullyRefuel()
        {
            if (this.Props.atomicFueling)
            {
                return Mathf.CeilToInt(this.Props.fuelCapacity / this.Props.FuelMultiplierCurrentDifficulty);
            }
            return Mathf.Max(Mathf.CeilToInt((this.TargetFuelLevel - this.fuel) / this.Props.FuelMultiplierCurrentDifficulty), 1);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars) yield break;
            if (Props.targetFuelLevelConfigurable)
            {
                yield return new Command_SetTargetNutritionLevel
                {
                    refuelable = this,
                    defaultLabel = "DankPyon_MealSlop_CommandSetTargetFuelLevel".Translate(Props.stat.LabelCap),
                    defaultDesc = "DankPyon_MealSlop_CommandSetTargetFuelLevelDesc".Translate(Props.stat.LabelCap),
                    icon = Props.FuelLevelIcon
                };
            }

            if (Props.showFuelGizmo && Find.Selector.SingleSelectedThing == parent)
                yield return new Gizmo_RefuelableNutritionStatus
                {
                    refuelable = this
                };
            if (Props.showAllowAutoRefuelToggle)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "DankPyon_MealSlop_CommandToggleAllowAutoRefuel".Translate(Props.stat.LabelCap),
                    defaultDesc = "DankPyon_MealSlop_CommandToggleAllowAutoRefuelDesc".Translate(Props.stat.LabelCap),
                    hotKey = KeyBindingDefOf.Command_TogglePower,
                    icon = allowAutoRefuel ? TexCommand.ForbidOff : (Texture)TexCommand.ForbidOn,
                    isActive = () => allowAutoRefuel,
                    toggleAction = () => allowAutoRefuel = !allowAutoRefuel
                };
            }

            if (!DebugSettings.ShowDevGizmos) yield break;
            yield return new Command_Action
            {
                defaultLabel = $"DEV: Set {Props.stat.LabelCap} to 0",
                action = () => ConsumeFuel(Fuel)
            };
            yield return new Command_Action
            {
                defaultLabel = $"DEV: Set {Props.stat.LabelCap} to 0.1",
                action = () =>
                {
                    ConsumeFuel(Fuel);
                    Refuel(0.1f);
                }
            };
            yield return new Command_Action
            {
                defaultLabel = $"DEV: Set {Props.stat.LabelCap} to max",
                action = () => Refuel(Props.fuelCapacity - Fuel)
            };
        }
        private float fuel;

        private float configuredTargetNutritionLevel = -1f;

        public bool allowAutoRefuel = true;

        private CompFlickable flickComp;

        public const string RefueledSignal = "Refueled";

        public const string RanOutOfFuelSignal = "RanOutOfFuel";

        private static readonly Texture2D SetTargetFuelLevelCommand = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel", true);

        private static readonly Vector2 FuelBarSize = new Vector2(1f, 0.2f);

        private static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f), false);

        private static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
    }
}