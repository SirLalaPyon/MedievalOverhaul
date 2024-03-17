using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ESCP_ThingProducer
{
    public class Comp_ThingProducer : ThingComp
    {
        private int ticksUntilDone;
        private string pausedReason;

        public CompProperties_ThingProducer Props => (CompProperties_ThingProducer)this.props;

        public override void PostPostMake()
        {
            base.PostPostMake();
            this.ticksUntilDone = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksUntilDone, "ticksUntilDone");
        }

        public bool CanProgress()
        {
            int num = 0;
            if (this.IsMinified)
                return false;
            if (this.Props.onlyUnroofed && this.IsRoofed)
            {
                ++num;
                this.pausedReason = (string)"ESCP_Tools_ThingProducer_Reason_roofed".Translate();
            }
            if (this.Props.restAtNight && this.Resting)
            {
                ++num;
                this.pausedReason = (string)"ESCP_Tools_ThingProducer_Reason_time".Translate();
            }
            if (this.Props.inTempRange && !this.Props.viableTempRange.Includes(this.Temperature))
            {
                ++num;
                this.pausedReason = (string)"ESCP_Tools_ThingProducer_Reason_temperature".Translate();
            }
            if (this.Props.disabledWeathers != null && (!this.Props.invertWeathers && this.Props.disabledWeathers.Contains(this.Weather) || this.Props.invertWeathers && !this.Props.disabledWeathers.Contains(this.Weather)))
            {
                ++num;
                this.pausedReason = (string)"ESCP_Tools_ThingProducer_Reason_weather".Translate();
            }
            if (this.Props.requireFuel)
            {
                CompRefuelable comp = this.parent.GetComp<CompRefuelable>();
                if (comp != null && !comp.HasFuel)
                {
                    ++num;
                    this.pausedReason = (string)"ESCP_Tools_ThingProducer_Reason_fuel".Translate();
                }
            }
            if (this.Props.requirePower)
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                if (comp != null && !comp.PowerOn)
                {
                    ++num;
                    this.pausedReason = (string)"ESCP_Tools_ThingProducer_Reason_power".Translate();
                }
            }
            if (num > 1)
                this.pausedReason = this.pausedReason + " + " + (num - 1).ToString();
            return num == 0;
        }

        public void Produce()
        {
            bool flag = false;
            if (!this.Props.items.NullOrEmpty<Item>())
            {
                if (this.Props.random)
                {
                    this.Spawn(this.Props.items.RandomElementByWeight<Item>((Func<Item, float>)(x => (float)x.weight)));
                }
                else
                {
                    foreach (Item obj in this.Props.items)
                    {
                        if (this.Spawn(obj))
                            flag = true;
                    }
                }
            }
            if (!(this.Props.displayMessage & flag))
                return;
            Messages.Message((string)"ESCP_Tools_ThingProducer_Message".Translate((NamedArgument)(Thing)this.parent), (LookTargets)(Thing)this.parent, MessageTypeDefOf.PositiveEvent, false);
        }

        public bool Spawn(Item item)
        {
            Thing thing = ThingMaker.MakeThing(item.thingDef, item.stuffDef);
            int randomInRange = item.countRange.RandomInRange;
            if (randomInRange <= 0)
                return false;
            thing.stackCount = randomInRange;
            GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
            return true;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!this.CanProgress())
                return;
            if (this.ticksUntilDone >= this.Props.timeRequired)
            {
                this.ticksUntilDone = 0;
                this.Produce();
            }
            ++this.ticksUntilDone;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!this.CanProgress())
                return;
            if (this.ticksUntilDone >= this.Props.timeRequired)
            {
                this.ticksUntilDone = 0;
                this.Produce();
            }
            this.ticksUntilDone += 250;
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            if (!this.CanProgress())
                return;
            if (this.ticksUntilDone >= this.Props.timeRequired)
            {
                this.ticksUntilDone = 0;
                this.Produce();
            }
            this.ticksUntilDone += 2000;
        }

        public override string CompInspectStringExtra() => (string)(this.CanProgress() ? "ESCP_Tools_ThingProducer_Progress".Translate((NamedArgument)((float)this.ticksUntilDone / (float)this.Props.timeRequired).ToStringPercent()) : (this.IsMinified ? (TaggedString)(string)null : "ESCP_Tools_ThingProducer_Paused".Translate((NamedArgument)this.pausedReason)));

        protected virtual bool IsMinified => !this.parent.Spawned;

        protected virtual bool IsRoofed => this.parent.Position.Roofed(this.parent.Map);

        protected virtual bool Resting => (double)GenLocalDate.DayPercent((Thing)this.parent) < 0.25 || (double)GenLocalDate.DayPercent((Thing)this.parent) > 0.800000011920929;

        protected virtual float Temperature
        {
            get
            {
                float tempResult;
                return GenTemperature.TryGetTemperatureForCell(this.parent.Position, this.parent.Map, out tempResult) ? tempResult : 20f;
            }
        }

        protected virtual WeatherDef Weather => this.parent.Map.weatherManager.curWeather;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                Command_Action commandAction1 = new Command_Action();
                commandAction1.defaultLabel = "Debug: Set progress to 0";
                commandAction1.action = (Action)(() => this.ticksUntilDone = 0);
                yield return (Gizmo)commandAction1;
                Command_Action commandAction2 = new Command_Action();
                commandAction2.defaultLabel = "Debug: Set progress to max";
                commandAction2.action = (Action)(() => this.ticksUntilDone = this.Props.timeRequired);
                yield return (Gizmo)commandAction2;
            }
        }
    }
}
