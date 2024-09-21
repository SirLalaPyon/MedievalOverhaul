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
    //public class ITab_Fuel : ITab
    //{
    //    private static readonly Vector2 WinSize = new Vector2(300f, 480f);
    //    private ThingFilterUI.UIState fuelFilterState = new ThingFilterUI.UIState();

    //    protected Building SelBuilding => (Building)this.SelThing;

    //    public ITab_Fuel()
    //    {
    //        this.size = ITab_Fuel.WinSize;
    //        this.labelKey = "ESCP_Tools_FuelExtension_TabFuel";
    //    }

    //    public override bool IsVisible => !this.Hidden;

    //    public override bool Hidden => this.IsDisabled();

    //    public bool IsDisabled() => Utility.LWMFuelFilterIsEnabled;

    //    public override void OnOpen()
    //    {
    //        base.OnOpen();
    //        this.fuelFilterState.quickSearch.Reset();
    //    }

    //    public override void FillTab()
    //    {
    //        CompStoreFuelThing comp1 = this.SelBuilding.GetComp<CompStoreFuelThing>();
    //        CompRefuelable comp2 = this.SelBuilding.GetComp<CompRefuelable>();
    //        Rect bottom;
    //        new Rect(0.0f, 0.0f, ITab_Fuel.WinSize.x, ITab_Fuel.WinSize.y).ContractedBy(10f).SplitHorizontally(18f, out Rect _, out bottom);
    //        ThingFilterUI.DoThingFilterConfigWindow(bottom, this.fuelFilterState, comp1.AllowedFuelFilter, comp2.Props.fuelFilter, 1, (IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null, true, true, false, (List<ThingDef>)null, (Map)null);
    //    }
    //}
}
