using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class Gizmo_RefuelableNutritionStatus : Gizmo
    {
        public Gizmo_RefuelableNutritionStatus()
        {
            this.Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(1523289473, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect2;
                Rect rect = rect2 = overRect.AtZero().ContractedBy(6f);
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, this.refuelable.Props.FuelGizmoLabel);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = this.refuelable.Fuel / this.refuelable.Props.fuelCapacity;
                Widgets.FillableBar(rect3, fillPercent, Gizmo_RefuelableNutritionStatus.FullBarTex, Gizmo_RefuelableNutritionStatus.EmptyBarTex, false);
                if (this.refuelable.Props.targetFuelLevelConfigurable)
                {
                    float num = this.refuelable.TargetFuelLevel / this.refuelable.Props.fuelCapacity;
                    float x = rect3.x + num * rect3.width - (float)Gizmo_RefuelableNutritionStatus.TargetLevelArrow.width * 0.5f / 2f;
                    float y = rect3.y - (float)Gizmo_RefuelableNutritionStatus.TargetLevelArrow.height * 0.5f;
                    GUI.DrawTexture(new Rect(x, y, (float)Gizmo_RefuelableNutritionStatus.TargetLevelArrow.width * 0.5f, (float)Gizmo_RefuelableNutritionStatus.TargetLevelArrow.height * 0.5f), Gizmo_RefuelableNutritionStatus.TargetLevelArrow);
                }
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, this.refuelable.Fuel.ToString("F0") + " / " + this.refuelable.Props.fuelCapacity.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f, null);
            return new GizmoResult(GizmoState.Clear);
        }

        public CompRefuelableStat refuelable;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

        private static readonly Texture2D TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated", true);

        private const float ArrowScale = 0.5f;
    }
}
