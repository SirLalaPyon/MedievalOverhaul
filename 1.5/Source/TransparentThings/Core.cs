using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TransparentThings
{
    [StaticConstructorOnStartup]
    public static class Core
    {
        public static bool hasTransparentTrees;
        public static bool hasTransparentRoofs;
        public static List<ThingDef> transparentRoofs;
        public static Dictionary<Map, Dictionary<IntVec3, List<Thing>>> cachedTransparentableCellsByMaps = new Dictionary<Map, Dictionary<IntVec3, List<Thing>>>();
        public static Dictionary<Thing, ThingExtension> cachedTransparentableThingsByExtensions = new Dictionary<Thing, ThingExtension>();
        public static Dictionary<Map, List<ThingWithExtension>> cachedTransparentableThingsByMaps = new Dictionary<Map, List<ThingWithExtension>>();
        public static Dictionary<Thing, HashSet<IntVec3>> cachedCells = new Dictionary<Thing, HashSet<IntVec3>>();
        public static Dictionary<Thing, Shader> lastCachedShaders = new Dictionary<Thing, Shader>();
        public static List<Thing> transparentThings = new List<Thing>();
        public static Dictionary<Thing, bool> matchedItems = new Dictionary<Thing, bool>();

        static Core()
        {
            hasTransparentTrees = DefDatabase<ThingDef>.AllDefs.All<ThingDef>((Func<ThingDef, bool>)(x => x.IsPlant && HasTransparencyExtension(x)));
            transparentRoofs = DefDatabase<ThingDef>.AllDefs.Where<ThingDef>((Func<ThingDef, bool>)(x => typeof(RoofSetter).IsAssignableFrom(x.thingClass) && HasTransparencyExtension(x))).ToList<ThingDef>();
            hasTransparentRoofs = transparentRoofs.Any<ThingDef>();
            static bool HasTransparencyExtension(ThingDef x)
            {
                ThingExtension modExtension = x.GetModExtension<ThingExtension>();
                return modExtension != null && (modExtension.transparentWhenPawnIsInsideArea || modExtension.transparentWhenItemIsInsideArea);
            }
        }

        public static HashSet<IntVec3> GetTransparentCheckArea(
          this Thing thing,
          ThingExtension extension)
        {
            HashSet<IntVec3> transparentCheckAreaInt;
            if (!Core.cachedCells.TryGetValue(thing, out transparentCheckAreaInt))
                Core.cachedCells[thing] = transparentCheckAreaInt = Core.GetTransparentCheckAreaInt(thing, extension);
            return transparentCheckAreaInt;
        }

        public static HashSet<IntVec3> GetTransparentCheckAreaInt(
          Thing thing,
          ThingExtension extension)
        {
            IntVec3 bottomLeft = thing.OccupiedRect().Min; // Probably wrong
            CellRect cellRect = new CellRect(bottomLeft.x, bottomLeft.z, extension.firstArea.x, extension.firstArea.z);
            if (extension.firstAreaOffset != IntVec2.Zero)
                cellRect = cellRect.MovedBy(extension.firstAreaOffset);
            List<IntVec3> list = cellRect.Cells.ToList<IntVec3>();
            if (extension.secondArea != IntVec2.Zero)
            {
                CellRect collection = new CellRect(bottomLeft.x, bottomLeft.z, extension.secondArea.x, extension.secondArea.z);
                if (extension.secondAreaOffset != IntVec2.Zero)
                    collection = collection.MovedBy(extension.secondAreaOffset);
                list.AddRange((IEnumerable<IntVec3>)collection);
            }
            return list.Where<IntVec3>((Func<IntVec3, bool>)(x => x.InBounds(thing.Map))).ToHashSet<IntVec3>();
        }

        public static bool HasItemsInCell(IntVec3 cell, Map map, ThingExtension extension)
        {
            foreach (Thing thing in cell.GetThingList(map))
            {
                if (Core.ItemMatches(thing, extension))
                    return true;
            }
            return false;
        }

        public static bool BaseItemMatches(Thing thing) => thing is Pawn || thing.def.category == ThingCategory.Item;

        public static bool ItemMatches(Thing thing, ThingExtension extension)
        {
            bool flag;
            if (!Core.matchedItems.TryGetValue(thing, out flag))
                Core.matchedItems[thing] = flag = (thing is Pawn && extension.transparentWhenPawnIsInsideArea || thing.def.category == ThingCategory.Item && extension.transparentWhenItemIsInsideArea) && (extension.ignoredThings == null || !extension.ignoredThings.Contains(thing.def));
            return flag;
        }

        public static void RecheckTransparency(Thing thing, Thing otherThing, ThingExtension extension)
        {
            if (thing == otherThing || !thing.Spawned || thing.Map != otherThing.Map)
                return;
            Shader shader;
            if (!Core.lastCachedShaders.TryGetValue(thing, out shader))
                Core.lastCachedShaders[thing] = shader = thing.Graphic.Shader;
            bool flag = (UnityEngine.Object)shader == (UnityEngine.Object)thing.GetTransparencyShader();
            if (!flag && Core.ItemMatches(otherThing, extension) && thing.GetTransparentCheckArea(extension).Contains(otherThing.Position))
            {
                thing.Map.mapDrawer.MapMeshDirty(thing.Position,1);
                if (!Core.transparentThings.Contains(thing))
                    Core.transparentThings.Add(thing);
            }
            if (flag)
            {
                HashSet<IntVec3> transparentCheckArea = thing.GetTransparentCheckArea(extension);
                if (!transparentCheckArea.Contains(otherThing.Position) && !transparentCheckArea.Any<IntVec3>((Func<IntVec3, bool>)(x => Core.HasItemsInCell(x, thing.Map, extension))))
                {
                    thing.Map.mapDrawer.MapMeshDirty(thing.Position, 1);
                    Core.transparentThings.Remove(thing);
                }
            }
        }

        public static Shader GetTransparencyShader(this Thing thing) => thing is Plant ? TransparentThingsDefOf.TransparentPlant.Shader : TransparentThingsDefOf.TransparentPostLight.Shader;

        public static void FormTransparencyGridIn(Map map)
        {
            List<ThingWithExtension> thingWithExtensionList;
            if (!Core.cachedTransparentableThingsByMaps.TryGetValue(map, out thingWithExtensionList))
                return;
            Dictionary<IntVec3, List<Thing>> dictionary;
            if (Core.cachedTransparentableCellsByMaps.TryGetValue(map, out dictionary))
                dictionary.Clear();
            else
                Core.cachedTransparentableCellsByMaps[map] = dictionary = new Dictionary<IntVec3, List<Thing>>();
            foreach (ThingWithExtension thingWithExtension in thingWithExtensionList)
            {
                foreach (IntVec3 key in thingWithExtension.thing.GetTransparentCheckArea(thingWithExtension.extension))
                {
                    if (dictionary.ContainsKey(key))
                        dictionary[key].Add(thingWithExtension.thing);
                    else
                        dictionary[key] = new List<Thing>()
            {
              thingWithExtension.thing
            };
                }
            }
        }

        public static bool TransparencyAllowed(this Thing thing)
        {
            switch (thing)
            {
                case Plant _:
                    return TransparentThingsMod.settings.enableTreeTransparency;
                case RoofSetter _:
                    return TransparentThingsMod.settings.enableRoofTransparency;
                default:
                    return false;
            }
        }

        [HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")]
        public class SavedGameLoaderNow_LoadGameFromSaveFileNow
        {
            public static void Prefix()
            {
                Core.cachedTransparentableThingsByExtensions.Clear();
                Core.cachedTransparentableThingsByMaps.Clear();
            }
        }

        [HarmonyPatch(typeof(Thing), "SpawnSetup")]
        public class Thing_SpawnSetup_Patch
        {
            private static void Postfix(Thing __instance)
            {
                if (!__instance.TransparencyAllowed())
                    return;
                ThingExtension modExtension = __instance.def.GetModExtension<ThingExtension>();
                if (modExtension != null && (modExtension.transparentWhenItemIsInsideArea || modExtension.transparentWhenPawnIsInsideArea))
                {
                    Map map = __instance.Map;
                    Core.cachedTransparentableThingsByExtensions[__instance] = modExtension;
                    List<ThingWithExtension> list;
                    if (!Core.cachedTransparentableThingsByMaps.TryGetValue(map, out list))
                        Core.cachedTransparentableThingsByMaps[__instance.Map] = list = new List<ThingWithExtension>();
                    if (!list.Any<ThingWithExtension>((Predicate<ThingWithExtension>)(x => x.thing == __instance)))
                        list.Add(new ThingWithExtension()
                        {
                            thing = __instance,
                            extension = __instance.def.GetModExtension<ThingExtension>()
                        });
                    Core.FormTransparencyGridIn(map);
                }
            }
        }

        [HarmonyPatch(typeof(Thing), "DeSpawn")]
        public class Thing_DeSpawn_Patch
        {
            private static void Prefix(Thing __instance)
            {
                Core.cachedTransparentableThingsByExtensions.Remove(__instance);
                Core.cachedCells.Remove(__instance);
                Map map = __instance.Map;
                List<ThingWithExtension> thingWithExtensionList;
                if (map == null || !__instance.TransparencyAllowed() || !Core.cachedTransparentableThingsByMaps.TryGetValue(map, out thingWithExtensionList) || thingWithExtensionList.RemoveAll((Predicate<ThingWithExtension>)(x => x.thing == __instance)) <= 0)
                    return;
                Core.FormTransparencyGridIn(map);
            }
        }

        [HarmonyPatch(typeof(Thing), "Position", MethodType.Setter)]
        public class Thing_Position_Patch
        {
            public static void Prefix(Thing __instance, out bool __state, IntVec3 value)
            {
                if (Core.BaseItemMatches(__instance) && __instance.Map != null && __instance.Position != value)
                    __state = true;
                else
                    __state = false;
            }

            public static void Postfix(Thing __instance, bool __state)
            {
                if (!__state)
                    return;
                Dictionary<IntVec3, List<Thing>> dictionary;
                List<Thing> thingList;
                if (Core.cachedTransparentableCellsByMaps.TryGetValue(__instance.Map, out dictionary) && dictionary.TryGetValue(__instance.Position, out thingList))
                {
                    foreach (Thing thing in thingList)
                        Core.RecheckTransparency(thing, __instance, thing.def.GetModExtension<ThingExtension>());
                }
                for (int index = Core.transparentThings.Count - 1; index >= 0; --index)
                {
                    Thing transparentThing = Core.transparentThings[index];
                    if (transparentThing.Map == __instance.Map && (double)transparentThing.Position.DistanceTo(__instance.Position) < 20.0)
                        Core.RecheckTransparency(transparentThing, __instance, transparentThing.def.GetModExtension<ThingExtension>());
                }
            }
        }

        [HarmonyPatch(typeof(Thing), "Graphic", MethodType.Getter)]
        public class Patch_Graphic_Postfix
        {
            private static void Postfix(Thing __instance, ref Graphic __result)
            {
                ThingExtension extension;
                if (!__instance.TransparencyAllowed() || !Core.cachedTransparentableThingsByExtensions.TryGetValue(__instance, out extension))
                    return;
                if (__instance.GetTransparentCheckArea(extension).Any<IntVec3>((Func<IntVec3, bool>)(x => Core.HasItemsInCell(x, __instance.Map, extension))))
                {
                    Shader transparencyShader = __instance.GetTransparencyShader();
                    __result = GraphicDatabase.Get(__result.GetType(), "Transparent/" + __result.path, transparencyShader, __instance.def.graphicData.drawSize, __result.color, __result.colorTwo);
                    __instance.Map.mapDrawer.MapMeshDirty(__instance.Position, 1);
                    Core.lastCachedShaders[__instance] = transparencyShader;
                    if (!Core.transparentThings.Contains(__instance))
                        Core.transparentThings.Add(__instance);
                }
                else
                {
                    Core.lastCachedShaders[__instance] = __instance.def.graphicData.shaderType.Shader;
                    Core.transparentThings.Remove(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
        public static class PlaySettings_DoPlaySettingsGlobalControls_Patch
        {
            [HarmonyPrepare]
            public static bool Prepare() => Core.hasTransparentRoofs;

            public static void Postfix(WidgetRow row, bool worldView)
            {
                if (worldView)
                    return;
                bool makeRoofsSelectable = TransparentThingsMod.settings.makeRoofsSelectable;
                row.ToggleableIcon(ref TransparentThingsMod.settings.makeRoofsSelectable, TexButton.ShowRoofOverlay, (string)"TT.MakeRoofsSelectable".Translate(), SoundDefOf.Mouseover_ButtonToggle);
                if (makeRoofsSelectable == TransparentThingsMod.settings.makeRoofsSelectable)
                    return;
                foreach (ThingDef transparentRoof in Core.transparentRoofs)
                    transparentRoof.selectable = TransparentThingsMod.settings.makeRoofsSelectable;
            }
        }
    }
}
