using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_GenericHide : CompProperties
    {
        public ThingDef leatherType;
        public int leatherAmount;
        public CompProperties_GenericHide() => this.compClass = typeof(CompGenericHide);
    }
}
