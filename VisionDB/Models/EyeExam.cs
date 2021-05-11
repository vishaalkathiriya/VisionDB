using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using VisionDB.Controllers;
using VisionDB.Helper;

namespace VisionDB.Models
{
    public class EyeExam
    {
        [Required]
        public Guid Id { get; set; }

        public virtual ApplicationUser CreatedByUser { get; set; }

        public DateTime UpdatedTimestamp { get; set; }

        [Required]
        [DisplayName("Appointment Date")]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime TestDateAndTime { get; set; }

        public string TestDateAndTimeToString 
        {
            get
            {
                return string.Concat(TestDateAndTime.ToShortDateString(), " ", TestDateAndTime.ToShortTimeString());
            }
        }

        public virtual Customer customer { get; set; }

        public float? RSphericalDist { get; set; }
        public float? LSphericalDist { get; set; }
        public float? RCylDist { get; set; }
        public float? LCylDist { get; set; }
        public float? RAxisDist { get; set; }
        public float? LAxisDist { get; set; }
        public float? RPrismDistH { get; set; }
        public float? LPrismDistH { get; set; }
        public float? RPrismDistV { get; set; }
        public float? LPrismDistV { get; set; }
        public Enums.BaseHorizontal? RBaseDistH { get; set; }
        public Enums.BaseHorizontal? LBaseDistH { get; set; }
        public Enums.BaseVertical? RBaseDistV { get; set; }
        public Enums.BaseVertical? LBaseDistV { get; set; }
        public float? RSphericalNear { get; set; }
        public float? LSphericalNear { get; set; }
        public float? RCylNear { get; set; }
        public float? LCylNear { get; set; }
        public float? RAxisNear { get; set; }
        public float? LAxisNear { get; set; }
        public float? RPrismNearH { get; set; }
        public float? LPrismNearH { get; set; }
        public float? RPrismNearV { get; set; }
        public float? LPrismNearV { get; set; }
        public Enums.BaseHorizontal? RBaseNearH { get; set; }
        public Enums.BaseHorizontal? LBaseNearH { get; set; }
        public Enums.BaseVertical? RBaseNearV { get; set; }
        public Enums.BaseVertical? LBaseNearV { get; set; }
        public float? RVision { get; set; }
        public float? LVision { get; set; }
        public int? RDistVA { get; set; }
        public int? LDistVA { get; set; }
        public int? RNearVA { get; set; }
        public int? LNearVA { get; set; }

        public override string ToString()
        {
            if (LSphericalDist != null && RSphericalDist != null)
            {
                return string.Format("{0}: R Sph Dist {1} / L Sph Dist {2}", appointmentCategory.ToString().Replace("_", " "), NumericHelper.GetSignedResult(RSphericalDist), NumericHelper.GetSignedResult(LSphericalDist)) + (IsExternalPrescription ? " (external prescription)" : "");
            }
            else
            {
                return appointmentCategory.ToString().Replace("_", " ") + (IsExternalPrescription ? " (external prescription)" : "");
            }
        }

        public string ToStringEyeExam 
        { 
            get
            {
                return TestDateAndTime.ToShortDateString() + " " + ToString();
            }
        }

        public string Notes { get; set; }

        public string RFV { get; set; }

        public string GH { get; set; }

        public string POH { get; set; }

        public string FH { get; set; }

        [DisplayName("Medications")]
        public string MEDS { get; set; }

        public string Allergies { get; set; }

        [DisplayName("Appointment Category")]
        public Enums.AppointmentCategory appointmentCategory { get; set; }

        [DisplayName("Appointment Room")]
        public string AppointmentRoom { get; set; }

        [DisplayName("NHS or Private")]
        public string NHSPrivate { get; set; }

        [DisplayName("NHS Reason")]
        public string NHSReason { get; set; }

        [DisplayName("NHS Voucher")]
        public string NHSVoucher { get; set; }

        [DisplayName("Cover test distance")]
        public string CoverTestDistance { get; set; }

        [DisplayName("Cover test near")]
        public string CoverTestNear { get; set; }

        [DisplayName("Convergence distance")]
        public string ConvergenceDistance { get; set; }

        [DisplayName("Convergence near")]
        public string ConvergenceNear { get; set; }

        [DisplayName("Motility distance")]
        public string MotilityDistance { get; set; }

        [DisplayName("Motility near")]
        public string MotilityNear { get; set; }

        [DisplayName("Accommodation distance")]
        public string AccommodationDistance { get; set; }

        [DisplayName("Accommodation near")]
        public string AccommodationNear { get; set; }

        [DisplayName("Muscle balance FD distance")]
        public string MuscleBalanceFDDistance { get; set; }

        [DisplayName("Muscle balance FD near")]
        public string MuscleBalanceFDNear { get; set; }

        [DisplayName("Keratometry distance")]
        public string KeratometryDistance { get; set; }

        [DisplayName("Keratometry near")]
        public string KeratometryNear { get; set; }

        [DisplayName("Retinoscopy right")]
        public string RetinoscopyRight { get; set; }

        [DisplayName("Retinoscopy left")]
        public string RetinoscopyLeft { get; set; }

        [DisplayName("Visual fields")]
        public string VisualFields { get; set; }

        [DisplayName("Amsler")]
        public string Amsler { get; set; }

        [DisplayName("Colour vision")]
        public string ColourVision { get; set; }

        [DisplayName("Stereo vision")]
        public string StereoVision { get; set; }

        [DisplayName("Other")]
        public string Other { get; set; }

        [DisplayName("D vision right")]
        public string DVisionRight { get; set; }

        [DisplayName("D vision left")]
        public string DVisionLeft { get; set; }

        [DisplayName("N vision Right")]
        public string NVision { get; set; }

        [DisplayName("N visionLeft")]
        public string NVisionLeft { get; set; }

        [DisplayName("Sph right")]
        public string SphRight { get; set; }

        [DisplayName("Sph left")]
        public string SphLeft { get; set; }

        [DisplayName("Cyl right")]
        public string CylRight { get; set; }

        [DisplayName("Cyl left")]
        public string CylLeft { get; set; }

        [DisplayName("Axis right")]
        public string AxisRight { get; set; }

        [DisplayName("Axis left")]
        public string AxisLeft { get; set; }

        [DisplayName("Prism right")]
        public string PrismRight { get; set; }

        [DisplayName("Prism left")]
        public string PrismLeft { get; set; }

        [DisplayName("VA right")]
        public string VARight { get; set; }

        [DisplayName("VA left")]
        public string VALeft { get; set; }

        [DisplayName("Bin VA")]
        public string BinVA { get; set; }

        [DisplayName("Bin VA Left")]
        public string BinVALeft { get; set; }

        [DisplayName("Bin NVA Left")]
        public string BinNVALeft { get; set; }

        [DisplayName("Bin NVA Left")]
        public string BinNVARight { get; set; }

        [DisplayName("I add right")]
        public float? IAddRight { get; set; }

        [DisplayName("I add left")]
        public float? IAddLeft { get; set; }

        [DisplayName("N add right")]
        public float? NAddRight { get; set; }

        [DisplayName("N add left")]
        public float? NAddLeft { get; set; }

        [DisplayName("Prism right")]
        public string Prism2Right { get; set; }

        [DisplayName("Prism left")]
        public string Prism2Left { get; set; }

        [DisplayName("NVA")]
        public string NVA { get; set; }

        [DisplayName("NVA Left")]
        public string NVALeft { get; set; }

        [DisplayName("Advice and recall")]
        [DataType(DataType.MultilineText)]
        public string AdviceAndRecall { get; set; }

        [DisplayName("DispenseSRight")]
        public string DispenseSRight { get; set; }

        [DisplayName("DispenseCRight")]
        public string DispenseCRight { get; set; }

        [DisplayName("DispenseARight")]
        public string DispenseARight { get; set; }

        [DisplayName("DispensePrismRight")]
        public string DispensePrismRight { get; set; }

        [DisplayName("DispenseAddRight")]
        public string DispenseAddRight { get; set; }

        [DisplayName("DispenseLensRight")]
        public string DispenseLensRight { get; set; }

        [DisplayName("DispenseOCRight")]
        public string DispenseOCRight { get; set; }

        [DisplayName("DispenseHeightAboveRight")]
        public string DispenseHeightAboveRight { get; set; }

        [DisplayName("DispenseSLeft")]
        public string DispenseSLeft { get; set; }

        [DisplayName("DispenseCLeft")]
        public string DispenseCLeft { get; set; }

        [DisplayName("DispenseALeft")]
        public string DispenseALeft { get; set; }

        [DisplayName("DispensePrismLeft")]
        public string DispensePrismLeft { get; set; }

        [DisplayName("DispenseAddLeft")]
        public string DispenseAddLeft { get; set; }

        [DisplayName("DispenseLensLeft")]
        public string DispenseLensLeft { get; set; }

        [DisplayName("DispenseOCLeft")]
        public string DispenseOCLeft { get; set; }

        [DisplayName("DispenseHeightAboveLeft")]
        public string DispenseHeightAboveLeft { get; set; }

        [DisplayName("DispenseFrameDetails")]
        public string DispenseFrameDetails { get; set; }

        [DisplayName("DispenseFrame")]
        public string DispenseFrame { get; set; }

        [DisplayName("DispenseFramePrice")]
        public string DispenseFramePrice { get; set; }

        [DisplayName("DispenseLens")]
        public string DispenseLens { get; set; }

        [DisplayName("DispenseLensPrice")]
        public string DispenseLensPrice { get; set; }

        [DisplayName("DispenseEyeExamination")]
        public string DispenseEyeExamination { get; set; }

        [DisplayName("DispenseEyeExaminationPrice")]
        public string DispenseEyeExaminationPrice { get; set; }

        [DisplayName("DispenseLessVoucher")]
        public string DispenseLessVoucher { get; set; }

        [DisplayName("DispenseTotal")]
        public string DispenseTotal { get; set; }

        [DisplayName("DispenseDeposit")]
        public string DispenseDeposit { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("DispenseDepositDate")]
        public DateTime? DispenseDepositDate { get; set; }

        [DisplayName("DispenseOutstandingBalance")]
        public string DispenseOutstandingBalance { get; set; }

        [DisplayName("DispenseOutstandingBalanceDate")]
        public string DispenseOutstandingBalanceDate { get; set; }

        [DisplayName("CLFitExistingWearer")]
        public string CLFitExistingWearer { get; set; }

        [DisplayName("Wearing Details")]
        public string CLFitPreviousWearingDetails { get; set; }

        [DisplayName("Fit Type")]
        public string CLFitType { get; set; }

        [DisplayName("Wearing Time")]
        public string CLFitWearingTime { get; set; }

        [DisplayName("Solutions")]
        public string CLFitSolutionsUsed { get; set; }

        [DisplayName("Problems")]
        public string CLFitCurrentPreviousProblems { get; set; }

        [DisplayName("CLFitTrialComments")]
        public string CLFitTrialComments { get; set; }

        [DisplayName("CLFitTrialOptometrist")]
        public string CLFitTrialOptometrist { get; set; }

        [DisplayName("CLFitWearingSchedule")]
        public string CLFitWearingSchedule { get; set; }

        [DisplayName("CLFitCleaningRegime")]
        public string CLFitCleaningRegime { get; set; }

        [DisplayName("CLFitDOHFormCompleted")]
        public string CLFitDOHFormCompleted { get; set; }

        [DisplayName("CLFitCollectionLensesIn")]
        public string CLFitCollectionLensesIn { get; set; }

        [DisplayName("CLFitCollectionWearingTime")]
        public string CLFitCollectionWearingTime { get; set; }

        [DisplayName("CLFitCollectionAdvice")]
        public string CLFitCollectionAdvice { get; set; }

        [DisplayName("CLFitCollectionNextAppointment")]
        public string CLFitCollectionNextAppointment { get; set; }

        [DisplayName("CLFitCollectionOptometrist")]
        public string CLFitCollectionOptometrist { get; set; }

        public string lidslashesleft { get; set; }

        public string cornealeft { get; set; }

        public string acleft { get; set; }

        public string vitreousleft { get; set; }

        public string lensleft { get; set; }

        public string disccdratioleft { get; set; }

        public string discnrrleft { get; set; }

        public string disccolourleft { get; set; }

        public string marginsleft { get; set; }

        public string vesselsleft { get; set; }

        public string avratioleft { get; set; }

        public string macularleft { get; set; }

        public string peripheryleft { get; set; }

        public string pupilsleft { get; set; }

        public string lidslashesright { get; set; }

        public string cornearight { get; set; }

        public string acright { get; set; }

        public string vitreousright { get; set; }

        public string lensright { get; set; }

        public string disccdratioright { get; set; }

        public string discnrrright { get; set; }

        public string disccolourright { get; set; }

        public string marginsright { get; set; }

        public string vesselsright { get; set; }

        public string avratioright { get; set; }

        public string macularright { get; set; }

        public string peripheryright { get; set; }

        public string pupilsright { get; set; }



        public string CTDist { get; set; }
        public string CTNear { get; set; }
        public string PhoriaD { get; set; }
        public string PhoriaN { get; set; }
        public string NPC { get; set; }
        public string IOPR1 { get; set; }
        public string IOPR2 { get; set; }
        public string IOPR3 { get; set; }
        public string IOPR4 { get; set; }
        public string IOPR5 { get; set; }
        public string IOPL1 { get; set; }
        public string IOPL2 { get; set; }
        public string IOPL3 { get; set; }
        public string IOPL4 { get; set; }
        public string IOPL5 { get; set; }
        public string VfieldsRight { get; set; }
        public string VfieldsLeft { get; set; }
        public Enums.YesNo? PupilRAPD { get; set; }
        public string ConfrontationRight { get; set; }
        public string ConfrontationLeft { get; set; }
        public Enums.YesNo? Dialated { get; set; }
        public string DrugUsed { get; set; }
        public string TimeDrugUsed { get; set; }
        public string DrugExpiry { get; set; }
        public string DrugBatch { get; set; }
        public string IOPRAvg { get; set; }
        public string IOPLAvg { get; set; }
        public string IOPTime { get; set; }
        public string VisualFieldsR { get; set; }
        public string AmslerR { get; set; }
        public string ColourVisionR { get; set; }
        public string StereoVisionR { get; set; }
        public string OtherR { get; set; }



        public string PupilDiameterL { get; set; }
        public string PupilDiameterR { get; set; }
        public string UpperLidR { get; set; }
        public string UpperLidL { get; set; }
        public string CorneaL { get; set; }
        public string CorneaR { get; set; }
        public string LowerLidR { get; set; }
        public string LowerLidL { get; set; }
        public string MeibomianGlandsL { get; set; }
        public string MeibomianGlandsR { get; set; }
        public string LimbalAppearanceR { get; set; }
        public string LimbalAppearanceL { get; set; }
        public string CounjunctivalAppaeranceL { get; set; }
        public string CounjunctivalAppaeranceR { get; set; }
        public string TearQualityR { get; set; }
        public string TearQualityL { get; set; }
        public string SpecificationR { get; set; }
        public string SpecificationL { get; set; }
        public string FittingCentrationL { get; set; }
        public string FittingCentrationR { get; set; }
        public string MovementR { get; set; }
        public string MovementL { get; set; }
        public string CLSphL { get; set; }
        public string CLSphR { get; set; }
        public string CLCyl { get; set; }
        public string CLCylLeft { get; set; }
        public string CLAxisR { get; set; }
        public string CLAxisL { get; set; }

        [DisplayName("Cleaning Regime")]
        public string CleaningRegime { get; set; }

        public string CLVaR { get; set; }
        public string CLVaL { get; set; }
        public string CLOverReactionL { get; set; }
        public string CLOverReactionR { get; set; }
        public string CLBestVaR { get; set; }
        public string CLBestVaL { get; set; }
        public Enums.YesNo? AppointmentComplete { get; set; }

        public string ConjunctivaRight { get; set; }
        public string ConjunctivaLeft { get; set; }
        public string IrisRight { get; set; }
        public string IrisLeft { get; set; }
        public Enums.YesNo? anisocoria { get; set; }

        public float? CLBozrRight { get; set; }
        public float? CLTdRight { get; set; }
        public string CLBinVARight { get; set; }
        public float? CLBozrLeft { get; set; }
        public float? CLTdLeft { get; set; }
        public string CLBinVALeft { get; set; }


        public string ManufacturerLeft { get; set; }
        public string ManufacturerRight { get; set; }
        public string LensNameLeft { get; set; }
        public string LensNameRight { get; set; }
        public Enums.LensType? LensTypeLeft { get; set; }
        public Enums.LensType? LensTypeRight { get; set; }

        public string RetDvisionR { get; set; }
        public string RetDvisionL { get; set; }
        public string RetNvisionR { get; set; }
        public string RetNvisionL { get; set; }
        public float? RetSphereR { get; set; }
        public float? RetSphereL { get; set; }
        public float? RetCylR { get; set; }
        public float? RetCylL { get; set; }
        public float? RetAxisR { get; set; }
        public float? RetAxisL { get; set; }
        public string TbutL { get; set; }
        public string TbutR { get; set; }
        public string SplitACR { get; set; }
        public string SplitACL { get; set; }
        public string CLNotes { get; set; }
        public Enums.YesNo? InsertionRemoval { get; set; }
        public string ReasonforCLApp { get; set; }

        public string CLCondofLensLeft { get; set; }
        public string CLCondofLensRight { get; set; }
        public string CLLagRight { get; set; }
        public string CLLagLeft { get; set; }
        public string CLToricRotationLeft { get; set; }
        public string CLToricRotationRight { get; set; }
        public string CLCondofLensLeft2 { get; set; }
        public string CLCondofLensRight2 { get; set; }
        public string CLLagRight2 { get; set; }
        public string CLLagLeft2 { get; set; }
        public string CLToricRotationLeft2 { get; set; }
        public string CLToricRotationRight2 { get; set; }


        [DataType(DataType.DateTime)]
        [DisplayName("Last test date")]
        public DateTime? LastTestDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Next test date")]
        public DateTime? NextTestDueDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Contact Lens Due Date")]
        public DateTime? ContactLensDueDate { get; set; }

        public Enums.AdviceAndRecallList? AdviceAndRecallType { get; set; }
        public string FirstLensType { get; set; }
        public string SecondLensType { get; set; }

        public DateTime? Deleted { get; set; }

        public virtual ApplicationUser DeletedByUser { get; set; }

        public virtual ApplicationUser Optician { get; set; }

        public string SubRVisionD { get; set; }
        public string SubRVisionN { get; set; }
        public string SubLVisionD { get; set; }
        public string SubLVisionN { get; set; }
        public float? SubRSph { get; set; }
        public float? SubLSph { get; set; }
        public float? SubRCyl { get; set; }
        public float? SubLCyl { get; set; }
        public float? SubRAxis { get; set; }
        public float? SubLAxis { get; set; }
        public float? SubRPrismD { get; set; }
        public float? SubLPrismD { get; set; }
        public string SubRVA { get; set; }
        public string SubLVA { get; set; }
        public string SubRBinVA { get; set; }
        public string SubLBinVA { get; set; }
        public float? SubRIAdd { get; set; }
        public float? SubLIAdd { get; set; }
        public float? SubRNAdd { get; set; }
        public float? SubLNAdd { get; set; }
        public string SubRNVA { get; set; }
        public string SubLNVA { get; set; }
        public string SubRBinNVA { get; set; }
        public string SubLBinNVA { get; set; }
        public float? SubRPrismN { get; set; }
        public float? SubLPrismN { get; set; }

        [DisplayName("BVD right")]
        public float? BVDRight { get; set; }

        [DisplayName("BVD left")]
        public float? BVDLeft { get; set; }

        [DisplayName("PD right")]
        public float? PDRight { get; set; }

        [DisplayName("PD left")]
        public float? PDLeft { get; set; }

        [DisplayName("Near PD right")]
        public float? PDRightNear { get; set; }

        [DisplayName("Near PD left")]
        public float? PDLeftNear { get; set; }

        [DisplayName("Height right")]
        public float? RHeight { get; set; }

        [DisplayName("Height left")]
        public float? LHeight { get; set; }

        public string OphthalmoscopyMethodUsed { get; set; }
        public string OphthalmoscopyRDisc { get; set; }
        public string OphthalmoscopyLDisc { get; set; }
        public string OphthalmoscopyRNRR { get; set; }
        public string OphthalmoscopyLNRR { get; set; }
        public string OphthalmoscopyRCDRatio { get; set; }
        public string OphthalmoscopyLCDRatio { get; set; }
        public string OphthalmoscopyRVessels { get; set; }
        public string OphthalmoscopyLVessels { get; set; }
        public string OphthalmoscopyRPeriphery { get; set; }
        public string OphthalmoscopyLPeriphery { get; set; }
        public string OphthalmoscopyRMacula { get; set; }
        public string OphthalmoscopyLMacula { get; set; }
        public string OphthalmoscopyDescription { get; set; }

        [DisplayName("Patient Advice")]
        public string PatientAdvice { get; set; }

        [DisplayName("Product Recommendations")]
        public string ProductRecommendations { get; set; }

        [DisplayName("Referral Sent")]
        public bool ReferralSent { get; set; }

        [DisplayName("Symptoms and History")]
        public string SymptomsAndHistory { get; set; }

        public Enums.ObjectiveMethod ObjectiveMethodEnum { get; set; }
        
        [NotMapped]
        public EyeExam LastEyeExam { get; set; }

        [NotMapped]
        public EyeExam LastContactLensExam { get; set; }

        public string Dipl { get; set; }

        [DisplayName("H/A")]
        public string HA { get; set; }

        [DisplayName("F/F")]
        public string FF { get; set; }

        [DisplayName("External optician")]
        public string ExternalOpticianName { get; set; }

        [DisplayName("External practice")]
        public string ExternalPracticeName { get; set; }

        [DisplayName("Right lens")]
        public string ExternalEyeLensRight { get; set; }

        [DisplayName("Left lens")]
        public string ExternalEyeLensLeft { get; set; }

        [DisplayName("Right PH")]
        public string ExternalEyePinholeRight { get; set; }

        [DisplayName("Left PH")]
        public string ExternalEyePinholeLeft { get; set; }
        public bool IsExternalPrescription 
        { 
            get
            {
                return ExternalOpticianName != null || ExternalOpticianName != null ? true : false;
            }
        }

        public List<IOPViewModel> IOPs { get; set; }

        public string LastEyeExamTestDateAndTimeToString 
        {
            get
            {
                return LastEyeExam != null ? LastEyeExam.TestDateAndTimeToString : "";
            }
        }

        public string LastContactLensExamTestDateAndTimeToString
        {
            get
            {
                return LastContactLensExam != null ? LastContactLensExam.TestDateAndTimeToString : "";
            }
        }
    }
}