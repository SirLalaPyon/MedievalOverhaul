using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Designator_DigIce : Designator
    {
        public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        protected override DesignationDef Designation
        {
            get
            {
                return MedievalOverhaulDefOf.DankPyon_DigIce;
            }
        }

        public Designator_DigIce()
        {
            defaultLabel = "DankPyon_Dig".Translate();
            defaultDesc = "DankPyon_DigDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/icesaw");
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
            if (terrain != MedievalOverhaulDefOf.Ice)
            {
                return "DankPyon_MustBeIce".Translate();
            }
            return AcceptanceReport.WasAccepted;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            base.Map.designationManager.AddDesignation(new Designation(c, MedievalOverhaulDefOf.DankPyon_DigIce));
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
