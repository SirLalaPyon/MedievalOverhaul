using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    class RecipeExtension_Mine : DefModExtension
    {
        public List<ThingDef> bonusGems = null;
        public float randomChance = 0.01f;
        public float workAmountPerChance = 600f;
        public static RecipeExtension_Mine Get(Def def)
        {
            return def.GetModExtension<RecipeExtension_Mine>();
        }
    }
}
