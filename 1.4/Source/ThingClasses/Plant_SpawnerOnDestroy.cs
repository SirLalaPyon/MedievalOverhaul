using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class Plant_SpawnerOnDestroy : Plant
    {
        /// <summary>
        /// Simple override
        /// All functionality is still handled in Comp_PawnSpawnerOnDestroy
        /// </summary>
        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {
            Comp_PawnSpawnerOnDestroy compDestroy = GetComp<Comp_PawnSpawnerOnDestroy>();
            if (compDestroy != null)
            {
                if (!compDestroy.Props.onlyIfHarvestable || (compDestroy.Props.onlyIfHarvestable && HarvestableNow))
                {
                    compDestroy.PawnSpawnerWorker(by.Map);
                }
            }
            base.PlantCollected(by, plantDestructionMode);
        }
    }
}
