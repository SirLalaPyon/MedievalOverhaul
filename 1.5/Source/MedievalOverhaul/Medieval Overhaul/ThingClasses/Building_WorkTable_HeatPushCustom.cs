using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class Building_WorkTable_HeatPushCustom : Building_WorkTableCustom
    { 

        public override void UsedThisTick()
        {
            base.UsedThisTick();
            if (Find.TickManager.TicksGame % 30 == 4)
            {
                GenTemperature.PushHeat(this, this.def.building.heatPerTickWhileWorking * HeatPushInterval);
            }
        }

        private const int HeatPushInterval = 30;
    }
}
