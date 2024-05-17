using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TransparentThings
{
    public class ThingExtension : DefModExtension
    {
        public bool transparentWhenPawnIsInsideArea;
        public bool transparentWhenItemIsInsideArea;
        public IntVec2 firstArea;
        public IntVec2 firstAreaOffset;
        public IntVec2 secondArea;
        public IntVec2 secondAreaOffset;
        public List<ThingDef> ignoredThings;
    }
}
