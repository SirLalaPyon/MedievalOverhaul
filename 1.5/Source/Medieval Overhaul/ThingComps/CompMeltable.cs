using System;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Security.Cryptography;

namespace MedievalOverhaul
{
    public class CompMeltable : ThingComp
    {
        public CompProperties_Meltable PropsRot
        {
            get
            {
                return (CompProperties_Meltable)this.props;
            }
        }

        public float MeltProgressPct
        {
            get
            {
                return this.MeltProgress / (float)this.PropsRot.TicksToMeltStart;
            }
        }

        public float MeltProgress
        {
            get
            {
                return this.meltProgressInt;
            }
            set
            {
                RotStage stage = this.Stage;
                this.meltProgressInt = value;
            }
        }

        public RotStage Stage
        {
            get
            {
                if (this.MeltProgress < (float)this.PropsRot.TicksToMeltStart)
                {
                    return RotStage.Fresh;
                }
                return RotStage.Rotting;
            }
        }

        public bool Active
        {
            get
            {
                return !parent.Position.GetThingList(parent.Map).Any((Thing x) => x.def == MedievalOverhaulDefOf.DankPyon_IceCellar);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.meltProgressInt, "rotProg", 0f, false);
            Scribe_Values.Look<bool>(ref this.disabled, "disabled", false, false);
        }

        public override void CompTick()
        {
            this.Tick(1);
        }

        public override void CompTickRare()
        {
            this.Tick(250);
        }

        private void Tick(int interval)
        {
            if (!this.Active)
            {
                return;
            }
            float num = MeltRateAtTemperature(this.parent.AmbientTemperature);
            MeltProgress += num * (float)interval;
            if (this.Stage == RotStage.Rotting)
            {
                if (this.parent.IsInAnyStorage() && this.parent.SpawnedOrAnyParentSpawned)
                {
                    Messages.Message("MessageRottedAwayInStorage".Translate(this.parent.Label, this.parent).CapitalizeFirst(), new TargetInfo(this.parent.PositionHeld, this.parent.MapHeld, false), MessageTypeDefOf.NegativeEvent, true);
                    LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers, OpportunityType.GoodToKnow);
                }
                this.parent.Destroy(DestroyMode.Vanish);
                return;
            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(this.parent.stackCount + count);
            float rotProgress = ((ThingWithComps)otherStack).GetComp<CompMeltable>().MeltProgress;
            MeltProgress = Mathf.Lerp(this.MeltProgress, rotProgress, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            ((ThingWithComps)piece).GetComp<CompMeltable>().MeltProgress = MeltProgress;
        }

        public override string CompInspectStringExtra()
        {
            if (!this.Active)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            if ((float)this.PropsRot.TicksToMeltStart - MeltProgress > 0f)
            {
                float num = MeltRateAtTemperature((float)Mathf.RoundToInt(this.parent.AmbientTemperature));
                int ticksUntilMeltAtCurrentTemp = TicksUntilMeltAtCurrentTemp;
                float percentage = MeltProgressPct * 100;
                stringBuilder.Append(percentage.ToString("F2") + "% " + "Melted");
                if (num < 0.001f)
                {
                    stringBuilder.Append(string.Format(" ({0})", "CurrentlyFrozen".Translate()));
                }
                else
                    stringBuilder.AppendTagged(string.Format(" ({0})", "DankPyon_Melted".Translate() + " " + ticksUntilMeltAtCurrentTemp.ToStringTicksToPeriod(true, false, true, true, false)));
            }
            return stringBuilder.ToString();
        }

        public static float MeltRateAtTemperature(float temperature)
        {
            if (temperature < 0f)
            {
                return 0f;
            }
            if (temperature >= 20f)
            {
                return 1f;
            }
            return (temperature - 0f) / 20f;
        }


        public int TicksUntilMeltAtTemp(float temp)
        {
            if (!this.Active)
            {
                return 72000000;
            }
            float num = MeltRateAtTemperature(temp);
            if (num <= 0f)
            {
                return 72000000;
            }
            float num2 = (float)this.PropsRot.TicksToMeltStart - MeltProgress;
            if (num2 <= 0f)
            {
                return 0;
            }
            return Mathf.RoundToInt(num2 / num);
        }
        public int TicksUntilMeltAtCurrentTemp
        {
            get
            {
                float ambientTemperature = parent.AmbientTemperature;
                ambientTemperature = Mathf.RoundToInt(ambientTemperature);
                return TicksUntilMeltAtTemp(ambientTemperature);
            }
        }

        private float meltProgressInt;

        public bool disabled;
    }
}