using RimWorld;
using Verse;
using UnityEngine;
using System;
using Verse.Sound;

namespace DankPyon_MedievalOverhaul
{
    [StaticConstructorOnStartup]
	public class CompWindMill : ThingComp
	{
		public int updateWeatherEveryXTicks = 250;

		private int ticksSinceWeatherUpdate;

		private float spinPosition;

		private Sustainer sustainer;

		[TweakValue("APSGraphics", 0f, 1f)]
		private static float SpinRateFactor = 0.035f;

		[TweakValue("APSGraphics", 0f, 80f)]
		private static float BladeHeight = 11.5f;

		[TweakValue("APSGraphics", 0f, 80f)]
		private static float BladeWidth = 1.12f;

		private static readonly Material WindTurbineBladesMat = MaterialPool.MatFrom("Buildings/Windmill/WindmillBlades");

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			spinPosition = Rand.Range(0f, 15f);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.End();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksSinceWeatherUpdate, "updateCounter", 0);
		}
		private float cachedPowerOutput;
		private float PowerPercent => cachedPowerOutput / 250f;
		public override void CompTick()
		{
			base.CompTick();
			ticksSinceWeatherUpdate++;
			if (ticksSinceWeatherUpdate >= updateWeatherEveryXTicks)
			{
				float num = Mathf.Min(parent.Map.windManager.WindSpeed, 1.5f);
				ticksSinceWeatherUpdate = 0;
				cachedPowerOutput = 250 * num;
			}
			if (cachedPowerOutput > 0.01f)
			{
				spinPosition += PowerPercent * SpinRateFactor;
			}
			if (sustainer == null || sustainer.Ended)
			{
				sustainer = SoundDefOf.WindTurbine_Ambience.TrySpawnSustainer(SoundInfo.InMap(parent));
			}
			sustainer.Maintain();
		}
		[TweakValue("Graphics", -1f, 3f)]
		private static float HorizontalBladeOffset = -0.02f;

		[TweakValue("Graphics", 0f, 3f)]
		private static float VerticalBladeOffset = 2.5f;
		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 pos = parent.TrueCenter();
			pos += parent.Rotation.FacingCell.ToVector3() * VerticalBladeOffset;
			pos += parent.Rotation.RighthandCell.ToVector3() * HorizontalBladeOffset;
			pos.y += 3f / 74f;
			float num = BladeHeight * Mathf.Sin(spinPosition);
			if (num < 0f)
			{
				num *= -1f;
			}
			bool num2 = spinPosition % (float)Math.PI * 2f < (float)Math.PI;
			Vector2 vector = new Vector2(num, BladeWidth);
			Vector3 s = new Vector3(vector.x, 1f, vector.y);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
			Graphics.DrawMesh(num2 ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, WindTurbineBladesMat, 0);
			pos.y += YOffset;
			matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
			Graphics.DrawMesh(num2 ? MeshPool.plane10Flip : MeshPool.plane10, matrix, WindTurbineBladesMat, 0);
		}

		[TweakValue("APSGraphics", -10f, 10f)] private static float YOffset = -0.02f;
	}
}
