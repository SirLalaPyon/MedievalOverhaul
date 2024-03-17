using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GenericWandersIn
{
    internal class IncidentProperties : DefModExtension
    {
        public PawnKindDef kindDef;
        public IntRange max = new IntRange(3, 5);
        public bool leaveMapAfterTime = true;

        public static IncidentProperties Get(Def def) => def.GetModExtension<IncidentProperties>();
    }
}
