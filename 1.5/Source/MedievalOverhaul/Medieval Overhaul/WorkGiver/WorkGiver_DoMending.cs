using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace MedievalOverhaul
{
    public class WorkGiver_DoMending : WorkGiver_Scanner
    {
        private List<ThingCount> chosenIngThings = [];
        private static List<IngredientCount> missingIngredients = [];
        private static List<Thing> tmpMissingUniqueIngredients = [];
        private static readonly IntRange ReCheckFailedBillTicksRange = new (500, 600);
        private static List<Thing> relevantThings = [];
        private static HashSet<Thing> processedThings = [];
        private static List<Thing> newRelevantThings = [];
        private static List<Thing> tmpMedicine = [];
        private static WorkGiver_DoMending.DefCountList availableCounts = new ();

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Some;

        public override ThingRequest PotentialWorkThingRequest => this.def.fixedBillGiverDefs != null && this.def.fixedBillGiverDefs.Count == 1 ? ThingRequest.ForDef(this.def.fixedBillGiverDefs[0]) : ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Thing> thingList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.PotentialBillGiver);
            for (int index = 0; index < thingList.Count; ++index)
            {
                if (thingList[index] is IBillGiver billGiver && billGiver != pawn && this.ThingIsUsableBillGiver(thingList[index]) && billGiver.BillStack.AnyShouldDoNow)
                    return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (!(thing is IBillGiver giver) || !this.ThingIsUsableBillGiver(thing) || !giver.BillStack.AnyShouldDoNow || !giver.UsableForBillsAfterFueling() || !pawn.CanReserve((LocalTargetInfo)thing, ignoreOtherReservations: forced) || thing.IsBurning() || thing.IsForbidden(pawn))
                return (Job)null;
            if (thing.def.hasInteractionCell && !ReservationUtility.CanReserveSittableOrSpot(pawn, thing.InteractionCell))
                return (Job)null;
            CompRefuelable comp = thing.TryGetComp<CompRefuelable>();
            if (comp != null && !comp.HasFuel)
                return !RefuelWorkGiverUtility.CanRefuel(pawn, thing, forced) ? (Job)null : RefuelWorkGiverUtility.RefuelJob(pawn, thing, forced);
            giver.BillStack.RemoveIncompletableBills();
            return this.StartOrResumeBillJob(pawn, giver, forced);
        }

        private static UnfinishedThing ClosestUnfinishedThingForBill(
          Pawn pawn,
          Bill_ProductionWithUft bill)
        {
            Predicate<Thing> validator = (Predicate<Thing>)(t => !t.IsForbidden(pawn) && ((UnfinishedThing)t).Recipe == bill.recipe && ((UnfinishedThing)t).Creator == pawn && ((UnfinishedThing)t).ingredients.TrueForAll((Predicate<Thing>)(x => bill.IsFixedOrAllowedIngredient(x.def))) && pawn.CanReserve((LocalTargetInfo)t));
            return (UnfinishedThing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(bill.recipe.unfinishedThingDef), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger()), validator: validator);
        }

        private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
        {
            if (uft.Creator != pawn)
            {
                Log.Error("Tried to get FinishUftJob for " + (object)pawn + " finishing " + (object)uft + " but its creator is " + (object)uft.Creator);
                return (Job)null;
            }
            Job job1 = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, bill.billStack.billGiver, (Thing)uft);
            if (job1 != null && job1.targetA.Thing != uft)
                return job1;
            Job job2 = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_DoBillMending, (LocalTargetInfo)(Thing)bill.billStack.billGiver);
            job2.bill = (Bill)bill;
            job2.targetQueueB = new List<LocalTargetInfo>()
      {
        (LocalTargetInfo) (Thing) uft
      };
            job2.countQueue = new List<int>() { 1 };
            job2.haulMode = HaulMode.ToCellNonStorage;
            return job2;
        }

        private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver, bool forced = false)
        {
            bool flag1 = FloatMenuMakerMap.makingFor == pawn;
            for (int index = 0; index < giver.BillStack.Count; ++index)
            {
                Bill bill1 = giver.BillStack[index];
                if ((bill1.recipe.requiredGiverWorkType == null || bill1.recipe.requiredGiverWorkType == this.def.workType) && (Find.TickManager.TicksGame > bill1.nextTickToSearchForIngredients || FloatMenuMakerMap.makingFor == pawn) && bill1.ShouldDoNow() && bill1.PawnAllowedToStartAnew(pawn))
                {
                    SkillRequirement skillRequirement = bill1.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
                    if (skillRequirement != null)
                    {
                        JobFailReason.Is((string)"UnderRequiredSkill".Translate((NamedArgument)skillRequirement.minLevel), bill1.Label);
                    }
                    else
                    {
                        if (bill1 is Bill_Medical bill2)
                        {
                            if (bill2.IsSurgeryViolationOnExtraFactionMember(pawn))
                            {
                                JobFailReason.Is((string)"SurgeryViolationFellowFactionMember".Translate());
                                continue;
                            }
                            if (!pawn.CanReserve((LocalTargetInfo)(Thing)bill2.GiverPawn, ignoreOtherReservations: forced))
                            {
                                Pawn pawn1 = pawn.MapHeld.reservationManager.FirstRespectedReserver((LocalTargetInfo)(Thing)bill2.GiverPawn, pawn);
                                JobFailReason.Is((string)"IsReservedBy".Translate((NamedArgument)bill2.GiverPawn.LabelShort, (NamedArgument)pawn1.LabelShort));
                                continue;
                            }
                        }
                        if (bill1 is Bill_Mech billMech && billMech.Gestator.WasteProducer.Waste != null && billMech.Gestator.GestatingMech == null)
                        {
                            JobFailReason.Is((string)"WasteContainerFull".Translate());
                        }
                        else
                        {
                            if (bill1 is Bill_ProductionWithUft bill3)
                            {
                                if (bill3.BoundUft != null)
                                {
                                    if (bill3.BoundWorker == pawn && pawn.CanReserveAndReach((LocalTargetInfo)(Thing)bill3.BoundUft, PathEndMode.Touch, Danger.Deadly) && !bill3.BoundUft.IsForbidden(pawn))
                                        return WorkGiver_DoMending.FinishUftJob(pawn, bill3.BoundUft, bill3);
                                    continue;
                                }
                                UnfinishedThing uft = WorkGiver_DoMending.ClosestUnfinishedThingForBill(pawn, bill3);
                                if (uft != null)
                                    return WorkGiver_DoMending.FinishUftJob(pawn, uft, bill3);
                            }
                            if (bill1 is Bill_Autonomous bill4 && bill4.State != FormingState.Gathering)
                                return WorkGiver_DoMending.WorkOnFormedBill((Thing)giver, bill4);
                            List<IngredientCount> ingredientCountList = (List<IngredientCount>)null;
                            if (flag1)
                            {
                                ingredientCountList = WorkGiver_DoMending.missingIngredients;
                                ingredientCountList.Clear();
                                WorkGiver_DoMending.tmpMissingUniqueIngredients.Clear();
                            }
                            bool? nullable;
                            if (bill1 is Bill_Medical billMedical)
                            {
                                List<Thing> requiredIngredients = billMedical.uniqueRequiredIngredients;
                                nullable = requiredIngredients != null ? new bool?(requiredIngredients.NullOrEmpty<Thing>()) : new bool?();
                                bool flag2 = false;
                                if (nullable.GetValueOrDefault() == flag2 & nullable.HasValue)
                                {
                                    foreach (Thing requiredIngredient in billMedical.uniqueRequiredIngredients)
                                    {
                                        if (requiredIngredient.IsForbidden(pawn) || !pawn.CanReserveAndReach((LocalTargetInfo)requiredIngredient, PathEndMode.OnCell, Danger.Deadly))
                                            WorkGiver_DoMending.tmpMissingUniqueIngredients.Add(requiredIngredient);
                                    }
                                }
                            }
                            if (!WorkGiver_DoMending.TryFindBestBillIngredients(bill1, pawn, (Thing)giver, this.chosenIngThings, ingredientCountList) || !WorkGiver_DoMending.tmpMissingUniqueIngredients.NullOrEmpty<Thing>())
                            {
                                if (FloatMenuMakerMap.makingFor != pawn)
                                    bill1.nextTickToSearchForIngredients = Find.TickManager.TicksGame + WorkGiver_DoMending.ReCheckFailedBillTicksRange.RandomInRange;
                                else if (flag1)
                                {
                                    if (WorkGiver_DoMending.CannotDoBillDueToMedicineRestriction(giver, bill1, ingredientCountList))
                                        JobFailReason.Is((string)"NoMedicineMatchingCategory".Translate(WorkGiver_DoMending.GetMedicalCareCategory((Thing)giver).GetLabel().Named("CATEGORY")), bill1.Label);
                                    else
                                        JobFailReason.Is((string)"MissingMaterials".Translate((NamedArgument)ingredientCountList.Select<IngredientCount, string>((Func<IngredientCount, string>)(missing => missing.Summary)).Concat<string>(WorkGiver_DoMending.tmpMissingUniqueIngredients.Select<Thing, string>((Func<Thing, string>)(t => t.Label))).ToCommaList()), bill1.Label);
                                    flag1 = false;
                                }
                                this.chosenIngThings.Clear();
                            }
                            else
                            {
                                Job job = WorkGiver_DoMending.TryStartNewDoBillJob(pawn, bill1, giver, this.chosenIngThings, out Job _);
                                this.chosenIngThings.Clear();
                                return job;
                            }
                        }
                    }
                }
            }
            this.chosenIngThings.Clear();
            return (Job)null;
        }

        private static bool CannotDoBillDueToMedicineRestriction(
          IBillGiver giver,
          Bill bill,
          List<IngredientCount> missingIngredients)
        {
            if (!(giver is Pawn billGiver))
                return false;
            bool flag = false;
            foreach (IngredientCount missingIngredient in missingIngredients)
            {
                if (missingIngredient.filter.Allows(ThingDefOf.MedicineIndustrial))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
                return false;
            MedicalCareCategory medicalCareCategory = WorkGiver_DoMending.GetMedicalCareCategory((Thing)billGiver);
            foreach (Thing t in billGiver.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine))
            {
                if (WorkGiver_DoMending.IsUsableIngredient(t, bill) && medicalCareCategory.AllowsMedicine(t.def))
                    return false;
            }
            return true;
        }

        public static Job TryStartNewDoBillJob(
          Pawn pawn,
          Bill bill,
          IBillGiver giver,
          List<ThingCount> chosenIngThings,
          out Job haulOffJob,
          bool dontCreateJobIfHaulOffRequired = true)
        {
            haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, (Thing)null);
            if (haulOffJob != null & dontCreateJobIfHaulOffRequired)
                return haulOffJob;
            Job job = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_DoBillMending, (LocalTargetInfo)(Thing)giver);
            job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
            job.countQueue = new List<int>(chosenIngThings.Count);
            for (int index = 0; index < chosenIngThings.Count; ++index)
            {
                job.targetQueueB.Add((LocalTargetInfo)chosenIngThings[index].Thing);
                job.countQueue.Add(chosenIngThings[index].Count);
            }
            if (bill.xenogerm != null)
            {
                job.targetQueueB.Add((LocalTargetInfo)(Thing)bill.xenogerm);
                job.countQueue.Add(1);
            }
            job.haulMode = HaulMode.ToCellNonStorage;
            job.bill = bill;
            return job;
        }

        private static Job WorkOnFormedBill(Thing giver, Bill_Autonomous bill)
        {
            Job job = JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_DoBillMending, (LocalTargetInfo)giver);
            job.bill = (Bill)bill;
            return job;
        }

        public bool ThingIsUsableBillGiver(Thing thing)
        {
            Pawn pawn1 = thing as Pawn;
            Corpse corpse = thing as Corpse;
            Pawn pawn2 = (Pawn)null;
            if (corpse != null)
                pawn2 = corpse.InnerPawn;
            return this.def.fixedBillGiverDefs != null && this.def.fixedBillGiverDefs.Contains(thing.def) || pawn1 != null && (this.def.billGiversAllHumanlikes && pawn1.RaceProps.Humanlike || this.def.billGiversAllMechanoids && pawn1.RaceProps.IsMechanoid || this.def.billGiversAllAnimals && pawn1.IsNonMutantAnimal) || corpse != null && pawn2 != null && (this.def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike || this.def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.IsMechanoid || this.def.billGiversAllAnimalsCorpses && pawn2.IsNonMutantAnimal);
        }

        private static bool IsUsableIngredient(Thing t, Bill bill)
        {
            if (!bill.IsFixedOrAllowedIngredient(t))
                return false;
            foreach (IngredientCount ingredient in bill.recipe.ingredients)
            {
                if (ingredient.filter.Allows(t) && t.HitPoints < t.MaxHitPoints)
                    return true;
            }
            return false;
        }

        public static bool TryFindBestFixedIngredients(
          List<IngredientCount> ingredients,
          Pawn pawn,
          Thing ingredientDestination,
          List<ThingCount> chosen,
          float searchRadius = 999f)
        {
            return WorkGiver_DoMending.TryFindBestIngredientsHelper((Predicate<Thing>)(t =>
            {
                foreach (IngredientCount ingredient in ingredients)
                {
                    if (ingredient.filter.Allows(t))
                        return true;
                }
                return false;
            }), (Predicate<List<Thing>>)(foundThings => WorkGiver_DoMending.TryFindBestIngredientsInSet_NoMixHelper(foundThings, ingredients, chosen, WorkGiver_DoMending.GetBillGiverRootCell(ingredientDestination, pawn), false, (List<IngredientCount>)null)), ingredients, pawn, ingredientDestination, chosen, searchRadius);
        }

        private static bool TryFindBestBillIngredients(
          Bill bill,
          Pawn pawn,
          Thing billGiver,
          List<ThingCount> chosen,
          List<IngredientCount> missingIngredients)
        {
            return WorkGiver_DoMending.TryFindBestIngredientsHelper((Predicate<Thing>)(t => WorkGiver_DoMending.IsUsableIngredient(t, bill)), (Predicate<List<Thing>>)(foundThings => WorkGiver_DoMending.TryFindBestBillIngredientsInSet(foundThings, bill, chosen, WorkGiver_DoMending.GetBillGiverRootCell(billGiver, pawn), billGiver is Pawn, missingIngredients)), bill.recipe.ingredients, pawn, billGiver, chosen, bill.ingredientSearchRadius);
        }

        private static bool TryFindBestIngredientsHelper(
          Predicate<Thing> thingValidator,
          Predicate<List<Thing>> foundAllIngredientsAndChoose,
          List<IngredientCount> ingredients,
          Pawn pawn,
          Thing billGiver,
          List<ThingCount> chosen,
          float searchRadius)
        {
            chosen.Clear();
            WorkGiver_DoMending.newRelevantThings.Clear();
            if (ingredients.Count == 0)
                return true;
            Region rootReg = WorkGiver_DoMending.GetBillGiverRootCell(billGiver, pawn).GetRegion(pawn.Map);
            if (rootReg == null)
                return false;
            WorkGiver_DoMending.relevantThings.Clear();
            WorkGiver_DoMending.processedThings.Clear();
            bool foundAll = false;
            float radiusSq = searchRadius * searchRadius;
            Predicate<Thing> baseValidator = (Predicate<Thing>)(t => t.Spawned && thingValidator(t) && (double)(t.Position - billGiver.Position).LengthHorizontalSquared < (double)radiusSq && !t.IsForbidden(pawn) && pawn.CanReserve((LocalTargetInfo)t));
            bool billGiverIsPawn = billGiver is Pawn;
            if (billGiverIsPawn)
            {
                WorkGiver_DoMending.AddEveryMedicineToRelevantThings(pawn, billGiver, WorkGiver_DoMending.relevantThings, baseValidator, pawn.Map);
                if (foundAllIngredientsAndChoose(WorkGiver_DoMending.relevantThings))
                {
                    WorkGiver_DoMending.relevantThings.Clear();
                    return true;
                }
            }
            if (billGiver is Building_WorkTableAutonomous workTableAutonomous)
            {
                WorkGiver_DoMending.relevantThings.AddRange((IEnumerable<Thing>)workTableAutonomous.innerContainer);
                if (foundAllIngredientsAndChoose(WorkGiver_DoMending.relevantThings))
                {
                    WorkGiver_DoMending.relevantThings.Clear();
                    return true;
                }
            }
            TraverseParms traverseParams = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition = (RegionEntryPredicate)null;
            entryCondition = (double)Math.Abs(999f - searchRadius) < 1.0 ? (RegionEntryPredicate)((from, r) => r.Allows(traverseParams, false)) : (RegionEntryPredicate)((from, r) =>
            {
                if (!r.Allows(traverseParams, false))
                    return false;
                CellRect extentsClose = r.extentsClose;
                int num1 = Math.Abs(billGiver.Position.x - Math.Max(extentsClose.minX, Math.Min(billGiver.Position.x, extentsClose.maxX)));
                if ((double)num1 > (double)searchRadius)
                    return false;
                int num2 = Math.Abs(billGiver.Position.z - Math.Max(extentsClose.minZ, Math.Min(billGiver.Position.z, extentsClose.maxZ)));
                return (double)num2 <= (double)searchRadius && (double)(num1 * num1 + num2 * num2) <= (double)radiusSq;
            });
            int adjacentRegionsAvailable = rootReg.Neighbors.Count<Region>((Func<Region, bool>)(region => entryCondition(rootReg, region)));
            int regionsProcessed = 0;
            WorkGiver_DoMending.processedThings.AddRange<Thing>(WorkGiver_DoMending.relevantThings);
            int num = foundAllIngredientsAndChoose(WorkGiver_DoMending.relevantThings) ? 1 : 0;
            RegionProcessor regionProcessor = (RegionProcessor)(r =>
            {
                List<Thing> thingList = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int index = 0; index < thingList.Count; ++index)
                {
                    Thing thing = thingList[index];
                    if (!WorkGiver_DoMending.processedThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn) && baseValidator(thing) && !(thing.def.IsMedicine & billGiverIsPawn))
                    {
                        WorkGiver_DoMending.newRelevantThings.Add(thing);
                        WorkGiver_DoMending.processedThings.Add(thing);
                    }
                }
                ++regionsProcessed;
                if (WorkGiver_DoMending.newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
                {
                    WorkGiver_DoMending.relevantThings.AddRange((IEnumerable<Thing>)WorkGiver_DoMending.newRelevantThings);
                    WorkGiver_DoMending.newRelevantThings.Clear();
                    if (foundAllIngredientsAndChoose(WorkGiver_DoMending.relevantThings))
                    {
                        foundAll = true;
                        return true;
                    }
                }
                return false;
            });
            RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999);
            WorkGiver_DoMending.relevantThings.Clear();
            WorkGiver_DoMending.newRelevantThings.Clear();
            WorkGiver_DoMending.processedThings.Clear();
            return foundAll;
        }

        private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
        {
            if (!(billGiver is Building building))
                return billGiver.Position;
            if (building.def.hasInteractionCell)
                return building.InteractionCell;
            Log.Error("Tried to find bill ingredients for " + (object)billGiver + " which has no interaction cell.");
            return forPawn.Position;
        }

        private static void AddEveryMedicineToRelevantThings(
          Pawn pawn,
          Thing billGiver,
          List<Thing> relevantThings,
          Predicate<Thing> baseValidator,
          Map map)
        {
            MedicalCareCategory medicalCareCategory = WorkGiver_DoMending.GetMedicalCareCategory(billGiver);
            List<Thing> thingList = map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
            WorkGiver_DoMending.tmpMedicine.Clear();
            for (int index = 0; index < thingList.Count; ++index)
            {
                Thing dest = thingList[index];
                if (medicalCareCategory.AllowsMedicine(dest.def) && baseValidator(dest) && pawn.CanReach((LocalTargetInfo)dest, PathEndMode.OnCell, Danger.Deadly))
                    WorkGiver_DoMending.tmpMedicine.Add(dest);
            }
            WorkGiver_DoMending.tmpMedicine.SortBy<Thing, float, int>((Func<Thing, float>)(x => -x.GetStatValue(StatDefOf.MedicalPotency)), (Func<Thing, int>)(x => x.Position.DistanceToSquared(billGiver.Position)));
            relevantThings.AddRange((IEnumerable<Thing>)WorkGiver_DoMending.tmpMedicine);
            WorkGiver_DoMending.tmpMedicine.Clear();
        }

        public static MedicalCareCategory GetMedicalCareCategory(Thing billGiver) => billGiver is Pawn pawn && pawn.playerSettings != null ? pawn.playerSettings.medCare : MedicalCareCategory.Best;

        private static bool TryFindBestBillIngredientsInSet(
          List<Thing> availableThings,
          Bill bill,
          List<ThingCount> chosen,
          IntVec3 rootCell,
          bool alreadySorted,
          List<IngredientCount> missingIngredients)
        {
            return bill.recipe.allowMixingIngredients ? WorkGiver_DoMending.TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen, rootCell, missingIngredients) : WorkGiver_DoMending.TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen, rootCell, alreadySorted, missingIngredients);
        }

        private static bool TryFindBestBillIngredientsInSet_NoMix(
          List<Thing> availableThings,
          Bill bill,
          List<ThingCount> chosen,
          IntVec3 rootCell,
          bool alreadySorted,
          List<IngredientCount> missingIngredients)
        {
            return WorkGiver_DoMending.TryFindBestIngredientsInSet_NoMixHelper(availableThings, bill.recipe.ingredients, chosen, rootCell, alreadySorted, missingIngredients, bill);
        }

        private static bool TryFindBestIngredientsInSet_NoMixHelper(
          List<Thing> availableThings,
          List<IngredientCount> ingredients,
          List<ThingCount> chosen,
          IntVec3 rootCell,
          bool alreadySorted,
          List<IngredientCount> missingIngredients,
          Bill bill = null)
        {
            if (!alreadySorted)
            {
                Comparison<Thing> comparison = (Comparison<Thing>)((t1, t2) => ((float)(t1.PositionHeld - rootCell).LengthHorizontalSquared).CompareTo((float)(t2.PositionHeld - rootCell).LengthHorizontalSquared));
                availableThings.Sort(comparison);
            }
            chosen.Clear();
            WorkGiver_DoMending.availableCounts.Clear();
            missingIngredients?.Clear();
            WorkGiver_DoMending.availableCounts.GenerateFrom(availableThings);
            for (int index1 = 0; index1 < ingredients.Count; ++index1)
            {
                IngredientCount ingredient = ingredients[index1];
                bool flag = false;
                for (int index2 = 0; index2 < WorkGiver_DoMending.availableCounts.Count; ++index2)
                {
                    float f = bill != null ? (float)ingredient.CountRequiredOfFor(WorkGiver_DoMending.availableCounts.GetDef(index2), bill.recipe, bill) : ingredient.GetBaseCount();
                    if ((bill == null || bill.recipe.ignoreIngredientCountTakeEntireStacks || (double)f <= (double)WorkGiver_DoMending.availableCounts.GetCount(index2)) && ingredient.filter.Allows(WorkGiver_DoMending.availableCounts.GetDef(index2)) && (bill == null || ingredient.IsFixedIngredient || bill.ingredientFilter.Allows(WorkGiver_DoMending.availableCounts.GetDef(index2))))
                    {
                        for (int index3 = 0; index3 < availableThings.Count; ++index3)
                        {
                            if (availableThings[index3].def == WorkGiver_DoMending.availableCounts.GetDef(index2))
                            {
                                int num = availableThings[index3].stackCount - ThingCountUtility.CountOf(chosen, availableThings[index3]);
                                if (num > 0)
                                {
                                    if (bill != null && bill.recipe.ignoreIngredientCountTakeEntireStacks)
                                    {
                                        ThingCountUtility.AddToList(chosen, availableThings[index3], num);
                                        return true;
                                    }
                                    int countToAdd = Mathf.Min(Mathf.FloorToInt(f), num);
                                    ThingCountUtility.AddToList(chosen, availableThings[index3], countToAdd);
                                    f -= (float)countToAdd;
                                    if ((double)f < 1.0 / 1000.0)
                                    {
                                        flag = true;
                                        float val = WorkGiver_DoMending.availableCounts.GetCount(index2) - f;
                                        WorkGiver_DoMending.availableCounts.SetCount(index2, val);
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag)
                            break;
                    }
                }
                if (!flag)
                {
                    if (missingIngredients == null)
                        return false;
                    missingIngredients.Add(ingredient);
                }
            }
            return missingIngredients == null || missingIngredients.Count == 0;
        }

        private static bool TryFindBestBillIngredientsInSet_AllowMix(
          List<Thing> availableThings,
          Bill bill,
          List<ThingCount> chosen,
          IntVec3 rootCell,
          List<IngredientCount> missingIngredients)
        {
            chosen.Clear();
            missingIngredients?.Clear();
            availableThings.SortBy<Thing, float, int>((Func<Thing, float>)(t => bill.recipe.IngredientValueGetter.ValuePerUnitOf(t.def)), (Func<Thing, int>)(t => (t.Position - rootCell).LengthHorizontalSquared));
            for (int index1 = 0; index1 < bill.recipe.ingredients.Count; ++index1)
            {
                IngredientCount ingredient = bill.recipe.ingredients[index1];
                float baseCount = ingredient.GetBaseCount();
                for (int index2 = 0; index2 < availableThings.Count; ++index2)
                {
                    Thing availableThing = availableThings[index2];
                    if (ingredient.filter.Allows(availableThing) && (ingredient.IsFixedIngredient || bill.ingredientFilter.Allows(availableThing)))
                    {
                        float num = bill.recipe.IngredientValueGetter.ValuePerUnitOf(availableThing.def);
                        int countToAdd = Mathf.Min(Mathf.CeilToInt(baseCount / num), availableThing.stackCount);
                        ThingCountUtility.AddToList(chosen, availableThing, countToAdd);
                        baseCount -= (float)countToAdd * num;
                        if ((double)baseCount <= 9.9999997473787516E-05)
                            break;
                    }
                }
                if ((double)baseCount > 9.9999997473787516E-05)
                {
                    if (missingIngredients == null)
                        return false;
                    missingIngredients.Add(ingredient);
                }
            }
            return missingIngredients == null || missingIngredients.Count == 0;
        }

        private class DefCountList
        {
            private List<ThingDef> defs = new List<ThingDef>();
            private List<float> counts = new List<float>();

            public int Count => this.defs.Count;

            public float this[ThingDef def]
            {
                get
                {
                    int index = this.defs.IndexOf(def);
                    return index < 0 ? 0.0f : this.counts[index];
                }
                set
                {
                    int index = this.defs.IndexOf(def);
                    if (index < 0)
                    {
                        this.defs.Add(def);
                        this.counts.Add(value);
                        index = this.defs.Count - 1;
                    }
                    else
                        this.counts[index] = value;
                    this.CheckRemove(index);
                }
            }

            public float GetCount(int index) => this.counts[index];

            public void SetCount(int index, float val)
            {
                this.counts[index] = val;
                this.CheckRemove(index);
            }

            public ThingDef GetDef(int index) => this.defs[index];

            private void CheckRemove(int index)
            {
                if ((double)this.counts[index] != 0.0)
                    return;
                this.counts.RemoveAt(index);
                this.defs.RemoveAt(index);
            }

            public void Clear()
            {
                this.defs.Clear();
                this.counts.Clear();
            }

            public void GenerateFrom(List<Thing> things)
            {
                this.Clear();
                for (int index = 0; index < things.Count; ++index)
                    this[things[index].def] += (float)things[index].stackCount;
            }

            
        }
    }
}
