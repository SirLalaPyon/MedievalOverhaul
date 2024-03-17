using ESCP_FuelExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ESCP_FuelExtension
{
    public class CompProperties_FuelRate : CompProperties
    {
        public float rate = 1f;

        public CompProperties_FuelRate() => this.compClass = typeof(CompFuelRate);
    }
}
