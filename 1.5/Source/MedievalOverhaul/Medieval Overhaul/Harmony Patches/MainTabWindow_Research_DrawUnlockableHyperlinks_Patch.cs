using HarmonyLib;
using RimWorld;
using System;
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
        public static void Prefix(MainTabWindow_Research __instance, ref Rect rect, Rect visibleRect, ResearchProjectDef project)
        {
            var extension = project.GetModExtension<RequiredSchematic>();
            if (extension != null)
            {
                Widgets.LabelCacheHeight(ref rect, "DankPyon_RequiredSchematic".Translate() + ":");
                Rect rect2 = new (rect.x, rect.yMin + 24, rect.width, 24f);
                var item2 = extension.schematicDef;
                if (visibleRect.Overlaps(rect2))
                {
                    Color? color = null;
                    if (__instance.quickSearchWidget.filter.Active)
                    {
                        if (__instance.MatchesUnlockedDef(item2))
                        {
                            Widgets.DrawTextHighlight(rect2);
                        }
                        else
                        {
                            color = __instance.NoMatchTint(Widgets.NormalOptionColor);
                        }
                    }
                    rect.x += 6f;
                    rect.yMin += rect2.height;
                    Dialog_InfoCard.Hyperlink hyperlink = new (item2);
                    Widgets.HyperlinkWithIcon(rect2, hyperlink, null, 2f, 6f, color, truncateLabel: false, __instance.LabelSuffixForUnlocked(item2));
                }
                rect.x -= 6f;
                rect.yMin += 24f;
            }
        }
    }
}
