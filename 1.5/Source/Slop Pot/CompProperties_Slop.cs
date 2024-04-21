using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{

    public class CompProperties_Slop : CompProperties
	{
		public int fuelCheckTicks = 2500;
		public ThingDef mealDef;
		public int unfueledTicksToRot = 60000;
		public string fullTexture; 

		public CompProperties_Slop()
		{
			compClass = typeof(CompSlop);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			mealDef ??= ThingDefOf.MealNutrientPaste;
		}
	}
}
