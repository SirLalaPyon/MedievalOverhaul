using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DankPyon
{
    [DefOf]
    public static class DankPyonDefOf
    {
        public static ShaderTypeDef TransparentPlant;
    }
    [StaticConstructorOnStartup]
    public static class HarmonyInstance
    {
        static HarmonyInstance()
        {
            var harmony = new Harmony("lalapyhon.rimworld.medievaloverhaul");
            harmony.Patch(AccessTools.Method(typeof(Thing), "ButcherProducts", null, null), null, 
                new HarmonyMethod(typeof(HarmonyInstance), "Thing_MakeButcherProducts_FatAndBone_PostFix", null), null);
            harmony.PatchAll();
        }

        public static List<IntVec3> GetTransparentCheckArea(this Thing plant, PlantExtension extension)
        {
            var cellRect = new CellRect(plant.Position.x, plant.Position.z, extension.firstArea.x, extension.firstArea.z);
            if (extension.firstAreaOffset != IntVec2.Zero)
            {
                cellRect = cellRect.MovedBy(extension.firstAreaOffset);
            }
            var cells = cellRect.Cells.ToList();
            if (extension.secondArea != IntVec2.Zero)
            {
                var cells2 = new CellRect(plant.Position.x, plant.Position.z, extension.secondArea.x, extension.secondArea.z);
                if (extension.secondAreaOffset != IntVec2.Zero)
                {
                    cells2 = cells2.MovedBy(extension.secondAreaOffset);
                }
                cells.AddRange(cells2);
            }

            return cells.Where(x => x.InBounds(plant.Map)).ToList();
        }

        public static bool HasItemsInCell(IntVec3 cell, Map map, PlantExtension extension)
        {
            foreach (var thing in cell.GetThingList(map))
            {
                if (ItemMatches(thing, extension))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ItemMatches(Thing thing, PlantExtension extension)
        {
            return (thing is Pawn || thing.def.category == ThingCategory.Item) && (extension.ignoredThings is null || !extension.ignoredThings.Contains(thing.def));
        }

        [HarmonyPatch(typeof(Thing), "Position", MethodType.Setter)]
        public class Thing_Position_Postfix
        {
            private static void Postfix(Thing __instance)
            {
                if (__instance.Map != null)
                {
                    foreach (var plant in __instance.Map.listerThings.ThingsInGroup(ThingRequestGroup.Plant))
                    {
                        var extension = plant.def.GetModExtension<PlantExtension>();
                        if (extension != null)
                        {
                            if (extension.transparencyWhenPawnOrItemIsBehind)
                            {
                                bool isTransparent = plant.Graphic.Shader == DankPyonDefOf.TransparentPlant.Shader;
                                if (!isTransparent && ItemMatches(__instance, extension))
                                {
                                    var cells = GetTransparentCheckArea(plant, extension);
                                    if (cells.Contains(__instance.Position))
                                    {
                                        __instance.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
                                        continue;
                                    }
                                }

                                if (isTransparent)
                                {
                                    var cells = GetTransparentCheckArea(plant, extension);
                                    if (!cells.Any(x => HasItemsInCell(x, __instance.Map, extension)))
                                    {
                                        __instance.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Plant), "Graphic", MethodType.Getter)]
        public class Patch_Graphic_Postfix
        {
            private static void Postfix(Plant __instance, ref Graphic __result)
            {
                var extension = __instance.def.GetModExtension<PlantExtension>();
                if (extension != null)
                {
                    if (extension.transparencyWhenPawnOrItemIsBehind)
                    {
                        var cells = GetTransparentCheckArea(__instance, extension);
                        bool anyItemsExistsInArea = cells.Any(x => HasItemsInCell(x, __instance.Map, extension));
                        if (anyItemsExistsInArea)
                        {
                            __result = GraphicDatabase.Get(__result.GetType(), "Transparent/" + __result.path, DankPyonDefOf.TransparentPlant.Shader,
                                __instance.def.graphicData.drawSize, __result.color, __result.colorTwo);
                            __instance.Map.mapDrawer.MapMeshDirty(__instance.Position, MapMeshFlag.Things);
                        }
                        else
                        {
                            __result = GraphicDatabase.Get(__result.GetType(), __instance.def.graphicData.texPath, __instance.def.graphicData.shaderType.Shader,
                                __instance.def.graphicData.drawSize, __result.color, __result.colorTwo);
                            __instance.Map.mapDrawer.MapMeshDirty(__instance.Position, MapMeshFlag.Things);
                        }
                    }
                }
            }
        }

        private static ThingDef fat = ThingDef.Named("DankPyon_Fat");
        private static ThingDef bone = ThingDef.Named("DankPyon_Bone");
        private static void Thing_MakeButcherProducts_FatAndBone_PostFix(Thing __instance, ref IEnumerable<Thing> __result, Pawn butcher, float efficiency)
        {
            if (__instance is Pawn pawn && pawn.RaceProps.IsFlesh && pawn.RaceProps.meatDef != null)
            {
                Thing Bone = ThingMaker.MakeThing(bone, null);
                Thing Fat = ThingMaker.MakeThing(fat, null);
                int amount = Math.Max(1, (int)(GenMath.RoundRandom(__instance.GetStatValue(StatDefOf.MeatAmount, true) * efficiency) * 0.2f));
                Bone.stackCount = amount;
                Fat.stackCount = amount;
                __result = __result.AddItem(Bone);
                __result = __result.AddItem(Fat);
            }
        }
    }
}
