using RimWorld;
using Verse;
using UnityEngine;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public class Comp_FireOverlayCustom : CompFireOverlayBase
    {
        public new CompProperties_FireOverlayCustom Props
        {
            get
            {
                return (CompProperties_FireOverlayCustom)this.props;
            }
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (this.refuelableCompCustom != null && !this.refuelableCompCustom.HasFuel)
            {
                return;
            }
            Vector3 drawPos = this.parent.DrawPos;
            drawPos.y += 0.03846154f;
            CompFireOverlay.FireGraphic.Draw(drawPos, this.parent.Rotation, this.parent, 0f);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.refuelableCompCustom = this.parent.GetComp<CompRefuelableCustom>();
        }
        public override void CompTick()
        {
            if (this.refuelableCompCustom != null && !this.refuelableCompCustom.HasFuel)
            {
                return;
            }
            if (this.startedGrowingAtTick < 0)
            {
                this.startedGrowingAtTick = GenTicks.TicksAbs;
            }
        }
        protected CompRefuelableCustom refuelableCompCustom;
        public static readonly Graphic FireGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Fire", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
    }
}
