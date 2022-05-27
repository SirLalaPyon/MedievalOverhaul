using RimWorld;
using Verse;
using UnityEngine;
using System;
using Verse.Sound;

namespace DankPyon_MedievalOverhaul
{
    [StaticConstructorOnStartup]
	public class Comp_WindMill : ThingComp
	{
		public int updateWeatherEveryXTicks = 250;

		private int ticksSinceWeatherUpdate;

		private float spinPosition;

		private Sustainer sustainer;

		[TweakValue("APSGraphics", 0f, 1f)]
		private static float SpinRateFactor = 0.035f;

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

		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 vector = parent.TrueCenter();
			vector += parent.Rotation.FacingCell.ToVector3() * 2.36f;
			for (int i = 0; i < 9; i++)
			{
				float num = spinPosition + (float)Math.PI * 2f * (float)i / 9f;
				float x = Mathf.Abs(4f * Mathf.Sin(num));
				bool num2 = num % ((float)Math.PI * 2f) < (float)Math.PI;
				Vector2 vector2 = new Vector2(x, 1f);
				Vector3 s = new Vector3(vector2.x, 1f, vector2.y);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(vector + Vector3.up * (3f / 74f) * Mathf.Cos(num), parent.Rotation.AsQuat, s);
				Graphics.DrawMesh(num2 ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, WindTurbineBladesMat, 0);
			}
		}
	}
}
