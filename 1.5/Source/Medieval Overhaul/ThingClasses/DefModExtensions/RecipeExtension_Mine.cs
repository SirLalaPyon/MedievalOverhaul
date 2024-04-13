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
        public static RecipeExtension_Mine Get(Def def)
        {
            return def.GetModExtension<RecipeExtension_Mine>();
        }
    }
}
