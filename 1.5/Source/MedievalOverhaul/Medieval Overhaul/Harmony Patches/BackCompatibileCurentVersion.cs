using HarmonyLib;
using ProcessorFramework;
using RimWorld;
using System;
using System.Xml;
using Verse;

namespace MedievalOverhaul
{
	[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
	public static class BackCompatibileCurentVersion
	{
		static BackCompatibilityConverter_TentWalls converter = new();

		/// <summary>
		/// Hack to force the game to apply the custom BackCompatibilityConverter when the curent build of the game is loaded (and the def is removed).
		/// </summary>
		/// <remarks>
		/// Note: this patch will no longer be needed once the game realeases version 1.6, beacause:
		/// 1. new saves won't have the removed def and
		/// 2. old saves will be forced though the normal conversion chain
		/// </remarks>
		public static void Postfix(Type defType, string defName, bool forDefInjections, XmlNode node, ref string __result)
		{
			if (Scribe.mode != LoadSaveMode.Inactive && VersionControl.CurrentBuild == ScribeMetaHeaderUtility.loadedGameVersionBuild || GenDefDatabase.GetDefSilentFail(defType, defName, false) != null)
			{
				if (converter.AppliesToLoadedGameVersion())
				{
					string compatibleName = converter.BackCompatibleDefName(defType, defName, forDefInjections, node);

					if (compatibleName != null) {
						__result = compatibleName;
					}
				}
				else
				{
					Log.Error("Developers note: this patch is no logner needed and can be removed");
				}
			}
		}
	}
}
