using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
	public class CompRefuelableStat : CompRefuelable
	{
		public new CompProperties_RefuelableStat Props => props as CompProperties_RefuelableStat;
        public CompSlop stewComp;
        private ThingFilter allowedFuelFilter;
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
            Scribe_Deep.Look<ThingFilter>(ref this.allowedFuelFilter, "allowedFuelFilter");
            Scribe_Values.Look<bool>(ref this.allowAutoRefuel, "allowAutoRefuel", false, false);
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

        public virtual int GetFuelCountToFullyRefuel(Thing thing)
		{
			return Mathf.CeilToInt(GetFuelCountToFullyRefuel() / Props.stat.Worker.GetValue(thing, false));
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
            int num = this.GetFuelCountToFullyRefuel();
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

        public override string CompInspectStringExtra()
		{
			return null;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Props.fuelIsMortarBarrel && Find.Storyteller.difficulty.classicMortars) yield break;
			if (Props.targetFuelLevelConfigurable)
			{
				yield return new Command_SetTargetFuelLevel
				{
					refuelable = this,
					defaultLabel = "DankPyon_MealSlop_CommandSetTargetFuelLevel".Translate(Props.stat.LabelCap),
					defaultDesc = "DankPyon_MealSlop_CommandSetTargetFuelLevelDesc".Translate(Props.stat.LabelCap),
					icon = Props.FuelLevelIcon
				};
			}

			if (Props.showFuelGizmo && Find.Selector.SingleSelectedThing == parent)
				yield return new Gizmo_RefuelableFuelStatus
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
	}
}