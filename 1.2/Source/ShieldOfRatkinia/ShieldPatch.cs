﻿using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;
using Verse.AI.Group;
using RimWorld.Planet;

namespace NewRatkin
{

    [StaticConstructorOnStartup]
    public static class ShieldPatch
    {
        private static readonly Type patchType = typeof(ShieldPatch);
        static ShieldPatch()
        {
            Harmony harmonyInstance = new Harmony("com.NewRatkin.rimworld.mod");
            harmonyInstance.Patch(AccessTools.Method(typeof(WorkGiver_HunterHunt), "HasShieldAndRangedWeapon"), new HarmonyMethod(patchType, "HasShieldAndRangedWeaponPrefix"));
            harmonyInstance.Patch(AccessTools.Method(typeof(Alert_ShieldUserHasRangedWeapon), "GetReport"), new HarmonyMethod(patchType, "GetReportPrefix"));
        }
        public static bool GetReportPrefix(ref AlertReport __result)
        {
            __result = AlertReport.CulpritsAre(ShieldUsersWithRangedWeapon());
            return false;
        }
        public static List<Pawn> ShieldUsersWithRangedWeapon()
        {
            List<Pawn> pawns = new List<Pawn>();

            foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                if (p.equipment.Primary != null && p.equipment.Primary.def.IsWeaponUsingProjectiles)
                {
                    List<Apparel> ap = p.apparel.WornApparel;
                    for (int i = 0; i < ap.Count; i++)
                    {
                        if (ap[i] is Shield || ap[i] is ShieldBelt)
                        {
                            pawns.Add(p);
                            break;
                        }
                    }
                }
            }
            return pawns;
        }
        public static bool HasShieldAndRangedWeaponPrefix(ref bool __result, Pawn p)
        {
            if (p.equipment.Primary != null && p.equipment.Primary.def.IsWeaponUsingProjectiles)
            {
                List<Apparel> wornApparel = p.apparel.WornApparel;
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    if (wornApparel[i] is ShieldBelt || wornApparel[i] is Shield)
                    {
                        __result = true;
                        return false;
                    }
                }
            }
            __result = false;
            return false;
        }
    }
}