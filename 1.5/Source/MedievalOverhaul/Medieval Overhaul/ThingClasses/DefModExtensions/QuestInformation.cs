using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class QuestInformation : DefModExtension
    {
        public int LinkablesNeeded;
        public int WorkTillTrigger;
        [MustTranslate]
        public string label;
        public bool onlyOnce;
        public ThingDef requiredLinkable;
    }
}
