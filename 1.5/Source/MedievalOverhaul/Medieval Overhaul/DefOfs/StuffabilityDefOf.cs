﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedievalOverhaul
{
    [DefOf]
    public class StuffabilityDefOf
    {
        public static StuffCategoryDef DankPyon_RawWood;

        static StuffabilityDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(StuffabilityDefOf));
    }
}
