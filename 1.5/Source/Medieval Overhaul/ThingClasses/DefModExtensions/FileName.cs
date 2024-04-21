using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class MeteorProperties : DefModExtension
    {

        public FactionDef factionDef;
        public bool spawnAsPlayerFaction = true;
        public Dictionary<ThingDef, PawnKindDef> golemDict;

        public static MeteorProperties Get(Def def) => def.GetModExtension<MeteorProperties>();
    }
}
