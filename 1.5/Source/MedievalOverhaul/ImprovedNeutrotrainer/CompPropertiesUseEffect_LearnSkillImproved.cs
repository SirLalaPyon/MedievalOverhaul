using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedNeurotrainers
{
    internal class CompPropertiesUseEffect_LearnSkillImproved : CompProperties_UseEffect
    {
        public string skillDefName = null;
        public float xpAmount = 0;

        public CompPropertiesUseEffect_LearnSkillImproved() => this.compClass = typeof(CompUseEffect_LearnSkillImproved);
    }
}
