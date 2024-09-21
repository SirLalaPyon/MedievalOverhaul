using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_Meltable : CompProperties
    {

        public int TicksToMeltStart
        {
            get
            {
                return Mathf.RoundToInt(this.daysToMeltStart * 60000f);
            }
        }

        public CompProperties_Meltable()
        {
            this.compClass = typeof(CompMeltable);
        }

        public CompProperties_Meltable(float daysToMeltStart)
        {
            this.daysToMeltStart = daysToMeltStart;
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string text in base.ConfigErrors(parentDef))
            {
                yield return text;
            }
            if (parentDef.tickerType != TickerType.Normal && parentDef.tickerType != TickerType.Rare)
            {
                yield return string.Concat(new object[]
                {
                    "CompRottable needs tickerType ",
                    TickerType.Rare,
                    " or ",
                    TickerType.Normal,
                    ", has ",
                    parentDef.tickerType
                });
            }
            yield break;
        }

        public float daysToMeltStart = 3f;

    }
}