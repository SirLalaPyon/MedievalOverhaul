using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace MedievalOverhaul
{
	public class CompSlop : ThingComp
	{
        public List<ThingDef> ingredients = new List<ThingDef>();
        public CompProperties_Slop Props => (CompProperties_Slop)props;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look<ThingDef>(ref this.ingredients, "ingredients", LookMode.Def, Array.Empty<object>());
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.ingredients == null)
            {
                this.ingredients = new List<ThingDef>();
            }
        }
        public string GetIngredientsString(bool includeMergeCompatibility, out bool hasMergeCompatibilityIngredients)
        {
            StringBuilder stringBuilder = new StringBuilder();
            hasMergeCompatibilityIngredients = false;
            for (int i = 0; i < this.ingredients.Count; i++)
            {
                ThingDef thingDef = this.ingredients[i];
                stringBuilder.Append((i == 0) ? thingDef.LabelCap.Resolve() : thingDef.label);
                if (i < this.ingredients.Count - 1)
                {
                    stringBuilder.Append(", ");
                }
            }
            return stringBuilder.ToString();
        }
    }
    
}