using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace LWM.FuelFilter
{
    public class ITab_FuelFilter : ITab
    {
        public ITab_FuelFilter()
        {
            this.size=WinSize;
            this.labelKey = "LWM.FuelFilter.FuelLabel";
        }

        public override bool IsVisible
		{
			get
			{
				ThingWithComps refuelable = base.SelObject as ThingWithComps;
                // Is it player-controlled:
				if (refuelable != null && refuelable.Faction != null 
                    && refuelable.Faction != Faction.OfPlayer)
				{
					return false;
				}
                // Does it use fuel? Does it use more than one kind of fuel?
                var fuelFilter = refuelable.GetComp<CompRefuelable>()?.Props.fuelFilter;
                if (fuelFilter == null || fuelFilter.AllowedDefCount < 2) return false;
                // Did we correctly give it a Comp_FuelFilter?
                return refuelable.GetComp<Comp_FuelFilter>() != null;
			}
		}

        protected CompRefuelable GetCompR {
            get => (SelObject as ThingWithComps)?.GetComp<CompRefuelable>();
        }

        protected Comp_FuelFilter GetCompFF {
            get => (SelObject as ThingWithComps)?.GetComp<Comp_FuelFilter>();
        }

        protected override void FillTab()
		{
			Rect position = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			GUI.BeginGroup(position);

            ThingFilter parentFilter = GetCompR.Props.fuelFilter;
            ThingFilter filter = GetCompFF.filter;

            if (filter != null && parentFilter != null) {
                Rect rect2 = new Rect(0f, 20f, position.width, position.height - 20f);
                ThingFilterUI.DoThingFilterConfigWindow(rect2, this.thingFilterState, filter,
                                  parentFilter, 8, null, null, false, null, null);
            } else {
                Log.Warning("FuelFilter - had null filter??"); // sanity check
            }
            GUI.EndGroup();
        }



        private static readonly Vector2 WinSize = new Vector2(300f, 480f);
        private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();
    }
}
