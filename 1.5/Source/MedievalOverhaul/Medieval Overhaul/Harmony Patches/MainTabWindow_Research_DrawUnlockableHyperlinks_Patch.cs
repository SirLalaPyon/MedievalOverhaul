using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace MedievalOverhaul.Patches
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
    [HotSwappable]
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawUnlockableHyperlinks")]
    public static class MainTabWindow_Research_DrawUnlockableHyperlinks_Patch
    {
		/// <summary>
		/// Adds custom schemaic gates to the resarch tree
		/// </summary>
		/// <remarks>
		/// DankPyon_BasicPolearms can be used to verfy that the patch is working.
		/// Developers note: there is a UI bug in which the source mod line that the game adds is not moved down to accomidate the new schematic block and thus overlaps the existing text
		/// </remarks>
		public static void Prefix(MainTabWindow_Research __instance, ref Rect rect, Rect visibleRect, ResearchProjectDef project)
        {
            var extension = project.GetModExtension<RequiredSchematic>();
            if (extension != null)
            {
                Widgets.LabelCacheHeight(ref rect, "DankPyon_RequiredSchematic".Translate() + ":");
                Rect rect2 = new Rect(rect.x, rect.yMin + 24, rect.width, 24f);
                var item2 = extension.schematicDef;
                if (visibleRect.Overlaps(rect2))
                {
                    Color? color = null;

					QuickSearchWidget searchWidget = (QuickSearchWidget) AccessTools.Field(__instance.GetType(), "quickSearchWidget").GetValue(__instance);
					if (searchWidget.filter.Active)
                    {
                        MethodInfo matchesUnlockedDef = AccessTools.Method(__instance.GetType(), "MatchesUnlockedDef", new Type[] { typeof(Def) });

                        if ((bool) matchesUnlockedDef.Invoke(__instance, new object[] { item2 }))
                        {
                            Widgets.DrawTextHighlight(rect2);
                        }
                        else
                        {
							MethodInfo noMatchTint = AccessTools.Method(__instance.GetType(), "NoMatchTint", new Type[] { typeof(Color) });

                            color = (Color)noMatchTint.Invoke(__instance, new object[] { Widgets.NormalOptionColor });
                        }
                    }
                    rect.x += 6f;
                    rect.yMin += rect2.height;
                    Dialog_InfoCard.Hyperlink hyperlink = new Dialog_InfoCard.Hyperlink(item2);

					MethodInfo labelSuffixForUnlocked = AccessTools.Method(__instance.GetType(), "LabelSuffixForUnlocked", new Type[] { typeof(Def) });

                    Widgets.HyperlinkWithIcon(rect2, hyperlink, null, 2f, 6f, color, truncateLabel: false, (string)labelSuffixForUnlocked.Invoke(__instance, new object[] { item2 }));
                }
                rect.x -= 6f;
                rect.yMin += 24f;
            }
        }
    }
}
