using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;

namespace MedievalOverhaul
{
    public class DeathActionWorker_DropItems : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            var deathDropOptions = corpse.InnerPawn.def.GetModExtension<DeathDrop_Extension>();
            
            if (deathDropOptions != null)
            {
                foreach (var option in deathDropOptions.deathDropOptions)
                {
                    if (Rand.Chance(option.chance))
                    {
                        ThingDef thingDef = option.thingDefs.RandomElement();
                        IntVec3 intVec3 = CellFinder.RandomClosewalkCellNear(corpse.Position, corpse.Map, 3);
                        Thing product = ThingMaker.MakeThing(thingDef, null);
                        product.stackCount = option.amount.RandomInRange;
                        GenSpawn.Spawn(product, intVec3, corpse.Map, Rot4.Random, WipeMode.Vanish, false);
                    }
                }
            }
        }
    }
}
