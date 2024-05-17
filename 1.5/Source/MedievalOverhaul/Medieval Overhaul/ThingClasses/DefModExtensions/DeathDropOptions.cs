using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class DeathDropOptions
    {
        public List<ThingDef> thingDefs;
        public IntRange amount;
        public float chance;
    }
    public class DeathDrop_Extension : DefModExtension
    {
        public List<DeathDropOptions> deathDropOptions;
       
    }
}