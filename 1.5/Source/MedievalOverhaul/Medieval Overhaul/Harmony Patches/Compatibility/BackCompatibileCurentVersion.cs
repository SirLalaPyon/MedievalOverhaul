using HarmonyLib;
using System;
using System.Linq;
using System.Xml;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.GetBackCompatibleType))]
    public class BackCompatibile_GetBackCompatibleType
    {

        internal static bool Prefix(string providedClassName, ref Type __result, XmlNode node)
        {
            if (node["stuff"] != null && node["stuff"].InnerText.ToString() == "DankPyon_Log_RawDarkWood")
            {
                node["stuff"].InnerText = "DankPyon_RawDarkWood";
                Log.Message(node["stuff"].InnerText.ToString());
            }
            if (providedClassName == "Building_WorkTable_HeatPush" && node["def"] != null)
            {
                if (node["def"].InnerText.ToString() == "DankPyon_Furnace" ||
                    node["def"].InnerText.ToString() == "DankPyon_RusticCookingTable" ||
                    node["def"].InnerText.ToString() == "DankPyon_StoneOven" ||
                    node["def"].InnerText.ToString() == "DankPyon_StoneOvenLarge" ||
                    node["def"].InnerText.ToString() == "DankPyon_Grill" ||
                    node["def"].InnerText.ToString() == "DankPyon_StewPot" ||
                    node["def"].InnerText.ToString() == "DankPyon_Pyre" ||
                    node["def"].InnerText.ToString() == "DankPyon_PyreRound" ||
                    node["def"].InnerText.ToString() == "DankPyon_RusticHearth" ||
                    node["def"].InnerText.ToString() == "FueledStove"
                    )
                {
                    __result = typeof(Building_WorkTable_HeatPushCustom);
                    return false;
                }
                //if (RefuelablePatching.AllWorkTable_HeatPushRefuelables.Contains(node["def"].InnerText.ToString()))
                //{
                //    __result = typeof(Building_WorkTable_HeatPushCustom);
                //    return false;
                //}
            }
            if (providedClassName == "Building_WorkTable" && node["def"] != null)
            {
                if (node["def"].InnerText.ToString() == "Campfire" ||
                    node["def"].InnerText.ToString() == "FueledSmithy"
                    )
                {
                    __result = typeof(Building_WorkTableCustom);
                    return false;
                }
                //if (RefuelablePatching.AllWorkTable_Refuelables.Contains(node["def"].InnerText.ToString()))
                //{
                //    __result = typeof(Building_WorkTableCustom);
                //    return false;
                //}
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
    public class BackCompatibile_BackCompatibleDefName
    {

        internal static void Postfix(Type defType, ref string __result, bool forDefInjections = false, XmlNode node = null)
        {
            if (defType == typeof(ThingDef))
            {
                if (__result == "DankPyon_Log_RawDarkWood")
                {
                    __result = "DankPyon_RawDarkWood";
                    return;

                }
            }
        }
    }
}