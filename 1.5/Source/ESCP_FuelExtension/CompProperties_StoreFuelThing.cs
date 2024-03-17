using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ESCP_FuelExtension
{
    public class CompProperties_StoreFuelThing : CompProperties
    {
        public CompProperties_StoreFuelThing() => this.compClass = typeof(CompStoreFuelThing);
    }
}
