//using RimWorld;
//using Verse;
//using System.Collections.Generic;

//namespace MedievalOverhaul
//{
//    ///// <summary>
//    ///// Updates the thing classes of all the great trees on the map
//    ///// Only runs when the map is generated
//    ///// Would be a good idea to remove this when updating to RimWorld 1.5, as it won't be needed for that
//    ///// </summary>
//    //public class GreatTreesClassUpdate_MapComponent : MapComponent
//    //{
//    //    public GreatTreesClassUpdate_MapComponent(Map map) : base(map)
//    //    {
//    //    }

//    //    public override void FinalizeInit()
//    //    {
//    //        base.FinalizeInit();
//    //        if (!fullyUpdated)
//    //        {
//    //            UpdateTreeThingClasses();
//    //            fullyUpdated = true;
//    //            Log.Message("Fin");
//    //        }
//    //    }

//    //    public void UpdateTreeThingClasses()
//    //    {
//    //        ThingRequest thingRequest = new() {  group = ThingRequestGroup.Plant};
//    //        List<Thing> plantList = map.listerThings.ThingsMatching(thingRequest);
//    //        if (!plantList.NullOrEmpty())
//    //        {
//    //            int count = plantList.Count-1;
//    //            for (int i = count; i >= 0; i--)
//    //            {
//    //                {
//    //                    if (plantList[i].def.defName == "MedievalOverhaul_GreatOak" || plantList[i].def.defName == "MedievalOverhaul_GreatIter" || plantList[i].def.defName == "MedievalOverhaul_GreatWillow")
//    //                    {
//    //                        UpdateTree(map, plantList[i] as Plant);
//    //                    }
//    //                }
//    //            }
//    //        }
           
//    //    }

//    //    public Thing UpdateTree(Map map, Plant oldPlant)
//    //    {
//    //        IntVec3 position = oldPlant.Position;
//    //        Thing newThing = ThingMaker.MakeThing(oldPlant.def);
//    //        Plant newPlant = newThing as Plant;
//    //        newThing.HitPoints = oldPlant.HitPoints;
//    //        newPlant.Growth = oldPlant.Growth;
//    //        oldPlant.Destroy();
//    //        GenSpawn.Spawn(newPlant, position, map);
//    //        return newPlant;
//    //    }

//    //    public override void ExposeData()
//    //    {
//    //        base.ExposeData();
//    //        Scribe_Values.Look(ref fullyUpdated, "MO_GreatTreesClassesFullyUpdated", false);
//    //    }

//    //    public bool fullyUpdated = false;
//    }
//}
