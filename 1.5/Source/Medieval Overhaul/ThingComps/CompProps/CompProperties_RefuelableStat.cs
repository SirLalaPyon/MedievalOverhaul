using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
	public class CompProperties_RefuelableStat : CompProperties_Refuelable
	{
		public StatDef stat;
		public string fuelLevelIconPath;

        private Texture2D fuelLevelIcon;

        public Texture2D FuelLevelIcon
		{
			get
			{
				if (fuelLevelIcon == null)
					fuelLevelIcon = ContentFinder<Texture2D>.Get(fuelLevelIconPath.NullOrEmpty() ? "UI/Commands/SetTargetFuelLevel" : fuelLevelIconPath);
				return fuelLevelIcon;
			}
		}
        public CompProperties_RefuelableStat()
		{
			compClass = typeof(CompRefuelableStat);
		}
	}
}