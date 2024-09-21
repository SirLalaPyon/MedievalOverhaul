using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MedievalOverhaul
{

	public class Building_SlopPot : Building_NutrientPasteDispenser
	{
		public CompRefuelableCustom fuelComp;

		public int lastFueledTick = -999;

		public CompRefuelableStat nutritionComp;
		public CompSlop slopComp;
		public float nutritionAmount;

        public override ThingDef DispensableDef => slopComp.Props.mealDef;

		public override Color DrawColor =>
			!this.IsSociallyProper(null, false) ? Building_Bed.SheetColorForPrisoner : base.DrawColor;

		public override bool HasEnoughFeedstockInHoppers()
		{
			return nutritionComp.Fuel >= def.building.nutritionCostPerDispense;
		}


		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			fuelComp = GetComp<CompRefuelableCustom>();
			nutritionComp = GetComp<CompRefuelableStat>();
			slopComp = GetComp<CompSlop>();
			if (slopComp == null) throw new Exception($"{def.defName} does not have CompProperties_Slop");
			if (nutritionComp == null)
				throw new Exception(
					$"{def.defName} does not have CompProperties_RefuelableStat with with at least one food category defined");
		}

		public override void Tick()
		{
			base.Tick();

            this.fuelComp.Notify_UsedThisTick();
            var ticks = Find.TickManager.TicksGame;
			if (ticks % slopComp.Props.fuelCheckTicks != 0) return;
			if (fuelComp.HasFuel) lastFueledTick = ticks;
			if (lastFueledTick < 0 || ticks - lastFueledTick <= slopComp.Props.unfueledTicksToRot) return;
			nutritionComp.ConsumeFuel(nutritionComp.Fuel);
			slopComp.ingredients.Clear();
			Messages.Message("DankPyon_MealSlop_Spoiled".TranslateSimple().CapitalizeFirst(),
				new TargetInfo(PositionHeld, MapHeld), MessageTypeDefOf.NegativeEvent);
		}

		public override Thing TryDispenseFood()
		{
			if (!CanDispenseNow)
				return null;
			nutritionComp.ConsumeFuel(def.building.nutritionCostPerDispense);
            Thing meal = ThingMaker.MakeThing(slopComp.Props.mealDef);
            CompIngredients compIngredients = meal.TryGetComp<CompIngredients>();
            for (int i = 0; i < this.slopComp.ingredients.Count; i++)
            {
                compIngredients.RegisterIngredient(this.slopComp.ingredients[i]);
            }
            if (this.nutritionComp.Fuel == 0)
            {
                this.slopComp.ingredients.Clear();
            }
            def.building.soundDispense.PlayOneShot(new TargetInfo(Position, Map));
			return meal;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new();
			stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.Append(
                "DankPyon_StewMealAmount".Translate() + ": " +
                (int)Math.Round(nutritionComp.Fuel / def.building.nutritionCostPerDispense) + "/" +
                    MaxMeals());
            stringBuilder.AppendLine(" | " +
				"DankPyon_StewPot_NutritionStored".Translate() + ": " +
                (int)Math.Round(nutritionComp.Fuel));
            if (!this.fuelComp.HasFuel && this.nutritionComp.HasFuel)
            {
                stringBuilder.AppendLine("DankPyon_SlopRotting".Translate());
            }
            else if(this.slopComp.ingredients.Count > 0)
            {
                stringBuilder.AppendLine("Ingredients".Translate() + ": ");
                stringBuilder.Append(this.slopComp.GetIngredientsString(false, out var _));
            }
			
			return stringBuilder.ToString().Trim();
		}

		public virtual int MaxMeals()
		{
			return (int)Math.Round(nutritionComp.TargetFuelLevel / def.building.nutritionCostPerDispense);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastFueledTick, "lastFueledTick", -999);

        }

		public bool Accepts(Thing t)
		{
			return nutritionComp.Props.fuelFilter.Allows(t);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
				if (!(gizmo is Designator_Build dbGizmo && dbGizmo.PlacingDef == ThingDefOf.Hopper))
					yield return gizmo;
			yield return new Command_Action
			{
				defaultLabel = "Empty stew pot",
				action = delegate ()
				{
					nutritionComp.ConsumeFuel(nutritionComp.Fuel);
					slopComp.ingredients.Clear();

				},
				icon = ContentFinder<Texture2D>.Get("UI/EmptyPot_Icon")
            };
        }
    }
}