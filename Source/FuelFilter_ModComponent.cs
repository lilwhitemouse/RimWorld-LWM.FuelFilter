using HarmonyLib;
using System;
//using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;
/***********************************************
 * LWM's Fuel Filter
 * Basic idea:
 *   Create a comp that has a ThingFilter
 *   Add that comp to everything that has a CompRefuelable
 *            (via C# instead of XML b/c XML would not catch CompDerivedFromRefuelable)
 *   Add an ITab that shows the filter (using the CompRefuelable's filter as parent)
 *   Patch the refuel job to use our filter instead of original filter for list
 *     of fuels to use.
 * Could also do:
 *   One could also do a similar patch in CompProperties_Refuelable (to make icon look
 *       correct)
 *   One could also do a similar patch in CompRefuelable to:
 *     display correct "out of fuel" message
 *     spawn some fuel if the building is destroyed
 * These are small details and not worth my time.
 */
namespace LWM.FuelFilter
{
    public class FuelFilter_ModComponent : Mod
    {
        public FuelFilter_ModComponent(ModContentPack content) : base(content)
        {
            try
            {
                var harmonyInstance = new Harmony("net.littlewhitemouse.fuelfilter");
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Log.Error("LWM's FuelFilter :: Caught exception: " + ex);
            }
        }
    }

    [StaticConstructorOnStartup]
    public class DoHorrifyingThingsToDefDatabase {
        static DoHorrifyingThingsToDefDatabase() {
            foreach (var d in DefDatabase<ThingDef>.AllDefs) {
                if (d.comps != null) {
                    foreach (var c in d.comps) {
                        if (c is CompProperties_Refuelable cpr /*&& cpr.fuelFilter.AllowedDefCount > 1*/) {
                            //                                   some mods allow def editing in-game
                            #if DEBUG
                            Log.Message("LWM Fuel Filter patching "+d.defName+" for fuels "
                                +String.Join(", ", cpr.fuelFilter.AllowedThingDefs.Select(a=>a.ToString())));
                            #endif
                            d.comps.Add(new CompProperties() { compClass = typeof(Comp_FuelFilter) });
                            if (d.inspectorTabs == null) d.inspectorTabs = new System.Collections.Generic.List<Type>();
                            d.inspectorTabs.Add(typeof(ITab_FuelFilter));
                            // But ITabs are "Resolved," and stored internally in inspectorTabsResolved -
                            //   because we are doing this via C# rather than XML patching, we have to do
                            //   the resolving ourselvs:
                            if (d.inspectorTabsResolved == null) d.inspectorTabsResolved = new System.Collections.Generic.List<InspectTabBase>();
                            d.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_FuelFilter)));
                            break;
                        }
                    }
                }
            }
        }
    }
}
