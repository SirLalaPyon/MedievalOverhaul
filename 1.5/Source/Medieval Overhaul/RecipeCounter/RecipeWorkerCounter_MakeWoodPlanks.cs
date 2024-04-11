using MedievalOverhaul;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Diagnostics;

namespace MedievalOverhaul
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
            List<ThingDef> childThingDefs = MedievalOverhaulDefOf.DankPyon_Wood.childThingDefs;
			Log.Message(bill.billStack.Count);
            for (int i = 0; i < bill.billStack.Count; i++)
			{
                num += bill.Map.resourceCounter.GetCount(childThingDefs[i]);
			}
			return num;
		}

		public override string ProductsDescription(Bill_Production bill)
		{
			return MedievalOverhaulDefOf.DankPyon_Wood.label;
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
