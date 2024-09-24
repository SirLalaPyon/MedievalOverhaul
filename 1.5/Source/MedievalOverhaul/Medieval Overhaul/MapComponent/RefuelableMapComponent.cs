using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class RefuelableMapComponent : MapComponent
    {
        public RefuelableMapComponent(Map map) : base(map)
        {
        
        }
        public override void MapGenerated()
        {
            base.MapGenerated();
            GetRefuelableMap();
        }
        public override void MapRemoved()
        {
            base.MapRemoved();

        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            GetRefuelableMap();
        }
        public void GetRefuelableMap()
        {
            foreach (var thing in this.map.listerBuildings.allBuildingsColonist)
            {
                if (thing.comps != null)
                {
                    foreach (var comp in thing.comps)
                    {
                        if (comp != null && comp is CompRefuelableCustom)
                        {
                            refuelableCustomThing.Add(thing);
                        }
                    }
                }
            }

        }
        public void Reset()
        {
            this.refuelableCustomThing.Clear();
        }
        public void Register(ThingWithComps thing)
        {
            if (refuelableCustomThing.Contains(thing))
            {
                return;
            }
            refuelableCustomThing.Add(thing);
        }
        public void Deregister(ThingWithComps thing)
        {
            if (!refuelableCustomThing.Contains(thing))
            {
                return;
            }
            refuelableCustomThing.Remove(thing);
        }

        public HashSet<Thing> refuelableCustomThing = [];
    }
}
