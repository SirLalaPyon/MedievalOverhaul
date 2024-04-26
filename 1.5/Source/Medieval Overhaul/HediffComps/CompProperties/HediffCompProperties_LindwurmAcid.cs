using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class HediffCompProperties_LindwurmAcid : HediffCompProperties
    {
        public HediffCompProperties_LindwurmAcid()
        {
            this.compClass = typeof(HediffComp_LindwurmAcid);
        }

        public int tickInterval = 180;

        public int apparelDamagePerInterval = 10;

    }
}