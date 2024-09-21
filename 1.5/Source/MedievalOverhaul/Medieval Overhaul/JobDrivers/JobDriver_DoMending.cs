using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using System;
using UnityEngine;
using System.Linq;
using System.Diagnostics.Eventing.Reader;
using UnityEngine.UIElements;

namespace MedievalOverhaul
{
    public class JobDriver_DoMending : JobDriver
    {
        public float workLeft;

        public int billStartTick;

        public int ticksSpentDoingRecipeWork;
        public override string GetReport()
        {
            if (this.job.RecipeDef != null)
            {
                return this.ReportStringProcessed(this.job.RecipeDef.jobString);
            }
            return base.GetReport();
        }
        public IBillGiver BillGiver
        {
            get
            {
                if (MendingBench is not IBillGiver billGiver)
                {
                    throw new InvalidOperationException("DoBill on non-Billgiver.");
                }
                return billGiver;
            }
        }
        protected Thing MendingBench
        {
            get
            {
                return this.job.GetTarget(TargetIndex.A).Thing;
            }
        }
        protected Thing RepairThing
        {
            get
            {
                return this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
            Scribe_Values.Look<int>(ref this.billStartTick, "billStartTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksSpentDoingRecipeWork, "ticksSpentDoingRecipeWork", 0, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
            if (!this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed, false))
            {
                return false;
            }
            if (thing != null && thing.def.hasInteractionCell && !this.pawn.ReserveSittableOrSpot(thing.InteractionCell, this.job, errorOnFailed))
            {
                return false;
            }
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            base.AddEndCondition(delegate
            {
                Thing thing = base.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (thing is Building && !thing.Spawned)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(delegate ()
            {
                if (this.job.GetTarget(TargetIndex.A).Thing is IBillGiver billGiver)
                {
                    if (this.job.bill.DeletedOrDereferenced)
                    {
                        return true;
                    }
                    if (!billGiver.CurrentlyUsableForBills())
                    {
                        return true;
                    }
                }
                return false;
            });
            Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell, false);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate ()
            {
                if (this.job.targetQueueB != null && this.job.targetQueueB.Count == 1)
                {
                    if (this.job.targetQueueB[0].Thing is UnfinishedThing unfinishedThing)
                    {
                        unfinishedThing.BoundBill = (Bill_ProductionWithUft)this.job.bill;
                    }
                }
                this.job.bill.Notify_DoBillStarted(this.pawn);
            };
            yield return toil;
            yield return Toils_Jump.JumpIf(gotoBillGiver, () => this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
            foreach (Toil toil2 in JobDriver_DoBill.CollectIngredientsToils(TargetIndex.B, TargetIndex.A, TargetIndex.C, false, true, this.BillGiver is Building_WorkTableAutonomous))
            {
                yield return toil2;
            }
            yield return gotoBillGiver;
            yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
            yield return DoRecipeWork_Mend().FailOnDespawnedNullOrForbiddenPlacedThings(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_Recipe.CheckIfRecipeCanFinishNow();
            yield return FinishRecipeAndStartStoringProduct_Mend(TargetIndex.None);
            yield break;
        }

        public static IEnumerable<Toil> CollectIngredientsToils(
      TargetIndex ingredientInd,
      TargetIndex billGiverInd,
      TargetIndex ingredientPlaceCellInd,
      bool subtractNumTakenFromJobCount = false,
      bool failIfStackCountLessThanJobCount = true,
      bool placeInBillGiver = false)
        {
            Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientInd);
            yield return extract;
            Toil jumpIfHaveTargetInQueue = Toils_Jump.JumpIfHaveTargetInQueue(ingredientInd, extract);
            yield return JumpIfTargetInsideBillGiver(jumpIfHaveTargetInQueue, ingredientInd, billGiverInd);
            Toil getToHaulTarget = Toils_Goto.GotoThing(ingredientInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden<Toil>(ingredientInd).FailOnSomeonePhysicallyInteracting<Toil>(ingredientInd);
            yield return getToHaulTarget;
            yield return Toils_Haul.StartCarryThing(ingredientInd, true, subtractNumTakenFromJobCount, failIfStackCountLessThanJobCount, false);
            yield return JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
            yield return Toils_Goto.GotoThing(billGiverInd, PathEndMode.InteractionCell).FailOnDestroyedOrNull<Toil>(ingredientInd);
            if (!placeInBillGiver)
            {
                Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(billGiverInd, ingredientInd, ingredientPlaceCellInd);
                yield return findPlaceTarget;
                yield return Toils_Haul.PlaceHauledThingInCell(ingredientPlaceCellInd, findPlaceTarget, false);
                Toil physReserveToil = ToilMaker.MakeToil(nameof(CollectIngredientsToils));
                physReserveToil.initAction = (Action)(() => physReserveToil.actor.Map.physicalInteractionReservationManager.Reserve(physReserveToil.actor, physReserveToil.actor.CurJob, physReserveToil.actor.CurJob.GetTarget(ingredientInd)));
                yield return physReserveToil;
                findPlaceTarget = (Toil)null;
            }
            else
                yield return Toils_Haul.DepositHauledThingInContainer(billGiverInd, ingredientInd);
            yield return jumpIfHaveTargetInQueue;
        }

        private static Toil JumpIfTargetInsideBillGiver(Toil jumpToil, TargetIndex ingredient, TargetIndex billGiver)
        {
            Toil toil = ToilMaker.MakeToil("JumpIfTargetInsideBillGiver");
            toil.initAction = delegate ()
            {
                Thing thing = toil.actor.CurJob.GetTarget(billGiver).Thing;
                if (thing == null || !thing.Spawned)
                {
                    return;
                }
                Thing thing2 = toil.actor.jobs.curJob.GetTarget(ingredient).Thing;
                if (thing2 == null)
                {
                    return;
                }
                ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
                if (thingOwner != null && thingOwner.Contains(thing2))
                {
                    HaulAIUtility.UpdateJobWithPlacedThings(toil.actor.jobs.curJob, thing2, thing2.stackCount);
                    toil.actor.jobs.curDriver.JumpToToil(jumpToil);
                }
            };
            return toil;
        }

        public static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            Toil toil = ToilMaker.MakeToil("JumpToCollectNextIntoHandsForBill");
            toil.initAction = delegate ()
            {
                Pawn actor = toil.actor;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
                    return;
                }
                if (actor.carryTracker.Full)
                {
                    return;
                }
                Job curJob = actor.jobs.curJob;
                List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
                if (targetQueue.NullOrEmpty<LocalTargetInfo>())
                {
                    return;
                }
                for (int i = 0; i < targetQueue.Count; i++)
                {
                    if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing) && targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing) && (float)(actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared <= 64f)
                    {
                        int num = (actor.carryTracker.CarriedThing == null) ? 0 : actor.carryTracker.CarriedThing.stackCount;
                        int num2 = curJob.countQueue[i];
                        num2 = Mathf.Min(num2, targetQueue[i].Thing.def.stackLimit - num);
                        num2 = Mathf.Min(num2, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
                        if (num2 > 0)
                        {
                            curJob.count = num2;
                            curJob.SetTarget(ind, targetQueue[i].Thing);
                            List<int> countQueue = curJob.countQueue;
                            int index = i;
                            countQueue[index] -= num2;
                            if (curJob.countQueue[i] <= 0)
                            {
                                curJob.countQueue.RemoveAt(i);
                                targetQueue.RemoveAt(i);
                            }
                            actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                            return;
                        }
                    }
                }
            };
            return toil;
        }

        public static Toil DoRecipeWork_Mend()
        {
            Toil toil = ToilMaker.MakeToil("DoRecipeWork_Mend");
            toil.initAction = delegate ()
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoMending jobDriver_DoBill = (JobDriver_DoMending)actor.jobs.curDriver;
                Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
                UnfinishedThing unfinishedThing = thing as UnfinishedThing;
                BuildingProperties building = curJob.GetTarget(TargetIndex.A).Thing.def.building;
                if (unfinishedThing != null && unfinishedThing.Initialized)
                {
                    jobDriver_DoBill.workLeft = unfinishedThing.workLeft;
                }
                else
                {
                    jobDriver_DoBill.workLeft = curJob.bill.GetWorkAmount(thing) * (thing.MaxHitPoints - thing.HitPoints);
                    if (unfinishedThing != null)
                    {
                        if (unfinishedThing.debugCompleted)
                        {
                            unfinishedThing.workLeft = (jobDriver_DoBill.workLeft = 0f);
                        }
                        else
                        {
                            unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
                        }
                    }
                }
                jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
                jobDriver_DoBill.ticksSpentDoingRecipeWork = 0;
                curJob.bill.Notify_BillWorkStarted(actor);
            };
            toil.tickAction = delegate ()
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoMending jobDriver_DoBill = (JobDriver_DoMending)actor.jobs.curDriver;
                UnfinishedThing unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing != null && unfinishedThing.Destroyed)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                    return;
                }
                jobDriver_DoBill.ticksSpentDoingRecipeWork++;
                curJob.bill.Notify_PawnDidWork(actor);
                if (toil.actor.CurJob.GetTarget(TargetIndex.A).Thing is IBillGiverWithTickAction billGiverWithTickAction)
                {
                    billGiverWithTickAction.UsedThisTick();
                }
                if (curJob.RecipeDef.workSkill != null && curJob.RecipeDef.UsesUnfinishedThing && actor.skills != null)
                {
                    actor.skills.Learn(curJob.RecipeDef.workSkill, 0.1f * curJob.RecipeDef.workSkillLearnFactor, false, false);
                }
                float num = (curJob.RecipeDef.workSpeedStat == null) ? 1f : actor.GetStatValue(curJob.RecipeDef.workSpeedStat, true, -1);
                if (curJob.RecipeDef.workTableSpeedStat != null)
                {
                    if (jobDriver_DoBill.BillGiver is Building_WorkTable building_WorkTable)
                    {
                        num *= building_WorkTable.GetStatValue(curJob.RecipeDef.workTableSpeedStat, true, -1);
                    }
                }
                if (DebugSettings.fastCrafting)
                {
                    num *= 30f;
                }
                jobDriver_DoBill.workLeft -= num;
                if (unfinishedThing != null)
                {
                    if (unfinishedThing.debugCompleted)
                    {
                        unfinishedThing.workLeft = (jobDriver_DoBill.workLeft = 0f);
                    }
                    else
                    {
                        unfinishedThing.workLeft = jobDriver_DoBill.workLeft;
                    }
                }
                actor.GainComfortFromCellIfPossible(true);
                if (jobDriver_DoBill.workLeft <= 0f)
                {
                    curJob.bill.Notify_BillWorkFinished(actor);
                    jobDriver_DoBill.ReadyForNextToil();
                    return;
                }
                if (curJob.bill.recipe.UsesUnfinishedThing)
                {
                    int num2 = Find.TickManager.TicksGame - jobDriver_DoBill.billStartTick;
                    if (num2 >= 3000 && num2 % 1000 == 0)
                    {
                        actor.jobs.CheckForJobOverride(0f);
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A, null);
            toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking, 1f);
            toil.WithProgressBar(TargetIndex.A, delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.CurJob;
                Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
                float workLeft = ((JobDriver_DoMending)actor.jobs.curDriver).workLeft;
                Bill_Mech bill_Mech;
                float num = ((bill_Mech = (curJob.bill as Bill_Mech)) != null && bill_Mech.State == FormingState.Formed) ? 300f : curJob.bill.recipe.WorkAmountTotal(thing)*(thing.MaxHitPoints - thing.HitPoints);
                return 1f - workLeft / num;
            }, false, -0.5f, false);
            toil.FailOn(delegate ()
            {
                RecipeDef recipeDef = toil.actor.CurJob.RecipeDef;
                if (recipeDef != null && recipeDef.interruptIfIngredientIsRotting)
                {
                    LocalTargetInfo target = toil.actor.CurJob.GetTarget(TargetIndex.B);
                    if (target.HasThing && target.Thing.GetRotStage() > RotStage.Fresh)
                    {
                        return true;
                    }
                }
                return toil.actor.CurJob.bill.suspended;
            });
            toil.activeSkill = (() => toil.actor.CurJob.bill.recipe.workSkill);
            return toil;
        }

        public static Toil FinishRecipeAndStartStoringProduct_Mend(TargetIndex productIndex = TargetIndex.A)
        {
            Toil toil = ToilMaker.MakeToil("FinishRecipeAndStartStoringProduct_Mend");
            toil.AddFinishAction(delegate
            {
                Bill_Production bill_Production;
                if ((bill_Production = (toil.actor.jobs.curJob.bill as Bill_Production)) != null && bill_Production.repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    toil.actor.Map.resourceCounter.UpdateResourceCounts();
                }
            });
            toil.initAction = delegate ()
            {
                
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thingMend = curJob.GetTarget(TargetIndex.B).Thing;
                int hpHeal = thingMend.MaxHitPoints - thingMend.HitPoints;
                thingMend.HitPoints += hpHeal;
                JobDriver_DoMending jobDriver_DoBill = (JobDriver_DoMending)actor.jobs.curDriver;
                if (curJob.RecipeDef.workSkill != null && !curJob.RecipeDef.UsesUnfinishedThing && actor.skills != null)
                {
                    float xp = (float)jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.1f * curJob.RecipeDef.workSkillLearnFactor;
                    actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp, false, false);
                }
                List<Thing> ingredients = CalculateIngredients(curJob, actor);
                Thing dominantIngredient = CalculateDominantIngredient(curJob, ingredients);
                curJob.bill.Notify_IterationCompleted(actor, ingredients);
                RecordsUtility.Notify_BillDone(actor, ingredients);
                actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                if ((curJob?.bill) == null)
                {
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        if (!GenPlace.TryPlaceThing(ingredients[i], actor.Position, actor.Map, ThingPlaceMode.Near, null, null, default))
                        {
                            Log.Error(string.Concat(new object[]
                            {
                                actor,
                                " could not drop recipe product ",
                                ingredients[i],
                                " near ",
                                actor.Position
                            }));
                        }
                    }
                    return;
                }
               
                if (curJob.bill.recipe.WorkAmountTotal(thingMend) >= 10000f && ingredients.Count > 0)
                {
                    TaleRecorder.RecordTale(TaleDefOf.CompletedLongCraftingProject,
                    [
                        actor,
                        ingredients[0].GetInnerIfMinified().def
                    ]);
                }
                IntVec3 invalid = IntVec3.Invalid;
                if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.BestStockpile)
                {
                    StoreUtility.TryFindBestBetterStoreCellFor(ingredients[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, out invalid, true);
                }
                else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.SpecificStockpile)
                {
                    StoreUtility.TryFindBestBetterStoreCellForIn(ingredients[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetSlotGroup(), out invalid, true);
                }
                else
                {
                    Log.ErrorOnce("Unknown store mode", 9158246);
                }
                if (!invalid.IsValid)
                {
                    if (!GenPlace.TryPlaceThing(ingredients[0], actor.Position, actor.Map, ThingPlaceMode.Near, null, null, default))
                    {
                        Log.Error(string.Format("Bill doer could not drop product {0} near {1}", ingredients[0], actor.Position));
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                    return;
                }
                int num = actor.carryTracker.MaxStackSpaceEver(ingredients[0].def);
                if (num < ingredients[0].stackCount)
                {
                    int count = ingredients[0].stackCount - num;
                    Thing thing2 = ingredients[0].SplitOff(count);
                    if (!GenPlace.TryPlaceThing(thing2, actor.Position, actor.Map, ThingPlaceMode.Near, null, null, default))
                    {
                        Log.Error(string.Format("{0} could not drop recipe extra product that pawn couldn't carry, {1} near {2}", actor, thing2, actor.Position));
                    }
                }
                if (num == 0)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                    return;
                }
                actor.carryTracker.TryStartCarry(ingredients[0]);
                actor.jobs.StartJob(HaulAIUtility.HaulToCellStorageJob(actor, ingredients[0], invalid, false), JobCondition.Succeeded, null, false, true, null, null, false, false, new bool?(true), false, true, false);
            };
            return toil;
        }
        private static List<Thing> CalculateIngredients(Job job, Pawn actor)
        {
            if (job.GetTarget(TargetIndex.B).Thing is UnfinishedThing unfinishedThing)
            {
                List<Thing> ingredients = unfinishedThing.ingredients;
                job.RecipeDef.Worker.ConsumeIngredient(unfinishedThing, job.RecipeDef, actor.Map);
                job.placedThings = null;
                return ingredients;
            }
            List<Thing> list = [];
            if (job.placedThings != null)
            {
                for (int i = 0; i < job.placedThings.Count; i++)
                {
                    if (job.placedThings[i].Count <= 0)
                    {
                        Log.Error(string.Concat(new object[]
                        {
                            "PlacedThing ",
                            job.placedThings[i],
                            " with count ",
                            job.placedThings[i].Count,
                            " for job ",
                            job
                        }));
                    }
                    else
                    {
                        Thing thing;
                        if (job.placedThings[i].Count < job.placedThings[i].thing.stackCount)
                        {
                            thing = job.placedThings[i].thing.SplitOff(job.placedThings[i].Count);
                        }
                        else
                        {
                            thing = job.placedThings[i].thing;
                        }
                        job.placedThings[i].Count = 0;
                        if (list.Contains(thing))
                        {
                            Log.Error("Tried to add ingredient from job placed targets twice: " + thing);
                        }
                        else
                        {
                            list.Add(thing);
                            if (job.RecipeDef.autoStripCorpses)
                            {
                                if (thing is IStrippable strippable && strippable.AnythingToStrip())
                                {
                                    strippable.Strip(true);
                                }
                            }
                        }
                    }
                }
            }
            job.placedThings = null;
            return list;
        }
        private static Thing CalculateDominantIngredient(Job job, List<Thing> ingredients)
        {
            if (job.GetTarget(TargetIndex.B).Thing is UnfinishedThing uft && uft.def.MadeFromStuff)
            {
                return uft.ingredients.First((Thing ing) => ing.def == uft.Stuff);
            }
            if (ingredients.NullOrEmpty<Thing>())
            {
                return null;
            }
            RecipeDef recipeDef = job.RecipeDef;
            if (recipeDef.productHasIngredientStuff)
            {
                return ingredients[0];
            }
            if (recipeDef.products.Any((ThingDefCountClass x) => x.thingDef.MadeFromStuff) || (recipeDef.unfinishedThingDef != null && recipeDef.unfinishedThingDef.MadeFromStuff))
            {
                return (from x in ingredients
                        where x.def.IsStuff
                        select x).RandomElementByWeight((Thing x) => (float)x.stackCount);
            }
            return ingredients.RandomElementByWeight((Thing x) => (float)x.stackCount);
        }

    }
}