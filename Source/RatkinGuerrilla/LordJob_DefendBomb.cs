﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using System.Text;
using System.Reflection;
using RimWorld;
using Harmony;
using UnityEngine;
using Verse.AI;

namespace NewRatkin
{
    public class LordJob_BombPlanting : LordJob
    {
        private Faction faction;

        private IntVec3 defendingSpot;

        private float eventPoint;

        public LordJob_BombPlanting()
        {
        }

        public LordJob_BombPlanting(Faction faction, IntVec3 defendingSpot, float eventPoint)
        {
            this.faction = faction;
            this.defendingSpot = defendingSpot;
            this.eventPoint = eventPoint;
        }

        public override bool GuiltyOnDowned
        {
            get
            {
                return true;
            }
        }

        public override StateGraph CreateGraph()
        {
            
            StateGraph stateGraph = new StateGraph();
            LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(defendingSpot).CreateGraph()).StartingToil;
            LordToil_DefendBomb lordToil_Siege = new LordToil_DefendBomb(defendingSpot,eventPoint);
            stateGraph.AddToil(lordToil_Siege);
            Transition transition = new Transition(startingToil, lordToil_Siege, false, true);
            transition.AddTrigger(new Trigger_Memo("TravelArrived"));
            transition.AddTrigger(new Trigger_TicksPassed(5000));
            stateGraph.AddTransition(transition, false);
            LordToil lordToil_Assault = stateGraph.AttachSubgraph(new LordJob_AssaultColony(this.faction, true, true, false, false, true).CreateGraph()).StartingToil;
            Transition transition2 = new Transition(lordToil_Siege, lordToil_Assault, false, true);
            transition2.AddTrigger(new Trigger_Memo("NoBuilders"));
            transition2.AddTrigger(new Trigger_Memo("NoBomb"));
            transition2.AddTrigger(new Trigger_PawnHarmed(0.08f, false, null));
            transition2.AddTrigger(new Trigger_FractionPawnsLost(0.3f));
            transition2.AddTrigger(new Trigger_TicksPassed(15000));
            transition2.AddPreAction(new TransitionAction_Message("MessageCommandoAssaulting".Translate(faction.def.pawnsPlural, faction), MessageTypeDefOf.ThreatBig, null, 1f));
            transition2.AddPostAction(new TransitionAction_WakeAll());
            stateGraph.AddTransition(transition2, false);

            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction", false);
            Scribe_Values.Look(ref defendingSpot, "siegeSpot", default(IntVec3), false);
            Scribe_Values.Look(ref eventPoint, "eventPoint", 0f, false);
        }
    }
}