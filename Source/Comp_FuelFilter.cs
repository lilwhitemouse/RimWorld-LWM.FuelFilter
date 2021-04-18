using System;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;

namespace LWM.FuelFilter
{
    public class Comp_FuelFilter : ThingComp {
        public ThingFilter filter;

        public override void Initialize(CompProperties props) {
            filter = new ThingFilter(new Action(this.FilterChanged));
            var fuels = parent.GetComp<CompRefuelable>()?.Props.fuelFilter;
            if (fuels != null) {
                filter.CopyAllowancesFrom(fuels);
            } else {
                Log.Warning("LWM.FuelFilter: Could not find any allowed fuels for " + parent
                            + "; this is probably a mistake on someone's part (maybe not mine)");
            }
        }

        public override void PostExposeData() {
            // Note: we can be fancy and use a callback for changes that looks for anyone carrying
            //   the wrong fuel and kills their job.  Since we do, this is what we use:
            //  Scribe_Deep.Look<ThingFilter>(ref this.filter, "LWMFF_filter", new object[] {new Action(this.TryNotifyChanged)});
            // Without the callback, we would do this:
            //  Scribe_Deep.Look<ThingFilter>(ref this.filter, "LWMFF_filter", Array.Empty<object>());
            // NOTE: remember to us LWMFF_filter and not filter - another mod (or even vanilla) might
            //   use "filter" and comps are not saved separately.
            Scribe_Deep.Look<ThingFilter>(ref this.filter, "LWMFF_filter", new object[] { new Action(this.FilterChanged) });
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.filter == null) {
                // Then we probably are loading a game that was saved WITHOUT the mod loaded?
                Initialize(this.props);
            }
        }

        public void FilterChanged() {
            if (!parent.Spawned) return;
            foreach (var p in parent.Map.mapPawns.FreeColonistsSpawned) {
                if (parent.Map.reservationManager.ReservedBy(parent, p)) {
                    if (p.CurJobDef == JobDefOf.Refuel) {
                        if (p.CurJob.targetB.Thing != null && !filter.Allows(p.CurJob.targetB.Thing))
                            p.jobs.StopAll();
                    }
                    // keep looking at other pawns; for all I know, someone has a mod that
                    //   lets everyone refuel something at once?
                }
            }
        }
    }


}
