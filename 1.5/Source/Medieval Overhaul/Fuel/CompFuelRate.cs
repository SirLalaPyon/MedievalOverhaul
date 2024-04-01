using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class CompFuelRate : ThingComp
    {
        public CompProperties_FuelRate Props => (CompProperties_FuelRate)this.props;
    }
}
