using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class SeperateHideList : Def
    {
        public List<string> whiteListRaces;
        public List<string> whiteListLeathers;
        public List<string> blackListRaces;
        public List<string> blackListLeathers;
    }
    public class HideGraphicList : Def
    {
        public List<string> scaleHidesBodyDef;
        public List<string> scaleHidesRaceDef;
        public List<string> furHidesBodyDef;
        public List<string> furHidesRaceDef;
        public List<string> plainHidesBodyDef;
        public List<string> plainHidesRaceDef;   
    }

    public class SeperateWoodList : Def
    {
        public List<ThingDef> blackListWood;
        public Dictionary<ThingDef, ThingDef> plankDict;
    }

    //public class FuelPatchList : Def
    //{
    //    public List<ThingDef> blackListRefuelable;

    //}
}