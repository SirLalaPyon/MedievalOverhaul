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
    internal class IncidentWorker_GenericWandersIn : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            IncidentProperties incidentProperties = IncidentProperties.Get((Def)this.def);
            if (!base.CanFireNowSub(parms) && incidentProperties != null && incidentProperties.kindDef != null)
                return false;
            Map target = (Map)parms.target;
            return !target.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(incidentProperties.kindDef.race) && this.TryFindEntryCell(target, out IntVec3 _);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            IncidentProperties incidentProperties = IncidentProperties.Get((Def)this.def);
            Map target = (Map)parms.target;
            IntVec3 cell;
            if (!this.TryFindEntryCell(target, out cell))
                return false;
            int num1 = Mathf.Clamp(GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow((IIncidentTarget)target) / incidentProperties.kindDef.combatPower), 1, incidentProperties.max.RandomInRange);
            int num2 = Rand.RangeInclusive(90000, 150000);
            IntVec3 result = IntVec3.Invalid;
            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(cell, target, 10f, out result))
                result = IntVec3.Invalid;
            Pawn pawn = (Pawn)null;
            for (int index = 0; index < num1; ++index)
            {
                IntVec3 intVec3 = CellFinder.RandomClosewalkCellNear(cell, target, 10);
                pawn = PawnGenerator.GeneratePawn(incidentProperties.kindDef);
                GenSpawn.Spawn((Thing)pawn, intVec3, target, Rot4.Random, WipeMode.Vanish, false);
                if (incidentProperties.leaveMapAfterTime)
                    pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num2;
                if (result.IsValid)
                    pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(result, target, 10);
            }
            Find.LetterStack.ReceiveLetter(this.def.letterLabel.Formatted((NamedArgument)incidentProperties.kindDef.label).CapitalizeFirst(), this.def.letterText.Formatted((NamedArgument)num1, (NamedArgument)incidentProperties.kindDef.label), this.def.letterDef, (LookTargets)(Thing)pawn, (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell) => RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
    }
}
