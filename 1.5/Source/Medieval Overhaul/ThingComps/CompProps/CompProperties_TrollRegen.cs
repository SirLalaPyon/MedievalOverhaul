using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_TrollRegen : CompProperties
    {
        public int tickInterval = 300;
        public int tickRegenBurn = 1000;
        public int healAmount = 10;

        public CompProperties_TrollRegen() => this.compClass = typeof(Comp_TrollRegen);
    }
}
