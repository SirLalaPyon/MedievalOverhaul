using DankPyon;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Diagnostics;

namespace DankPyon_MedievalOverhaul
{
	public class RecipeWorkerCounter_MakeWoodPlanks : RecipeWorkerCounter
	{
		public override bool CanCountProducts(Bill_Production bill)
		{
			return true;
		}

		public override int CountProducts(Bill_Production bill)
		{
			int num = 0;
			List<ThingDef> childThingDefs = DankPyonDefOf.DankPyon_Wood.childThingDefs;
			for (int i = 0; i < childThingDefs.Count; i++)
			{
				num += bill.Map.resourceCounter.GetCount(childThingDefs[i]);
			}
			return num;
		}

		public override string ProductsDescription(Bill_Production bill)
		{
			return DankPyonDefOf.DankPyon_Wood.label;
		}

		public override bool CanPossiblyStore(Bill_Production bill, ISlotGroup slotGroup)
		{
			foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
			{
				if (!allowedThingDef.butcherProducts.NullOrEmpty())
				{
					ThingDef thingDef = allowedThingDef.butcherProducts[0].thingDef;
					if (!slotGroup.Settings.AllowedToAccept(thingDef))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
