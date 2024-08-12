using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace MedievalOverhaul
{
	public class BackCompatibilityConverter_TentWalls : BackCompatibilityConverter
	{
		public override bool AppliesToVersion(int majorVer, int minorVer) => true; // majorVer == 1 && minorVer <= 5;

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef))
			{
				if (defName == "DankPyon_HideTentWall")
				{
					return "DankPyon_TentWall";
				}
				else if (defName == "Blueprint_DankPyon_HideTentWall")
				{
					return "Blueprint_DankPyon_TentWall";
				}
				else if (defName == "Frame_DankPyon_HideTentWall")
				{
					return "Frame_DankPyon_TentWall";
				}
			}

			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			return null;
		}

		public override void PostExposeData(object obj)
		{
			return;
		}
	}
}
