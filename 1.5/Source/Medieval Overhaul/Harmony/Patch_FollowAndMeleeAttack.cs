using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace DankPyon
{
    [HarmonyPatch(typeof(Toils_Combat), "FollowAndMeleeAttack", new Type[] { typeof(TargetIndex), typeof(TargetIndex), typeof(Action) })]
    public static class Patch_FollowAndMeleeAttack
    {
        private static bool Prefix(ref Toil __result, TargetIndex targetInd, TargetIndex standPositionInd, Action hitAction)
        {
            if (MedievalOverhaulMod.settings.enableMultiTileMeleeAttacks)
            {
                __result = FollowAndMeleeAttackModified(targetInd, standPositionInd, hitAction);
                return false;
            }
            return true;
        }

        public static Toil FollowAndMeleeAttackModified(TargetIndex targetInd, TargetIndex standPositionInd, Action hitAction)
        {
            Toil followAndAttack = new Toil();
            followAndAttack.tickAction = delegate
            {
                Pawn actor = followAndAttack.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver curDriver = actor.jobs.curDriver;
                LocalTargetInfo target = curJob.GetTarget(targetInd);
                Thing thing = target.Thing;
                Pawn pawn = thing as Pawn;
                if (!thing.Spawned || (pawn != null && pawn.IsPsychologicallyInvisible()))
                {
                    curDriver.ReadyForNextToil();
                }
                else
                {
                    var verbToUse = curJob.verbToUse ?? actor.meleeVerbs.TryGetMeleeVerb(thing);
                    var meleeReachRange = actor.GetMeleeReachRange(verbToUse);
                    if (meleeReachRange <= 1.42f)
                    {
                        LocalTargetInfo localTargetInfo = target;
                        PathEndMode peMode = PathEndMode.Touch;
                        if (standPositionInd != 0)
                        {
                            LocalTargetInfo target2 = curJob.GetTarget(standPositionInd);
                            if (target2.IsValid)
                            {
                                localTargetInfo = target2;
                                peMode = PathEndMode.OnCell;
                            }
                        }
                        if (localTargetInfo != actor.pather.Destination || (!actor.pather.Moving && !actor.CanReachImmediate(target, PathEndMode.Touch)))
                        {
                            actor.pather.StartPath(localTargetInfo, peMode);
                        }
                        else if (actor.CanReachImmediate(target, PathEndMode.Touch))
                        {
                            if (pawn != null && pawn.Downed && !curJob.killIncappedTarget)
                            {
                                curDriver.ReadyForNextToil();
                            }
                            else
                            {
                                hitAction();
                            }
                        }
                    }
                    else
                    {
                        if (actor.Position.DistanceTo(thing.Position) > meleeReachRange)
                        {
                            CastPositionRequest newReq = default(CastPositionRequest);
                            newReq.caster = followAndAttack.actor;
                            newReq.target = thing;
                            newReq.verb = verbToUse;
                            newReq.maxRangeFromTarget = meleeReachRange;
                            newReq.wantCoverFromTarget = false;
                            verbToUse.verbProps.range = meleeReachRange;
                            if (!CastPositionFinder.TryFindCastPosition(newReq, out var dest))
                            {
                                followAndAttack.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                            }
                            else
                            {
                                followAndAttack.actor.pather.StartPath(dest, PathEndMode.OnCell);
                                actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, dest);
                            }
                        }
                        else
                        {
                            if (pawn != null && pawn.Downed && !curJob.killIncappedTarget)
                            {
                                curDriver.ReadyForNextToil();
                            }
                            else
                            {
                                hitAction();
                            }
                        }
                    }

                }
            };
            followAndAttack.activeSkill = () => SkillDefOf.Melee;
            followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
            return followAndAttack;
        }
    }
}
