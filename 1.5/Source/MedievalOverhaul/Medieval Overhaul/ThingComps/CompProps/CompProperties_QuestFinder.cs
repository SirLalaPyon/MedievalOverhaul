using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_QuestFinder : CompProperties_Scanner
    {
        public CompProperties_QuestFinder() => this.compClass = typeof(CompQuestFinder);

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            yield break;
        }
    }
}
