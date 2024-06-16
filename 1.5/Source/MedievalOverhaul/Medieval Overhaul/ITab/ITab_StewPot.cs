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
    public class ITab_StewPot : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(300f, 480f);
        private ThingFilterUI.UIState fuelFilterState = new ThingFilterUI.UIState();

        protected Building SelBuilding => (Building)this.SelThing;

        public ITab_StewPot()
        {
            this.size = ITab_StewPot.WinSize;
            this.labelKey = "DankPyon_StewPot_Itab";
        }

        public override bool IsVisible => !this.Hidden;

        public override bool Hidden => this.IsDisabled();

       public bool IsDisabled() => false;

        public override void OnOpen()
        {
            base.OnOpen();
            this.fuelFilterState.quickSearch.Reset();
        }

        public override void FillTab()
        {
            CompRefuelableStat comp1 = this.SelBuilding.GetComp<CompRefuelableStat>();
            //CompRefuelableStat comp2 = this.SelBuilding.GetComp<CompRefuelableStat>();
            Rect bottom;
            new Rect(0.0f, 0.0f, ITab_StewPot.WinSize.x, ITab_StewPot.WinSize.y).ContractedBy(10f).SplitHorizontally(18f, out Rect _, out bottom);
            ThingFilterUI.DoThingFilterConfigWindow(bottom, this.fuelFilterState, comp1.AllowedFuelFilter, comp1.Props.fuelFilter, 1, (IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null, true, true, false, (List<ThingDef>)null, (Map)null);
        }
    }
}