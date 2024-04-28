using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace MedievalOverhaul
{
    public class Building_ScribeTable : Building_CommsConsole
    {
        private CompAffectedByFacilities facilities;
        private RequireLinkables extension;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.facilities = this.TryGetComp<CompAffectedByFacilities>();
            this.extension = this.def.GetModExtension<RequireLinkables>();
            if (this.extension == null)
            {
                Log.Error(this.def + " " + "does not have the required Mod Extesion.");
            }
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.OpeningComms, OpportunityType.GoodToKnow);
            if (this.CanUseCommsNow)
            {
                LongEventHandler.ExecuteWhenFinished(new Action(this.AnnounceTradeShips));
            }
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            FloatMenuOption failureReason = this.GetFailureReason(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
                yield break;
            }
            foreach (ICommunicable communicable in this.GetCommTargets(myPawn))
            {
                FloatMenuOption floatMenuOption = communicable.CommFloatMenuOption(this, myPawn);
                if (floatMenuOption != null)
                {
                    yield return floatMenuOption;
                }
            }
            IEnumerator<ICommunicable> enumerator = null;
            foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption2;
            }
            IEnumerator<FloatMenuOption> enumerator2 = null;
            yield break;
        }
        private FloatMenuOption GetFailureReason(Pawn myPawn)
        {
            if (this.facilities.LinkedFacilitiesListForReading.Count < extension.linkablesNeeded)
            {
                return new FloatMenuOption("Missing required building", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            for (int i = 0; i < this.facilities.LinkedFacilitiesListForReading.Count; i++)
            {
                Building building = this.facilities.LinkedFacilitiesListForReading[i] as Building;
                if (building == null || building.HasComp<CompRefuelable>() && !building.GetComp<CompRefuelable>().HasFuel)
                {
                    return new FloatMenuOption("Required building needs fuel", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                }
            }
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, myPawn.Named("PAWN"))), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            if (!this.GetCommTargets(myPawn).Any<ICommunicable>())
            {
                return new FloatMenuOption("CannotUseReason".Translate("NoCommsTarget".Translate()), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            if (!this.CanUseCommsNow)
            {
                Log.Error(myPawn + " could not use comm console for unknown reason.");
                return new FloatMenuOption("Cannot use now", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            return null;
        }
        private void AnnounceTradeShips()
        {
            foreach (TradeShip tradeShip in from s in base.Map.passingShipManager.passingShips.OfType<TradeShip>()
                                            where !s.WasAnnounced
                                            select s)
            {
                TaggedString baseLetterText = "TraderArrival".Translate(tradeShip.name, tradeShip.def.label, (tradeShip.Faction == null) ? "TraderArrivalNoFaction".Translate() : "TraderArrivalFromFaction".Translate(tradeShip.Faction.Named("FACTION")));
                IncidentParms incidentParms = new IncidentParms();
                incidentParms.target = base.Map;
                incidentParms.traderKind = tradeShip.TraderKind;
                IncidentWorker.SendIncidentLetter(tradeShip.def.LabelCap, baseLetterText, LetterDefOf.PositiveEvent, incidentParms, LookTargets.Invalid, null, Array.Empty<NamedArgument>());
                tradeShip.WasAnnounced = true;
            }
        }
    }
}
