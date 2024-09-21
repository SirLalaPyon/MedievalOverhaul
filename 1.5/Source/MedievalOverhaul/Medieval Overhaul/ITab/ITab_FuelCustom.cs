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
    public class ITab_FuelCustom : ITab
    {
        private static readonly Vector2 WinSize = new (300f, 480f);
        private ThingFilterUI.UIState fuelFilterState = new ();

        protected Building SelBuilding => (Building)this.SelThing;

        public ITab_FuelCustom()
        {
            this.size = ITab_FuelCustom.WinSize;
            this.labelKey = "ESCP_Tools_FuelExtension_TabFuel";
        }

        public override bool IsVisible => !this.Hidden;

        public override bool Hidden => this.IsDisabled();

        public bool IsDisabled() => Utility.LWMFuelFilterIsEnabled;

        public override void OnOpen()
        {
            base.OnOpen();
            this.fuelFilterState.quickSearch.Reset();
        }

        public override void FillTab()
        {
            CompRefuelableCustom comp = this.SelBuilding.GetComp<CompRefuelableCustom>();
            new Rect(0.0f, 0.0f, ITab_FuelCustom.WinSize.x, ITab_FuelCustom.WinSize.y).ContractedBy(10f).SplitHorizontally(18f, out Rect _, out Rect bottom);
            ThingFilterUI.DoThingFilterConfigWindow(bottom, this.fuelFilterState, comp.AllowedFuelFilter, comp.Props.fuelFilter, 1, (IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null, true, true, false, (List<ThingDef>)null, (Map)null);
        }
    }
}
