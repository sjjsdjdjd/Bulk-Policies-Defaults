using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BulkOutfitPolicy
{
    [StaticConstructorOnStartup]
    internal static class Startup
    {
        static Startup()
        {
            new Harmony("local.bulkoutfitpolicy").PatchAll();
        }
    }

    internal static class BulkPolicyButtons
    {
        private const int ExtraHeaderHeight = 32;
        private const int ButtonHeight = 32;

        public static void PrepareHeader(
            Rect headerRect,
            string labelKey,
            Action<List<Pawn>> openMenu)
        {
            if (HasMultipleColonistsSelected())
            {
                Rect buttonRect = new Rect(
                    headerRect.x,
                    headerRect.y,
                    Mathf.Min(headerRect.width, 360f),
                    ButtonHeight);

                if (Widgets.ButtonText(buttonRect, labelKey.Translate()))
                {
                    openMenu(GetSelectedColonists());
                }
            }
        }

        public static void EnsureHeaderSpace(ref int height)
        {
            if (HasMultipleColonistsSelected())
            {
                height += ExtraHeaderHeight;
            }
        }

        public static bool HasMultipleColonistsSelected()
        {
            int count = 0;
            foreach (object selectedObject in Find.Selector.SelectedObjects)
            {
                if (selectedObject is Pawn pawn &&
                    pawn.IsColonist &&
                    pawn.Faction == Faction.OfPlayer &&
                    ++count >= 2)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<Pawn> GetSelectedColonists()
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach (object selectedObject in Find.Selector.SelectedObjects)
            {
                if (selectedObject is Pawn pawn &&
                    pawn.IsColonist &&
                    pawn.Faction == Faction.OfPlayer)
                {
                    pawns.Add(pawn);
                }
            }

            return pawns;
        }

        public static void OpenPolicyMenu<TPolicy>(
            List<Pawn> pawns,
            IEnumerable<TPolicy> policies,
            Func<TPolicy, string> getLabel,
            Action<Pawn, TPolicy> applyPolicy)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (TPolicy policy in policies)
            {
                TPolicy selectedPolicy = policy;
                options.Add(new FloatMenuOption(getLabel(selectedPolicy), delegate
                {
                    foreach (Pawn pawn in pawns)
                    {
                        applyPolicy(pawn, selectedPolicy);
                    }
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_Outfit), nameof(PawnColumnWorker_Outfit.DoHeader))]
    internal static class OutfitHeaderPatch
    {
        private static void Prefix(Rect rect)
        {
            BulkPolicyButtons.PrepareHeader(rect, "BulkOutfitPolicy_Apparel", OpenMenu);
        }

        private static void OpenMenu(List<Pawn> pawns)
        {
            BulkPolicyButtons.OpenPolicyMenu(
                pawns,
                Current.Game.outfitDatabase.AllOutfits,
                policy => policy.label,
                (pawn, policy) =>
                {
                    if (pawn.outfits != null)
                    {
                        pawn.outfits.CurrentApparelPolicy = policy;
                    }
                });
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_Outfit), nameof(PawnColumnWorker_Outfit.GetMinHeaderHeight))]
    internal static class OutfitHeaderHeightPatch
    {
        private static void Postfix(ref int __result)
        {
            BulkPolicyButtons.EnsureHeaderSpace(ref __result);
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_FoodRestriction), nameof(PawnColumnWorker_FoodRestriction.DoHeader))]
    internal static class FoodHeaderPatch
    {
        private static void Prefix(Rect rect)
        {
            BulkPolicyButtons.PrepareHeader(rect, "BulkOutfitPolicy_Food", OpenMenu);
        }

        private static void OpenMenu(List<Pawn> pawns)
        {
            BulkPolicyButtons.OpenPolicyMenu(
                pawns,
                Current.Game.foodRestrictionDatabase.AllFoodRestrictions,
                policy => policy.label,
                (pawn, policy) =>
                {
                    if (pawn.foodRestriction != null)
                    {
                        pawn.foodRestriction.CurrentFoodPolicy = policy;
                    }
                });
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_FoodRestriction), nameof(PawnColumnWorker_FoodRestriction.GetMinHeaderHeight))]
    internal static class FoodHeaderHeightPatch
    {
        private static void Postfix(ref int __result)
        {
            BulkPolicyButtons.EnsureHeaderSpace(ref __result);
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_DrugPolicy), nameof(PawnColumnWorker_DrugPolicy.DoHeader))]
    internal static class DrugHeaderPatch
    {
        private static void Prefix(Rect rect)
        {
            BulkPolicyButtons.PrepareHeader(rect, "BulkOutfitPolicy_Drug", OpenMenu);
        }

        private static void OpenMenu(List<Pawn> pawns)
        {
            BulkPolicyButtons.OpenPolicyMenu(
                pawns,
                Current.Game.drugPolicyDatabase.AllPolicies,
                policy => policy.label,
                (pawn, policy) =>
                {
                    if (pawn.drugs != null)
                    {
                        pawn.drugs.CurrentPolicy = policy;
                    }
                });
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_DrugPolicy), nameof(PawnColumnWorker_DrugPolicy.GetMinHeaderHeight))]
    internal static class DrugHeaderHeightPatch
    {
        private static void Postfix(ref int __result)
        {
            BulkPolicyButtons.EnsureHeaderSpace(ref __result);
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_Reading), nameof(PawnColumnWorker_Reading.DoHeader))]
    internal static class ReadingHeaderPatch
    {
        private static void Prefix(Rect rect)
        {
            BulkPolicyButtons.PrepareHeader(rect, "BulkOutfitPolicy_Reading", OpenMenu);
            Rect gearRect = new Rect(rect.xMax + 6f, rect.y + 4f, 26f, 26f);
            if (Widgets.ButtonImage(gearRect, TexButton.OpenInspectSettings))
            {
                Find.WindowStack.Add(new DefaultPolicyAssignmentsDialog());
            }

            TooltipHandler.TipRegion(gearRect, "BulkOutfitPolicy_DefaultsTooltip".Translate());
        }

        private static void OpenMenu(List<Pawn> pawns)
        {
            BulkPolicyButtons.OpenPolicyMenu(
                pawns,
                Current.Game.readingPolicyDatabase.AllReadingPolicies,
                policy => policy.label,
                (pawn, policy) =>
                {
                    if (pawn.reading != null)
                    {
                        pawn.reading.CurrentPolicy = policy;
                    }
                });
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_Reading), nameof(PawnColumnWorker_Reading.GetMinHeaderHeight))]
    internal static class ReadingHeaderHeightPatch
    {
        private static void Postfix(ref int __result)
        {
            BulkPolicyButtons.EnsureHeaderSpace(ref __result);
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.SetFaction))]
    internal static class PawnFactionChangedPatch
    {
        private static void Prefix(Thing __instance, out Faction __state)
        {
            __state = __instance.Faction;
        }

        private static void Postfix(Thing __instance, Faction newFaction, Faction __state)
        {
            if (__state == Faction.OfPlayer || newFaction != Faction.OfPlayer || !(__instance is Pawn pawn))
            {
                return;
            }

            DefaultPolicyAssignments.TryApply(pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
    internal static class PawnGuestStatusChangedPatch
    {
        private struct GuestState
        {
            public GuestStatus status;
            public Faction hostFaction;
            public Faction pawnFaction;
        }

        [HarmonyPatch(typeof(PawnComponentsUtility), nameof(PawnComponentsUtility.AddAndRemoveDynamicComponents))]
        internal static class PawnComponentsInitializedPatch
        {
            private static void Postfix(Pawn pawn)
            {
                DefaultPolicyAssignments.ApplyPending(pawn);
            }
        }

        private static void Prefix(Pawn_GuestTracker __instance, Pawn ___pawn, out GuestState __state)
        {
            __state = new GuestState
            {
                status = __instance.guestStatusInt,
                hostFaction = __instance.HostFaction,
                pawnFaction = ___pawn.Faction
            };
        }

        private static void Postfix(Pawn_GuestTracker __instance, Pawn ___pawn, GuestState __state)
        {
            if (__state.status == __instance.guestStatusInt &&
                __state.hostFaction == __instance.HostFaction &&
                __state.pawnFaction == ___pawn.Faction)
            {
                return;
            }

            DefaultPolicyAssignments.TryApply(___pawn);
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_PawnTable), nameof(MainTabWindow_PawnTable.DoWindowContents))]
    internal static class AssignWindowSelectionLayoutPatch
    {
        private static bool previousMultiselect;
        private static bool initialized;

        private static void Prefix(MainTabWindow_PawnTable __instance)
        {
            if (!(__instance is MainTabWindow_Assign))
            {
                return;
            }

            bool currentMultiselect = BulkPolicyButtons.HasMultipleColonistsSelected();
            if (initialized && currentMultiselect == previousMultiselect)
            {
                return;
            }

            initialized = true;
            previousMultiselect = currentMultiselect;
            __instance.Notify_PawnsChanged();
        }
    }
}
