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
        public List<string> biomeKeyWords = null;
        public bool useTemperatureRange = false;
        public float temperatureRangeMin = 0f;
        public float temperatureRangeMax = 0f;
        public bool useElevationRange = false;
        public float elevationRangeMin = 0f;
        public float elevationRangeMax = 0f;
        public bool useSwampinessRange = false;
        public float swampinessRangeMin = 0f;
        public float swampinessRangeMax = 0f;
        public bool useRainfallRange = false;
        public float rainfallRangeMin = 0f;
        public float rainfallRangeMax = 0f;
        public List<string> likedBiomeList = null;
        public List<string> dislikedBiomeList = null;
        public List<Hilliness> requiredHillLevels = null;
        public List<Hilliness> disallowedHillLevels = null;
        public bool requiresWater = false;
        public bool onlyCoastal = false;
        public bool onlyLakeside = false;
        public bool onlyRiver = false;
        public bool onlyRoad = false;
        public bool IgnoreBiomeSelectionWeight = true;

        public static SettlementPreference Get(Def def) => def.GetModExtension<SettlementPreference>();
    }
}
