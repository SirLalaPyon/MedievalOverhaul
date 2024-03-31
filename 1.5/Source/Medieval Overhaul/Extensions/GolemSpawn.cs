using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    internal class GolemSpawn : DefModExtension
    {
        public PawnKindDef kindDef;

        public static GolemSpawn Get(Def def) => def.GetModExtension<GolemSpawn>();
    }

}
