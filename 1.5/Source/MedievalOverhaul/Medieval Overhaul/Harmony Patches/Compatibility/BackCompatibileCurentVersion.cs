using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.GetBackCompatibleType))]
    public class BackCompatibileCurentVersion
    {

        internal static bool Prefix(string providedClassName, ref Type __result, XmlNode node)
        {
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
            }
            return true;
        }
    }
}
