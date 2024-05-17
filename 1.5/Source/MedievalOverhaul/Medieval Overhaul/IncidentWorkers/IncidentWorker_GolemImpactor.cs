using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static HarmonyLib.Code;

namespace MedievalOverhaul
{
    public class IncidentWorker_GolemImpactor : IncidentWorker
    {
        private const float DefendRadius = 28f;
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            return true;
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            MeteorProperties meteorProperties = MeteorProperties.Get((Def)this.def);
            Map map = (Map)parms.target;
            List<TargetInfo> targetInfoList = new List<TargetInfo>();
            ThingDef golemImpactDef = FindRandomImpact(meteorProperties.golemDict);
            IntVec3 dropPodLocation = IncidentWorker_GolemImpactor.FindDropPodLocation(map, (Predicate<IntVec3>)(spot => CanPlaceAt(spot)));
            if (dropPodLocation == IntVec3.Invalid)
                return false;
            float num = Mathf.Max(parms.points * 0.9f, 300f);
            float storytellerPoints = StorytellerUtility.DefaultThreatPointsNow(map);
            PawnKindDef golemKindDef = meteorProperties.golemDict.TryGetValue(golemImpactDef);
            int num2 = GenMath.RoundRandom(storytellerPoints / golemKindDef.combatPower);
            num2 = Mathf.Clamp(num2, 1, 20);
            List<Pawn> list = new List<Pawn>();
            for (int i = 0; i < num2; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(golemKindDef, meteorProperties.factionDef != null && FactionUtility.DefaultFactionFrom(meteorProperties.factionDef) != null ? FactionUtility.DefaultFactionFrom(meteorProperties.factionDef) : null);
                list.Add(pawn);
            }
            Thing innerThing = ThingMaker.MakeThing(golemImpactDef);
            innerThing.SetFaction(list[0].Faction);
            LordMaker.MakeNewLord(list[0].Faction, (LordJob)new LordJob_SleepThenMechanoidsDefend(new List<Thing>()
      {
        innerThing
      }, list[0].Faction, DefendRadius, dropPodLocation, false, false), map, (IEnumerable<Pawn>)list);
            DropPodUtility.DropThingsNear(dropPodLocation, map, list.Cast<Thing>());
            foreach (Thing thing in list)
                thing.TryGetComp<CompCanBeDormant>()?.ToSleep();
            targetInfoList.AddRange(list.Select<Pawn, TargetInfo>((Func<Pawn, TargetInfo>)(p => new TargetInfo((Thing)p))));
            GenSpawn.Spawn((Thing)SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, innerThing), dropPodLocation, map);
            targetInfoList.Add(new TargetInfo(dropPodLocation, map));
            this.SendStandardLetter(parms, (LookTargets)targetInfoList);
            return true;

            bool CanPlaceAt(IntVec3 loc)
            {
                CellRect cellRect = GenAdj.OccupiedRect(loc, Rot4.North, golemImpactDef.Size);
                if (loc.Fogged(map) || !cellRect.InBounds(map) || !DropCellFinder.SkyfallerCanLandAt(loc, map, golemImpactDef.Size))
                    return false;
                foreach (IntVec3 c in cellRect)
                {
                    RoofDef roof = c.GetRoof(map);
                    if (roof != null && roof.isNatural)
                        return false;
                }
                return GenConstruct.CanBuildOnTerrain((BuildableDef)golemImpactDef, loc, map, Rot4.North);
            }
        }

        private static IntVec3 FindDropPodLocation(Map map, Predicate<IntVec3> validator)
        {
            for (int index = 0; index < 200; ++index)
            {
                IntVec3 siegePositionFrom = RCellFinder.FindSiegePositionFrom(DropCellFinder.FindRaidDropCenterDistant(map, true), map, true);
                if (validator(siegePositionFrom))
                    return siegePositionFrom;
            }
            return IntVec3.Invalid;
        }

        public static ThingDef FindRandomImpact(Dictionary<ThingDef, PawnKindDef> dict)
        {
            int num = Rand.Range(0, dict.Count());
            ThingDef randomKey = dict.Keys.ElementAt(num);
            return randomKey;
        }
    }
}