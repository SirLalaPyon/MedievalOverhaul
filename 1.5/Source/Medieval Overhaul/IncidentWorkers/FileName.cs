using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI.Group;
using Verse;

namespace MedievalOverhaul
{
    public class IncidentWorker_GolemCrash: IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return Faction.OfMechanoids != null && ((Map)parms.target).listerThings.ThingsOfDef(this.def.mechClusterBuilding).Count <= 0;
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<TargetInfo> list = new List<TargetInfo>();
            ThingDef shipPartDef = this.def.mechClusterBuilding;
            IntVec3 intVec = IncidentWorker_CrashedShipPart.FindDropPodLocation(map, (IntVec3 spot) => base.< TryExecuteWorker > g__CanPlaceAt | 0(spot));
            if (intVec == IntVec3.Invalid)
            {
                return false;
            }
            float points = Mathf.Max(parms.points * 0.9f, 300f);
            List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = map.Tile,
                faction = Faction.OfMechanoids,
                points = points
            }, true).ToList<Pawn>();
            Thing thing = ThingMaker.MakeThing(shipPartDef, null);
            thing.SetFaction(Faction.OfMechanoids, null);
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend(new List<Thing>
            {
                thing
            }, Faction.OfMechanoids, 28f, intVec, false, false), map, list2);
            DropPodUtility.DropThingsNear(intVec, map, list2.Cast<Thing>(), 110, false, false, true, true, true, null);
            foreach (Pawn thing2 in list2)
            {
                CompCanBeDormant compCanBeDormant = thing2.TryGetComp<CompCanBeDormant>();
                if (compCanBeDormant != null)
                {
                    compCanBeDormant.ToSleep();
                }
            }
            list.AddRange(from p in list2
                          select new TargetInfo(p));
            GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, thing), intVec, map, WipeMode.Vanish);
            list.Add(new TargetInfo(intVec, map, false));
            base.SendStandardLetter(parms, list, Array.Empty<NamedArgument>());
            return true;
        }
        private static IntVec3 FindDropPodLocation(Map map, Predicate<IntVec3> validator)
        {
            for (int i = 0; i < 200; i++)
            {
                IntVec3 intVec = RCellFinder.FindSiegePositionFrom(DropCellFinder.FindRaidDropCenterDistant(map, true), map, true, true, null, true);
                if (validator(intVec))
                {
                    return intVec;
                }
            }
            return IntVec3.Invalid;
        }
        private const float ShipPointsFactor = 0.9f;
        private const int IncidentMinimumPoints = 300;
        private const float DefendRadius = 28f;
    }
}
