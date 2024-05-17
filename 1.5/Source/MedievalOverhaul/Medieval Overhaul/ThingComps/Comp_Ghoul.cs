using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Verse;
using Verse.Sound;

namespace MedievalOverhaul
{
    public class Comp_Ghoul : ThingComp
    {
        public bool ghoulFlag = false;
        public CompProperties_Ghoul Props
        {
            get
            {
                return (CompProperties_Ghoul)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (ghoulFlag)
            {
                Pawn pawn = this.parent as Pawn;
                for (int i = 0; i < Props.ghoulTicks; ++i)
                {
                    if (Props.ghoulTicks <= (i + 1))
                    {
                        Faction faction = this.parent.Faction;
                        PawnGenerationRequest pawnRequest = new PawnGenerationRequest(PawnKindDef.Named(Props.pawnKindDef), faction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, false, false);
                        Pawn pawnGen = PawnGenerator.GeneratePawn(pawnRequest);
                        pawnGen.needs.food.CurLevel = Props.postChangeLevel;
                        pawnGen.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, false, null, false, false);
                        GenSpawn.Spawn(pawnGen, CellFinder.RandomClosewalkCellNear(this.parent.Position, this.parent.Map, 3, null), this.parent.Map, WipeMode.Vanish);
                        for (int j = 0; j < 20; j++)
                        {
                            IntVec3 filthPosition;
                            CellFinder.TryFindRandomReachableNearbyCell(this.parent.Position, this.parent.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out filthPosition);
                            FilthMaker.TryMakeFilth(filthPosition, this.parent.Map, ThingDefOf.Filth_AmnioticFluid);
                        }
                        if (this.Props.spawnSound != null)
                            this.Props.spawnSound.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));

                        this.parent.Destroy();
                    }
                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            Pawn pawn = this.parent as Pawn;
            if (parent.Map != null && pawn.needs.food.CurLevelPercentage >= Props.foodLevelChange)
            {
                ghoulFlag = true;
            }
        }
    }
}