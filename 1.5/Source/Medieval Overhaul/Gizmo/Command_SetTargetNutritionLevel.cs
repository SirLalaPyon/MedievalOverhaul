using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class Command_SetTargetNutritionLevel : Command
    {
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (this.refuelables == null)
            {
                this.refuelables = new List<CompRefuelableStat>();
            }
            if (!this.refuelables.Contains(this.refuelable))
            {
                this.refuelables.Add(this.refuelable);
            }
            int num = int.MaxValue;
            for (int i = 0; i < this.refuelables.Count; i++)
            {
                if ((int)this.refuelables[i].Props.fuelCapacity < num)
                {
                    num = (int)this.refuelables[i].Props.fuelCapacity;
                }
            }
            int startingValue = num / 2;
            for (int j = 0; j < this.refuelables.Count; j++)
            {
                if ((int)this.refuelables[j].TargetFuelLevel <= num)
                {
                    startingValue = (int)this.refuelables[j].TargetFuelLevel;
                    break;
                }
            }
            Func<int, string> textGetter;
            if (this.refuelable.parent.def.building.hasFuelingPort)
            {
                textGetter = ((int x) => "SetPodLauncherTargetFuelLevel".Translate(x, CompLaunchable.MaxLaunchDistanceAtFuelLevel((float)x)));
            }
            else
            {
                textGetter = ((int x) => "SetTargetFuelLevel".Translate(x));
            }
            Dialog_Slider dialog_Slider = new Dialog_Slider(textGetter, 0, num, delegate (int value)
            {
                for (int k = 0; k < this.refuelables.Count; k++)
                {
                    this.refuelables[k].TargetFuelLevel = (float)value;
                }
            }, startingValue, 1f);
            if (this.refuelable.parent.def.building.hasFuelingPort)
            {
                dialog_Slider.extraBottomSpace = Text.LineHeight + 4f;
            }
            Find.WindowStack.Add(dialog_Slider);
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (this.refuelables == null)
            {
                this.refuelables = new List<CompRefuelableStat>();
            }
            this.refuelables.Add(((Command_SetTargetNutritionLevel)other).refuelable);
            return false;
        }

        public CompRefuelableStat refuelable;

        private List<CompRefuelableStat> refuelables;
    }
}
