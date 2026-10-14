using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BulkOutfitPolicy
{
    public sealed class DefaultPolicyAssignmentsDialog : Window
    {
        private const float CategoryWidth = 150f;
        private const float ColumnGap = 10f;
        private const float RowHeight = 45f;
        private const float WindowChromeAndFooterHeight = 180f;

        public DefaultPolicyAssignmentsDialog()
        {
            doCloseButton = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(
            720f,
            WindowChromeAndFooterHeight + VisibleCategoryCount * RowHeight);

        public override void DoWindowContents(Rect inRect)
        {
            DefaultPolicyAssignments assignments = Current.Game.GetComponent<DefaultPolicyAssignments>();
            if (assignments == null)
            {
                Log.Error("Bulk Policies & Defaults could not find its game settings component.");
                return;
            }

            assignments.InitializeDefaults();

            Text.Font = GameFont.Medium;
            Widgets.Label(
                new Rect(0f, 0f, inRect.width, 36f),
                "BulkOutfitPolicy_DefaultsTitle".Translate());
            Text.Font = GameFont.Small;

            float foodX = CategoryWidth + ColumnGap;
            float careX = foodX + (inRect.width - foodX - ColumnGap) / 2f + ColumnGap;
            float policyWidth = careX - foodX - ColumnGap;
            float headerY = 44f;

            Widgets.Label(
                new Rect(foodX, headerY, policyWidth, 30f),
                "BulkOutfitPolicy_Food".Translate());
            Widgets.Label(
                new Rect(careX, headerY, policyWidth, 30f),
                "BulkOutfitPolicy_MedicalCare".Translate());

            float rowY = headerY + 32f;
            DrawRow(
                rowY,
                foodX,
                careX,
                policyWidth,
                "BulkOutfitPolicy_Colonists".Translate(),
                assignments.ColonistFood,
                Current.Game.foodRestrictionDatabase.AllFoodRestrictions,
                policy => assignments.ColonistFood = policy,
                assignments.ColonistMedicalCare,
                category => assignments.ColonistMedicalCare = category);
            rowY += RowHeight;

            DrawRow(
                rowY,
                foodX,
                careX,
                policyWidth,
                "BulkOutfitPolicy_Prisoners".Translate(),
                assignments.PrisonerFood,
                Current.Game.foodRestrictionDatabase.AllFoodRestrictions,
                policy => assignments.PrisonerFood = policy,
                assignments.PrisonerMedicalCare,
                category => assignments.PrisonerMedicalCare = category);
            rowY += RowHeight;

            if (ModsConfig.IdeologyActive)
            {
                DrawRow(
                    rowY,
                    foodX,
                    careX,
                    policyWidth,
                    "BulkOutfitPolicy_Slaves".Translate(),
                    assignments.SlaveFood,
                    Current.Game.foodRestrictionDatabase.AllFoodRestrictions,
                    policy => assignments.SlaveFood = policy,
                    assignments.SlaveMedicalCare,
                    category => assignments.SlaveMedicalCare = category);
                rowY += RowHeight;
            }

            if (ModsConfig.RoyaltyActive)
            {
                DrawRow(
                    rowY,
                    foodX,
                    careX,
                    policyWidth,
                    "BulkOutfitPolicy_Guests".Translate(),
                    assignments.GuestFood,
                    Current.Game.foodRestrictionDatabase.AllFoodRestrictions,
                    policy => assignments.GuestFood = policy,
                    assignments.GuestMedicalCare,
                    category => assignments.GuestMedicalCare = category);
            }
        }

        private static int VisibleCategoryCount =>
            2 + (ModsConfig.IdeologyActive ? 1 : 0) + (ModsConfig.RoyaltyActive ? 1 : 0);

        private static void DrawRow(
            float y,
            float foodX,
            float careX,
            float policyWidth,
            string category,
            FoodPolicy selectedFoodPolicy,
            IEnumerable<FoodPolicy> foodPolicies,
            Action<FoodPolicy> setFoodPolicy,
            MedicalCareCategory selectedMedicalCare,
            Action<MedicalCareCategory> setMedicalCare)
        {
            Widgets.Label(new Rect(0f, y, CategoryWidth, RowHeight), category);
            DrawFoodPolicyButton(
                new Rect(foodX, y, policyWidth, 32f),
                selectedFoodPolicy,
                foodPolicies,
                setFoodPolicy);
            DrawMedicalCareButton(
                new Rect(careX, y, policyWidth, 32f),
                selectedMedicalCare,
                setMedicalCare);
        }

        private static void DrawFoodPolicyButton(
            Rect rect,
            FoodPolicy selectedPolicy,
            IEnumerable<FoodPolicy> policies,
            Action<FoodPolicy> setPolicy)
        {
            if (!Widgets.ButtonText(rect, selectedPolicy.label.Truncate(rect.width)))
            {
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (FoodPolicy policy in policies)
            {
                FoodPolicy selected = policy;
                options.Add(new FloatMenuOption(selected.label, delegate { setPolicy(selected); }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void DrawMedicalCareButton(
            Rect rect,
            MedicalCareCategory selectedCategory,
            Action<MedicalCareCategory> setCategory)
        {
            if (!Widgets.ButtonText(rect, selectedCategory.GetLabel().CapitalizeFirst()))
            {
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (MedicalCareCategory category in Enum.GetValues(typeof(MedicalCareCategory)))
            {
                MedicalCareCategory selected = category;
                options.Add(new FloatMenuOption(
                    selected.GetLabel().CapitalizeFirst(),
                    delegate { setCategory(selected); }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
