using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    class RecipeExtension_Timber : DefModExtension
    {
        public static RecipeExtension_Timber Get(Def def)
        {
            return def.GetModExtension<RecipeExtension_Timber>();
        }
    }
}
