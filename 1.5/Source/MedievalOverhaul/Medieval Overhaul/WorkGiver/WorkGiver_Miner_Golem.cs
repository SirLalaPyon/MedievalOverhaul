using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace MedievalOverhaul
{
    public class WorkGiver_Miner_Golem : WorkGiver_Scanner
    {

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public static void ResetStaticData()
        {
            WorkGiver_Miner_Golem.NoPathTrans = "NoPath".Translate();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            WorkGiver_Miner_Golem.tmpDesignations.Clear();
            WorkGiver_Miner_Golem.tmpDesignations.AddRange(pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Mine));
            WorkGiver_Miner_Golem.tmpDesignations.AddRange(pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.MineVein));
            foreach (Designation designation in WorkGiver_Miner_Golem.tmpDesignations)
            {
                bool flag = false;
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 c = designation.target.Cell + GenAdj.AdjacentCells[i];
                    if (c.InBounds(pawn.Map) && c.Walkable(pawn.Map))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    Mineable_CompSpawnerDestroy firstMineable = designation.target.Cell.GetFirstMineable(pawn.Map);
                    if (firstMineable != null)
                    {
                        yield return firstMineable;
                    }
                }
            }
            yield break;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Mine) && !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.MineVein);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!t.def.mineable)
            {
                return null;
            }
            if (pawn.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) == null && pawn.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.MineVein) == null)
            {
                return null;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return null;
            }
            if (!new HistoryEvent(HistoryEventDefOf.Mined, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
            {
                return null;
            }
            bool flag = false;
            for (int i = 0; i < 8; i++)
            {
                IntVec3 intVec = t.Position + GenAdj.AdjacentCells[i];
                if (intVec.InBounds(pawn.Map) && intVec.Standable(pawn.Map) && ReachabilityImmediate.CanReachImmediate(intVec, t, pawn.Map, PathEndMode.Touch, pawn))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                for (int j = 0; j < 8; j++)
                {
                    IntVec3 intVec2 = t.Position + GenAdj.AdjacentCells[j];
                    if (intVec2.InBounds(t.Map) && ReachabilityImmediate.CanReachImmediate(intVec2, t, pawn.Map, PathEndMode.Touch, pawn) && intVec2.WalkableBy(t.Map, pawn) && !intVec2.Standable(t.Map))
                    {
                        List<Thing> thingList = intVec2.GetThingList(t.Map);
                        for (int k = 0; k < thingList.Count; k++)
                        {
                            if (thingList[k].def.designateHaulable && thingList[k].def.passability == Traversability.PassThroughOnly)
                            {
                                Job job = HaulAIUtility.HaulAsideJobFor(pawn, thingList[k]);
                                if (job != null)
                                {
                                    return job;
                                }
                            }
                        }
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    JobFailReason.Is(WorkGiver_Miner_Golem.NoPathTrans, null);
                    return null;
                }
            }
            return JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_Mine_Golem, t, 20000, true);
        }

        // Token: 0x04002B67 RID: 11111
        private static string NoPathTrans;

        // Token: 0x04002B68 RID: 11112
        private const int MiningJobTicks = 20000;

        // Token: 0x04002B69 RID: 11113
        private static List<Designation> tmpDesignations = new List<Designation>();
    }
}
