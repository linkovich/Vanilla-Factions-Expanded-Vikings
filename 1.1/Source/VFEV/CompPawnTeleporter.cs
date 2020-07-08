﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEV
{
    public class CompPawnTeleporter : ThingComp
    {
        public bool disappear = false;

        public int appearInTick = 0;
        public CompProperties_PawnTeleporter Props
        {
            get
            {
                return (CompProperties_PawnTeleporter)this.props;
            }
        }

        private int readyToUseTicks = 0;
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent is Pawn pawn && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f
                    && Find.TickManager.TicksGame >= readyToUseTicks)
            {
                var hostiles = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget).Where(x
                    => x.HostileTo(pawn));
                readyToUseTicks = Find.TickManager.TicksGame + Props.cooldown;
                IntVec3 loc = IntVec3.Invalid;
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) =>
                    hostiles.Where(y => y.Position.DistanceTo(x) > Props.minDistance).Count() == 0, 
                    pawn.Map, out loc))
                {
                    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
                    disappear = true;
                    appearInTick = Find.TickManager.TicksGame + 120;
                    var mapComp = pawn.Map.GetComponent<MapComponentTeleportHelper>();
                    if (mapComp.pawnsToTeleport == null) mapComp.pawnsToTeleport = new Dictionary<Pawn, TargetInfo>();
                    mapComp.pawnsToTeleport[pawn] = new TargetInfo(loc, pawn.Map);
                    pawn.DeSpawn(DestroyMode.Vanish);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref readyToUseTicks, "readyToUseTicks", 0);
        }
    }
}

