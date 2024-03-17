using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Verse;

namespace MedievalOverhaul
{
    public class Plant_SecondaryDrop : Plant
    {
        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {
            if (HarvestableNow)
            {
                SecondaryPlantDropExtension props = SecondaryPlantDropExtension.Get(def);
                if (props != null && props.secondaryDrop != null)
                {
                    if (!props.secondaryNotWhenLeafless || !LeaflessNow)
                    {
                        if (Rand.Chance(props.secondaryDropChance))
                        {
                            Thing droppedThing = ThingMaker.MakeThing(props.secondaryDrop);
                            droppedThing.stackCount = props.secondaryDropAmountRange.RandomInRange;
                            GenPlace.TryPlaceThing(droppedThing, Position, Map, ThingPlaceMode.Near);
                        }
                    }
                }
            }
            base.PlantCollected(by, plantDestructionMode);
        }
    }
}
