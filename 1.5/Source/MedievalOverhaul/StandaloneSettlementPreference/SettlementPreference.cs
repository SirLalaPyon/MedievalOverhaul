using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StandaloneSettlementPreference
{
    internal class SettlementPreference : DefModExtension
    {
        public float chance = 1f;
        public List<string> biomeKeyWords;
        public bool useTemperatureRange = false;
        public float temperatureRangeMin;
        public float temperatureRangeMax;
        public bool useElevationRange = false;
        public float elevationRangeMin;
        public float elevationRangeMax;
        public bool useSwampinessRange = false;
        public float swampinessRangeMin;
        public float swampinessRangeMax;
        public bool useRainfallRange = false;
        public float rainfallRangeMin;
        public float rainfallRangeMax;
        public List<string> likedBiomeList;
        public List<string> dislikedBiomeList;
        public List<Hilliness> requiredHillLevels;
        public List<Hilliness> disallowedHillLevels;
        public bool requiresWater = false;
        public bool onlyCoastal = false;
        public bool onlyLakeside = false;
        public bool onlyRiver = false;
        public bool onlyRoad = false;
        public bool IgnoreBiomeSelectionWeight = true;

        public static SettlementPreference Get(Def def) => def.GetModExtension<SettlementPreference>();
    }
}
