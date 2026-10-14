using System.Linq;
using RimWorld;
using Verse;

namespace BulkOutfitPolicy
{
    public sealed class DefaultPolicyAssignments : GameComponent
    {
        public FoodPolicy ColonistFood;
        public FoodPolicy PrisonerFood;
        public FoodPolicy SlaveFood;
        public FoodPolicy GuestFood;
        public MedicalCareCategory ColonistMedicalCare;
        public MedicalCareCategory PrisonerMedicalCare;
        public MedicalCareCategory SlaveMedicalCare;
        public MedicalCareCategory GuestMedicalCare;
        private bool medicalCareDefaultsInitialized;

        public DefaultPolicyAssignments(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref ColonistFood, "colonistFood");
            Scribe_References.Look(ref PrisonerFood, "prisonerFood");
            Scribe_References.Look(ref SlaveFood, "slaveFood");
            Scribe_References.Look(ref GuestFood, "guestFood");
            Scribe_Values.Look(ref ColonistMedicalCare, "colonistMedicalCare", Find.PlaySettings.defaultCareForColonist);
            Scribe_Values.Look(ref PrisonerMedicalCare, "prisonerMedicalCare", Find.PlaySettings.defaultCareForPrisoner);
            Scribe_Values.Look(ref SlaveMedicalCare, "slaveMedicalCare", Find.PlaySettings.defaultCareForSlave);
            Scribe_Values.Look(ref GuestMedicalCare, "guestMedicalCare", Find.PlaySettings.defaultCareForFriendlyFaction);
            Scribe_Values.Look(ref medicalCareDefaultsInitialized, "medicalCareDefaultsInitialized", false);
        }

        private static readonly System.Collections.Generic.HashSet<Pawn> pendingAssignments =
            new System.Collections.Generic.HashSet<Pawn>();

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            InitializeDefaults();
        }

        public void InitializeDefaults()
        {
            FoodRestrictionDatabase foodDatabase = Current.Game.foodRestrictionDatabase;
            FoodPolicy defaultFood = foodDatabase.DefaultFoodRestriction();

            if (ColonistFood == null || !foodDatabase.AllFoodRestrictions.Contains(ColonistFood))
            {
                ColonistFood = defaultFood;
            }
            if (PrisonerFood == null || !foodDatabase.AllFoodRestrictions.Contains(PrisonerFood))
            {
                PrisonerFood = defaultFood;
            }
            if (SlaveFood == null || !foodDatabase.AllFoodRestrictions.Contains(SlaveFood))
            {
                SlaveFood = defaultFood;
            }
            if (GuestFood == null || !foodDatabase.AllFoodRestrictions.Contains(GuestFood))
            {
                GuestFood = defaultFood;
            }

            if (medicalCareDefaultsInitialized)
            {
                return;
            }

            ColonistMedicalCare = Find.PlaySettings.defaultCareForColonist;
            PrisonerMedicalCare = Find.PlaySettings.defaultCareForPrisoner;
            SlaveMedicalCare = Find.PlaySettings.defaultCareForSlave;
            GuestMedicalCare = Find.PlaySettings.defaultCareForFriendlyFaction;
            medicalCareDefaultsInitialized = true;
        }

        public static void TryApply(Pawn pawn)
        {
            if (pawn == null || Current.Game == null)
            {
                return;
            }

            DefaultPolicyAssignments assignments = Current.Game.GetComponent<DefaultPolicyAssignments>();
            if (assignments == null)
            {
                return;
            }

            FoodPolicy foodPolicy = null;
            MedicalCareCategory medicalCare = MedicalCareCategory.Best;

            if (pawn.IsPrisonerOfColony)
            {
                foodPolicy = assignments.PrisonerFood;
                medicalCare = assignments.PrisonerMedicalCare;
            }
            else if (ModsConfig.IdeologyActive && pawn.IsSlaveOfColony)
            {
                foodPolicy = assignments.SlaveFood;
                medicalCare = assignments.SlaveMedicalCare;
            }
            else if (ModsConfig.RoyaltyActive &&
                     pawn.guest != null &&
                     pawn.guest.HostFaction == Faction.OfPlayer)
            {
                foodPolicy = assignments.GuestFood;
                medicalCare = assignments.GuestMedicalCare;
            }
            else if (pawn.IsColonist && pawn.Faction == Faction.OfPlayer)
            {
                foodPolicy = assignments.ColonistFood;
                medicalCare = assignments.ColonistMedicalCare;
            }
            else
            {
                return;
            }

            if (pawn.foodRestriction == null || pawn.playerSettings == null)
            {
                pendingAssignments.Add(pawn);
                return;
            }

            pendingAssignments.Remove(pawn);
            if (foodPolicy != null &&
                Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Contains(foodPolicy))
            {
                pawn.foodRestriction.CurrentFoodPolicy = foodPolicy;
            }

            pawn.playerSettings.medCare = medicalCare;
        }

        public static void ApplyPending(Pawn pawn)
        {
            if (pendingAssignments.Contains(pawn))
            {
                TryApply(pawn);
            }
        }
    }
}
