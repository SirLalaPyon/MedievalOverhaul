using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Designator_GatherIce : Designator
    {
        public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        protected override DesignationDef Designation
        {
            get
            {
                return MedievalOverhaulDefOf.DankPyon_GatherIce;
            }
        }

        public Designator_GatherIce()
        {
            defaultLabel = "DankPyon_GatherIce".Translate();
            defaultDesc = "DankPyon_GatherIceDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/GatherWater");
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_SmoothSurface;
            hotKey = KeyBindingDefOf.Misc1;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map) || c.Fogged(base.Map) || base.Map.designationManager.DesignationAt(c, Designation) != null)
            {
                return false;
            }
            if (c.InNoBuildEdgeArea(base.Map))
            {
                return "TooCloseToMapEdge".Translate();
            }
            Building edifice = c.GetEdifice(base.Map);
            if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
            {
                return false;
            }
            TerrainDef terrain = base.Map.terrainGrid.TerrainAt(c);
            if (terrain == TerrainDefOf.Ice || terrain.IsWater)
            {
                return AcceptanceReport.WasAccepted;

            }
            return "DankPyon_MustBeIce".Translate();
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            base.Map.designationManager.AddDesignation(new Designation(c, MedievalOverhaulDefOf.DankPyon_GatherIce));
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }
    }
}
