using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_Ghoul : CompProperties
    {
        public float ghoulTicks = 180;
        public string pawnKindDef;
        public SoundDef spawnSound;
        public float foodLevelChange = 0.6f;
        public float postChangeLevel = 0.2f;

        public CompProperties_Ghoul()
        {
            this.compClass = typeof(Comp_Ghoul);
        }
    }
}
