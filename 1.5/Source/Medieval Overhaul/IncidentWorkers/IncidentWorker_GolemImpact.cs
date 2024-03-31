using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace MedievalOverhaul
{
    public class IncidentWorker_GolemImpact : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            return IncidentWorker_GolemImpact.TryFindCell(out intVec, map);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            List<Thing> contents;
            Skyfaller skyfaller = this.SpawnMeteoriteIncoming(parms, out contents);
            if (skyfaller == null)
            {
                return false;
            }
            skyfaller.impactLetter = this.MakeLetter(skyfaller, contents);
            return true;
        }

        protected virtual List<Thing> GenerateMeteorContents(IncidentParms parms)
        {
            return MedievalOverhaulDefOf.DankPyon_GolemMeteorite.root.Generate();
        }

        protected Skyfaller SpawnMeteoriteIncoming(IncidentParms parms, out List<Thing> contents)
        {
            Map map = (Map)parms.target;
            IntVec3 pos;
            if (!IncidentWorker_GolemImpact.TryFindCell(out pos, map))
            {
                contents = null;
                return null;
            }
            contents = this.GenerateMeteorContents(parms);
            return SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, contents, pos, map);
        }

        protected virtual Letter MakeLetter(Skyfaller meteorite, List<Thing> contents)
        {
            Thing thing = (contents != null) ? contents[0] : null;
            if (thing == null)
            {
                return null;
            }
            LetterDef def = LetterDefOf.NeutralEvent;
            string str = this.def.letterText.Formatted(thing.def.label).CapitalizeFirst();
            return LetterMaker.MakeLetter(this.def.letterLabel + ": " + thing.def.LabelCap, str, def, new TargetInfo(meteorite.Position, meteorite.Map, false), null, null, null);
        }

        private static bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ThingSetMaker_GolemImpact.MineablesCountRange.max;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, true, false, false, false, true, true, delegate (IntVec3 x)
            {
                int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
                CellRect other = CellRect.CenteredOn(x, num, num);
                int num2 = 0;
                foreach (IntVec3 c in other)
                {
                    if (c.InBounds(map) && c.Standable(map))
                    {
                        num2++;
                    }
                }
                return num2 >= maxMineables;
            });
        }
    }
}