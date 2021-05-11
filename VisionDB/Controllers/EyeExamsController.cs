using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using VisionDB.Helper;
using VisionDB.Models;
using MvcReportViewer;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Globalization;

namespace VisionDB.Controllers
{
    [Authorize]
    public class EyeExamsController : VisionDBController
    {
        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        #region Shared

        public List<EyeExam> GetEyeExams(Customer customer)
        {
            CustomersDataContext db = new CustomersDataContext();

            var results = from row in db.EyeExams
                          select row;

            if (customer != null)
            {
                results = results.Where(c => c.customer.Id == customer.Id);
            }
            else
            {
                results = results.Where(c => false);
            }

            return results.ToList();
        }

        private static void CopyEyeExam(ref EyeExam From, ref EyeExam To)
        {
            To.AccommodationDistance = From.AccommodationDistance;
            To.AccommodationNear = From.AccommodationNear;
            To.acleft = From.acleft;
            To.acright = From.acright;
            To.AdviceAndRecall = From.AdviceAndRecall;
            To.AdviceAndRecallType = From.AdviceAndRecallType;
            To.Amsler = From.Amsler;
            To.AmslerR = From.AmslerR;
            To.anisocoria = From.anisocoria;
            To.AppointmentComplete = From.AppointmentComplete;
            To.AppointmentRoom = From.AppointmentRoom;
            To.appointmentCategory = From.appointmentCategory;
            To.avratioleft = From.avratioleft;
            To.avratioright = From.avratioright;
            To.AxisLeft = From.AxisLeft;
            To.AxisRight = From.AxisRight;
            To.BinNVALeft = From.BinNVALeft;
            To.BinNVARight = From.BinNVARight;
            To.BinVA = From.BinVA;
            To.BinVALeft = From.BinVALeft;
            To.BVDLeft = From.BVDLeft;
            To.BVDRight = From.BVDRight;
            To.CLAxisL = From.CLAxisL;
            To.CLAxisR = From.CLAxisR;
            To.CLBestVaL = From.CLBestVaL;
            To.CLBestVaR = From.CLBestVaR;
            To.CLBinVALeft = From.CLBinVALeft;
            To.CLBinVARight = From.CLBinVARight;
            To.CLBozrLeft = From.CLBozrLeft;
            To.CLBozrRight = From.CLBozrRight;
            To.CLTdLeft = From.CLTdLeft;
            To.CLTdRight = From.CLTdRight;
            To.CLCondofLensLeft = From.CLCondofLensLeft;
            To.CLCondofLensLeft2 = From.CLCondofLensLeft2;
            To.CLCondofLensRight = From.CLCondofLensRight;
            To.CLCondofLensRight2 = From.CLCondofLensRight2;
            To.CLCyl = From.CLCyl;
            To.CLCylLeft = From.CLCylLeft;
            To.CleaningRegime = From.CleaningRegime;
            To.CLFitCleaningRegime = From.CLFitCleaningRegime;
            To.CLFitCollectionAdvice = From.CLFitCollectionAdvice;
            To.CLFitCollectionLensesIn = From.CLFitCollectionLensesIn;
            To.CLFitCollectionNextAppointment = From.CLFitCollectionNextAppointment;
            To.CLFitCollectionOptometrist = From.CLFitCollectionOptometrist;
            To.CLFitCollectionWearingTime = From.CLFitCollectionWearingTime;
            To.CLFitCurrentPreviousProblems = From.CLFitCurrentPreviousProblems;
            To.CLFitDOHFormCompleted = From.CLFitDOHFormCompleted;
            To.CLFitExistingWearer = From.CLFitExistingWearer;
            To.CLFitPreviousWearingDetails = From.CLFitPreviousWearingDetails;
            To.CLFitSolutionsUsed = From.CLFitSolutionsUsed;
            To.CLFitTrialComments = From.CLFitTrialComments;
            To.CLFitTrialOptometrist = From.CLFitTrialOptometrist;
            To.CLFitType = From.CLFitType;
            To.CLFitWearingSchedule = From.CLFitWearingSchedule;
            To.CLFitWearingTime = From.CLFitWearingTime;
            To.CLLagLeft = From.CLLagLeft;
            To.CLLagLeft2 = From.CLLagLeft2;
            To.CLLagRight = From.CLLagRight;
            To.CLLagRight2 = From.CLLagRight2;
            To.CLNotes = From.CLNotes;
            To.CLOverReactionL = From.CLOverReactionL;
            To.CLOverReactionR = From.CLOverReactionR;
            To.CLSphL = From.CLSphL;
            To.CLSphR = From.CLSphR;
            To.CLToricRotationLeft = From.CLToricRotationLeft;
            To.CLToricRotationLeft2 = From.CLToricRotationLeft2;
            To.CLToricRotationRight = From.CLToricRotationRight;
            To.CLToricRotationRight2 = From.CLToricRotationRight2;
            To.CLVaL = From.CLVaL;
            To.CLVaR = From.CLVaR;
            To.ColourVision = From.ColourVision;
            To.ColourVisionR = From.ColourVisionR;
            To.ConfrontationLeft = From.ConfrontationLeft;
            To.ConfrontationRight = From.ConfrontationRight;
            To.ConjunctivaLeft = From.ConjunctivaLeft;
            To.ConjunctivaRight = From.ConjunctivaRight;
            To.ContactLensDueDate = From.ContactLensDueDate;
            To.ConvergenceDistance = From.ConvergenceDistance;
            To.ConvergenceNear = From.ConvergenceNear;
            To.CorneaL = From.CorneaL;
            To.cornealeft = From.cornealeft;
            To.CorneaR = From.CorneaR;
            To.cornearight = From.cornearight;
            To.CounjunctivalAppaeranceL = From.CounjunctivalAppaeranceL;
            To.CounjunctivalAppaeranceR = From.CounjunctivalAppaeranceR;
            To.CoverTestDistance = From.CoverTestDistance;
            To.CoverTestNear = From.CoverTestNear;
            To.CreatedByUser = From.CreatedByUser;
            To.CTDist = From.CTDist;
            To.CTNear = From.CTNear;
            To.customer = From.customer;
            To.CylLeft = From.CylLeft;
            To.CylRight = From.CylRight;
            To.Dialated = From.Dialated;
            To.disccdratioleft = From.disccdratioleft;
            To.disccdratioright = From.disccdratioright;
            To.disccolourleft = From.disccolourleft;
            To.disccolourright = From.disccolourright;
            To.discnrrleft = From.discnrrleft;
            To.discnrrright = From.discnrrright;
            To.DispenseAddLeft = From.DispenseAddLeft;
            To.DispenseAddRight = From.DispenseAddRight;
            To.DispenseALeft = From.DispenseALeft;
            To.DispenseARight = From.DispenseARight;
            To.DispenseCLeft = From.DispenseCLeft;
            To.DispenseCRight = From.DispenseCRight;
            To.DispenseDeposit = From.DispenseDeposit;
            To.DispenseDepositDate = From.DispenseDepositDate;
            To.DispenseEyeExamination = From.DispenseEyeExamination;
            To.DispenseEyeExaminationPrice = From.DispenseEyeExaminationPrice;
            To.DispenseFrame = From.DispenseFrame;
            To.DispenseFrameDetails = From.DispenseFrameDetails;
            To.DispenseFramePrice = From.DispenseFramePrice;
            To.DispenseHeightAboveLeft = From.DispenseHeightAboveLeft;
            To.DispenseHeightAboveRight = From.DispenseHeightAboveRight;
            To.DispenseLens = From.DispenseLens;
            To.DispenseLensLeft = From.DispenseLensLeft;
            To.DispenseLensPrice = From.DispenseLensPrice;
            To.DispenseLensRight = From.DispenseLensRight;
            To.DispenseLessVoucher = From.DispenseLessVoucher;
            To.DispenseOCLeft = From.DispenseOCLeft;
            To.DispenseOCRight = From.DispenseOCRight;
            To.DispenseOutstandingBalance = From.DispenseOutstandingBalance;
            To.DispenseOutstandingBalanceDate = From.DispenseOutstandingBalanceDate;
            To.DispensePrismLeft = From.DispensePrismLeft;
            To.DispensePrismRight = From.DispensePrismRight;
            To.DispenseSLeft = From.DispenseSLeft;
            To.DispenseSRight = From.DispenseSRight;
            To.DispenseTotal = From.DispenseTotal;
            To.DrugBatch = From.DrugBatch;
            To.DrugExpiry = From.DrugExpiry;
            To.DrugUsed = From.DrugUsed;
            To.DVisionLeft = From.DVisionLeft;
            To.DVisionRight = From.DVisionRight;
            To.FH = From.FH;
            To.FirstLensType = From.FirstLensType;
            To.FittingCentrationL = From.FittingCentrationL;
            To.FittingCentrationR = From.FittingCentrationR;
            To.GH = From.GH;
            To.IAddLeft = From.IAddLeft;
            To.IAddRight = From.IAddRight;
            To.InsertionRemoval = From.InsertionRemoval;
            To.IOPL1 = From.IOPL1;
            To.IOPL2 = From.IOPL2;
            To.IOPL3 = From.IOPL3;
            To.IOPR4 = From.IOPR4;
            To.IOPR5 = From.IOPR5;
            To.IOPLAvg = From.IOPLAvg;
            To.IOPR1 = From.IOPR1;
            To.IOPR2 = From.IOPR2;
            To.IOPR3 = From.IOPR3;
            To.IOPL4 = From.IOPL4;
            To.IOPL5 = From.IOPL5;
            To.IOPRAvg = From.IOPRAvg;
            To.IOPTime = From.IOPTime;
            To.IrisLeft = From.IrisLeft;
            To.IrisRight = From.IrisRight;
            To.KeratometryDistance = From.KeratometryDistance;
            To.KeratometryNear = From.KeratometryNear;
            To.LastTestDate = From.LastTestDate;
            To.LAxisDist = From.LAxisDist;
            To.LAxisNear = From.LAxisNear;
            To.LBaseDistH = From.LBaseDistH;
            To.LBaseNearH = From.LBaseNearH;
            To.LBaseDistV = From.LBaseDistV;
            To.LBaseNearV = From.LBaseNearV;
            To.LCylDist = From.LCylDist;
            To.LCylNear = From.LCylNear;
            To.LDistVA = From.LDistVA;
            To.lensleft = From.lensleft;
            To.LensNameLeft = From.LensNameLeft;
            To.LensNameRight = From.LensNameRight;
            To.lensright = From.lensright;
            To.LensTypeLeft = From.LensTypeLeft;
            To.LensTypeRight = From.LensTypeRight;
            To.lidslashesleft = From.lidslashesleft;
            To.lidslashesright = From.lidslashesright;
            To.LimbalAppearanceL = From.LimbalAppearanceL;
            To.LimbalAppearanceR = From.LimbalAppearanceR;
            To.LNearVA = From.LNearVA;
            To.LowerLidL = From.LowerLidL;
            To.LowerLidR = From.LowerLidR;
            To.LPrismDistH = From.LPrismDistH;
            To.LPrismNearH = From.LPrismNearH;
            To.LPrismDistV = From.LPrismDistV;
            To.LPrismNearV = From.LPrismNearV;
            To.LSphericalDist = From.LSphericalDist;
            To.LSphericalNear = From.LSphericalNear;
            To.LVision = From.LVision;
            To.macularleft = From.macularleft;
            To.macularright = From.macularright;
            To.ManufacturerLeft = From.ManufacturerLeft;
            To.ManufacturerRight = From.ManufacturerRight;
            To.marginsleft = From.marginsleft;
            To.marginsright = From.marginsright;
            To.MEDS = From.MEDS;
            To.MeibomianGlandsL = From.MeibomianGlandsL;
            To.MeibomianGlandsR = From.MeibomianGlandsR;
            To.MotilityDistance = From.MotilityDistance;
            To.MotilityNear = From.MotilityNear;
            To.MovementL = From.MovementL;
            To.MovementR = From.MovementR;
            To.MuscleBalanceFDDistance = From.MuscleBalanceFDDistance;
            To.MuscleBalanceFDNear = From.MuscleBalanceFDNear;
            To.NAddLeft = From.NAddLeft;
            To.NAddRight = From.NAddRight;
            To.NextTestDueDate = From.NextTestDueDate;
            To.NHSPrivate = From.NHSPrivate;
            To.NHSReason = From.NHSReason;
            To.NHSVoucher = From.NHSVoucher;
            To.Notes = From.Notes;
            To.NPC = From.NPC;
            To.NVA = From.NVA;
            To.NVALeft = From.NVALeft;
            To.NVision = From.NVision;
            To.NVisionLeft = From.NVisionLeft;
            To.Other = From.Other;
            To.OtherR = From.OtherR;
            To.PDLeft = From.PDLeft;
            To.PDLeftNear = From.PDLeftNear;
            To.PDRight = From.PDRight;
            To.PDRightNear = From.PDRightNear;
            To.RHeight = From.RHeight;
            To.LHeight = From.LHeight;
            To.peripheryleft = From.peripheryleft;
            To.peripheryright = From.peripheryright;
            To.PhoriaD = From.PhoriaD;
            To.PhoriaN = From.PhoriaN;
            To.POH = From.POH;
            To.Prism2Left = From.Prism2Left;
            To.Prism2Right = From.Prism2Right;
            To.PrismLeft = From.PrismLeft;
            To.PrismRight = From.PrismRight;
            To.PupilDiameterL = From.PupilDiameterL;
            To.PupilDiameterR = From.PupilDiameterR;
            To.PupilRAPD = From.PupilRAPD;
            To.pupilsleft = From.pupilsleft;
            To.pupilsright = From.pupilsright;
            To.RAxisDist = From.RAxisDist;
            To.RAxisNear = From.RAxisNear;
            To.RBaseDistH = From.RBaseDistH;
            To.RBaseNearH = From.RBaseNearH;
            To.RBaseDistV = From.RBaseDistV;
            To.RBaseNearV = From.RBaseNearV;
            To.RCylDist = From.RCylDist;
            To.RCylNear = From.RCylNear;
            To.RDistVA = From.RDistVA;
            To.ReasonforCLApp = From.ReasonforCLApp;
            To.RetAxisL = From.RetAxisL;
            To.RetAxisR = From.RetAxisR;
            To.RetCylL = From.RetCylL;
            To.RetCylR = From.RetCylR;
            To.RetDvisionL = From.RetDvisionL;
            To.RetDvisionR = From.RetDvisionR;
            To.RetinoscopyLeft = From.RetinoscopyLeft;
            To.RetinoscopyRight = From.RetinoscopyRight;
            To.RetNvisionL = From.RetNvisionL;
            To.RetNvisionR = From.RetNvisionR;
            To.RetSphereL = From.RetSphereL;
            To.RetSphereR = From.RetSphereR;
            To.RFV = From.RFV;
            To.RNearVA = From.RNearVA;
            To.RPrismDistH = From.RPrismDistH;
            To.RPrismNearH = From.RPrismNearH;
            To.RPrismDistV = From.RPrismDistV;
            To.RPrismNearV = From.RPrismNearV;
            To.RSphericalDist = From.RSphericalDist;
            To.RSphericalNear = From.RSphericalNear;
            To.RVision = From.RVision;
            To.SecondLensType = From.SecondLensType;
            To.SpecificationL = From.SpecificationL;
            To.SpecificationR = From.SpecificationR;
            To.SphLeft = From.SphLeft;
            To.SphRight = From.SphRight;
            To.SplitACL = From.SplitACL;
            To.SplitACR = From.SplitACR;
            To.StereoVision = From.StereoVision;
            To.StereoVisionR = From.StereoVisionR;
            To.TbutL = From.TbutL;
            To.TbutR = From.TbutR;
            To.TearQualityL = From.TearQualityL;
            To.TearQualityR = From.TearQualityR;
            To.TimeDrugUsed = From.TimeDrugUsed;
            To.UpperLidL = From.UpperLidL;
            To.UpperLidR = From.UpperLidR;
            To.VALeft = From.VALeft;
            To.VARight = From.VARight;
            To.vesselsleft = From.vesselsleft;
            To.vesselsright = From.vesselsright;
            To.VfieldsLeft = From.VfieldsLeft;
            To.VfieldsRight = From.VfieldsRight;
            To.VisualFields = From.VisualFields;
            To.VisualFieldsR = From.VisualFieldsR;
            To.vitreousleft = From.vitreousleft;
            To.vitreousright = From.vitreousright;
            To.SubRVisionD = From.SubRVisionD;
            To.SubRVisionN = From.SubRVisionN;
            To.SubLVisionD = From.SubLVisionD;
            To.SubLVisionN = From.SubLVisionN;
            To.SubRSph = From.SubRSph;
            To.SubLSph = From.SubLSph;
            To.SubRCyl = From.SubRCyl;
            To.SubLCyl = From.SubLCyl;
            To.SubRAxis = From.SubRAxis;
            To.SubLAxis = From.SubLAxis;
            To.SubRPrismD = From.SubRPrismD;
            To.SubLPrismD = From.SubLPrismD;
            To.SubRVA = From.SubRVA;
            To.SubLVA = From.SubLVA;
            To.SubRBinVA = From.SubRBinVA;
            To.SubLBinVA = From.SubLBinVA;
            To.SubRIAdd = From.SubRIAdd;
            To.SubLIAdd = From.SubLIAdd;
            To.SubRNAdd = From.SubRNAdd;
            To.SubLNAdd = From.SubLNAdd;
            To.SubRNVA = From.SubRNVA;
            To.SubLNVA = From.SubLNVA;
            To.SubRBinNVA = From.SubRBinNVA;
            To.SubLBinNVA = From.SubLBinNVA;
            To.SubRPrismN = From.SubRPrismN;
            To.SubLPrismN = From.SubLPrismN;
            To.OphthalmoscopyMethodUsed = From.OphthalmoscopyMethodUsed;
            To.OphthalmoscopyRDisc = From.OphthalmoscopyRDisc;
            To.OphthalmoscopyLDisc = From.OphthalmoscopyLDisc;
            To.OphthalmoscopyRNRR = From.OphthalmoscopyRNRR;
            To.OphthalmoscopyLNRR = From.OphthalmoscopyLNRR;
            To.OphthalmoscopyRCDRatio = From.OphthalmoscopyRCDRatio;
            To.OphthalmoscopyLCDRatio = From.OphthalmoscopyLCDRatio;
            To.OphthalmoscopyRVessels = From.OphthalmoscopyRVessels;
            To.OphthalmoscopyLVessels = From.OphthalmoscopyLVessels;
            To.OphthalmoscopyRPeriphery = From.OphthalmoscopyRPeriphery;
            To.OphthalmoscopyLPeriphery = From.OphthalmoscopyLPeriphery;
            To.OphthalmoscopyRMacula = From.OphthalmoscopyRMacula;
            To.OphthalmoscopyLMacula = From.OphthalmoscopyLMacula;
            To.OphthalmoscopyDescription = From.OphthalmoscopyDescription;
            To.PatientAdvice = From.PatientAdvice;
            To.ProductRecommendations = From.ProductRecommendations;
            To.ReferralSent = From.ReferralSent;
            To.SymptomsAndHistory = From.SymptomsAndHistory;
            To.Allergies = From.Allergies;
            To.ObjectiveMethodEnum = From.ObjectiveMethodEnum;
            To.Dipl = From.Dipl;
            To.HA = From.HA;
            To.FF = From.FF;
            To.ExternalOpticianName = From.ExternalOpticianName;
            To.ExternalPracticeName = From.ExternalPracticeName;
            To.ExternalEyeLensRight = From.ExternalEyeLensRight;
            To.ExternalEyeLensLeft = From.ExternalEyeLensLeft;
            To.ExternalEyePinholeRight = From.ExternalEyePinholeRight;
            To.ExternalEyePinholeLeft = From.ExternalEyePinholeLeft;

            if (From.TestDateAndTime != DateTime.MinValue)
            {
                To.TestDateAndTime = From.TestDateAndTime;
            }
        }


        #endregion

        #region Eye Exam

        private void UpdateCachedDates(Models.EyeExam eyeExam)
        {
            if (eyeExam.appointmentCategory == Enums.AppointmentCategory.Eye_Exam)
            {
                eyeExam.customer.PreviousEyeExamDate = eyeExam.TestDateAndTime;
            }
            else if (eyeExam.appointmentCategory == Enums.AppointmentCategory.Contact_Lens_Exam)
            {
                eyeExam.customer.PreviousContactLensExamDate = eyeExam.TestDateAndTime;
            }

            CustomersDataContext db = new CustomersDataContext();
            if (GetLatestEyeExam(db, eyeExam.customer) != null)
            {
                eyeExam.customer.NextDueDateEyeExam = AddFrequency(GetLatestEyeExam(db, eyeExam.customer).TestDateAndTime, eyeExam.customer.EyeExamFrequencyUnit, (int)eyeExam.customer.EyeExamFrequencyValue);
            }
            else
            {
                eyeExam.customer.NextDueDateEyeExam = null;
            }

            if (GetLatestContactLensExam(eyeExam.customer) != null)
            {
                eyeExam.customer.NextDueDateContactLensExam = AddFrequency(GetLatestContactLensExam(eyeExam.customer).TestDateAndTime, eyeExam.customer.ContactLensExamFrequencyUnit, (int)eyeExam.customer.ContactLensExamFrequencyValue);
            }
            else
            {
                eyeExam.customer.NextDueDateContactLensExam = null;
            }

        }

        private static DateTime AddFrequency(DateTime Date, Enums.FrequencyUnit Frequency, int FrequencyValue)
        {
            if (Frequency == Enums.FrequencyUnit.Days)
            {
                return Date.AddDays(FrequencyValue);
            }
            else if (Frequency == Enums.FrequencyUnit.Months)
            {
                return Date.AddMonths(FrequencyValue);
            }
            else if (Frequency == Enums.FrequencyUnit.Years)
            {
                return Date.AddYears(FrequencyValue);
            }
            else
            {
                throw new ArgumentOutOfRangeException(string.Format("EyeExamFrequencyValue {0} is not supported", Frequency.ToString()));
            }
        }

        public ActionResult EyeExam(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            CustomersDataContext db = new CustomersDataContext();
            Practice practice = db.Practices.Find(practiceId);

            return RedirectToActionPermanent("EyeExam" + practice.EyeExamScreenDesign.ToString(), new { Id = Id });
        }

        [HttpPost]
        public ActionResult EditEyeExam1(EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string error = null;
            if (EyeExamFrequencyValue == null)
            {
                error = "The eye exam frequency is required";
                ModelState.AddModelError("EyeExamFrequencyValue", error);
            }

            if (error == null)
            {
                CustomersDataContext db;
                EyeExam existingEyeExam;
                eyeExam = ExistingEyeExam(eyeExam, EyeExamFrequencyValue, EyeExamFrequencyUnit, SelectedOptician, out db, out existingEyeExam);
                if (ModelState.IsValid)
                {
                    db.Entry(existingEyeExam).State = EntityState.Modified;
                    db.SaveChanges();
                    UpdateCachedDates(existingEyeExam);
                    existingEyeExam.customer.LastUpdated = DateTime.Now;
                    db.SaveChanges();
                    TempData["Message"] = "Eye exam saved";
                    return RedirectToAction("EyeExam", new { eyeExam.Id });
                }
            }

            ViewBag.Error = error;
            return EditEyeExam1(eyeExam.Id);
        }

        private Models.EyeExam ExistingEyeExam(EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician, out CustomersDataContext db, out EyeExam existingEyeExam)
        {
            db = new CustomersDataContext();
            existingEyeExam = db.EyeExams.Find(eyeExam.Id);
            CopyEyeExam(ref eyeExam, ref existingEyeExam);
            existingEyeExam.UpdatedTimestamp = DateTime.Now;
            existingEyeExam.customer.EyeExamFrequencyValue = EyeExamFrequencyValue;
            existingEyeExam.customer.EyeExamFrequencyUnit = EyeExamFrequencyUnit;
            existingEyeExam.appointmentCategory = Enums.AppointmentCategory.Eye_Exam;
            existingEyeExam.Optician = db.ApplicationUsers.Find(SelectedOptician);
            if (existingEyeExam.customer.PreviousEyeExamDate <= existingEyeExam.TestDateAndTime)
            {
                existingEyeExam.customer.SymptomsAndHistory = existingEyeExam.SymptomsAndHistory;
                existingEyeExam.customer.RFV = existingEyeExam.RFV;
                existingEyeExam.customer.GH = existingEyeExam.GH;
                existingEyeExam.customer.MEDS = existingEyeExam.MEDS;
                existingEyeExam.customer.POH = existingEyeExam.POH;
                existingEyeExam.customer.FH = existingEyeExam.FH;
                existingEyeExam.customer.Allergies = existingEyeExam.Allergies;
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            ModelState.Clear();
            return eyeExam;
        }

        #endregion

        #region Contact Lens Exam

        [HttpGet]
        public ActionResult AddContactLensExam()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Customer customer = ((Customer)HttpContext.Session["customer"]);
            CustomersDataContext db = new CustomersDataContext();
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            var validationResult = db.Entry(customer).GetValidationResult();

            if (!validationResult.IsValid)
            {
                TempData["Error"] = validationResult.ValidationErrors.ToList()[0].ErrorMessage;
                return RedirectToAction("Edit", "Customers", new { customer.Id });
            }
            ViewBag.Customer = customer;

            ViewBag.OpticiansViewModels = new AccountController().GetOpticianViewModels(practiceId);
            EyeExam eyeExam = new EyeExam();
            eyeExam.LastContactLensExam = GetLatestContactLensExam(customer);
            eyeExam.SymptomsAndHistory = customer.SymptomsAndHistory;
            eyeExam.RFV = customer.RFV;
            eyeExam.GH = customer.GH;
            eyeExam.MEDS = customer.MEDS;
            eyeExam.POH = customer.POH;
            eyeExam.FH = customer.FH;
            eyeExam.Allergies = customer.Allergies;
            eyeExam.TestDateAndTime = GetEyeExamDefaultDateAndTime(db, customer);
            return View(eyeExam);
        }

        [HttpPost]
        public ActionResult AddContactLensExam(Models.EyeExam eyeExam, int? ContactLensExamFrequencyValue, Enums.FrequencyUnit ContactLensExamFrequencyUnit, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string error = null;
            if (ContactLensExamFrequencyValue == null)
            {
                error = "The contact lens exam frequency is required";
                ModelState.AddModelError("ContactLensExamFrequencyValue", error);
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();

                if (eyeExam.Id == Guid.Empty)
                {
                    eyeExam.Id = Guid.NewGuid();
                }

                eyeExam.UpdatedTimestamp = DateTime.Now;
                eyeExam.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                eyeExam.customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
                eyeExam.customer.ContactLensExamFrequencyValue = ContactLensExamFrequencyValue;
                eyeExam.customer.ContactLensExamFrequencyUnit = ContactLensExamFrequencyUnit;
                eyeExam.appointmentCategory = Enums.AppointmentCategory.Contact_Lens_Exam;
                eyeExam.Optician = db.ApplicationUsers.Find(SelectedOptician);
                eyeExam.customer.RecallCount = 0;
                eyeExam.customer.LastUpdated = DateTime.Now;
                eyeExam.customer.SymptomsAndHistory = eyeExam.SymptomsAndHistory;
                eyeExam.customer.RFV = eyeExam.RFV;
                eyeExam.customer.GH = eyeExam.GH;
                eyeExam.customer.MEDS = eyeExam.MEDS;
                eyeExam.customer.POH = eyeExam.POH;
                eyeExam.customer.FH = eyeExam.FH;
                eyeExam.customer.Allergies = eyeExam.Allergies;
                db.EyeExams.Add(eyeExam);
                db.SaveChanges();
                UpdateCachedDates(eyeExam);
                db.SaveChanges();
                TempData["Message"] = "Contact lens exam saved";
                return RedirectToAction("ContactLensExam", new { eyeExam.Id });
            }

            ViewBag.Error = error;
            return AddContactLensExam();
        }

        public ActionResult ContactLensExam(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            var eyeExam = db.EyeExams.Find(Id);
            ViewBag.Customer = db.Customers.Find(eyeExam.customer.Id);

            return View(eyeExam);
        }

        [HttpGet]
        public ActionResult EditContactLensExam(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            CustomersDataContext db = new CustomersDataContext();

            var eyeExam = db.EyeExams.Find(Id);
            ViewBag.Customer = db.Customers.Find(eyeExam.customer.Id);
            ViewBag.OpticiansViewModels = new AccountController().GetOpticianViewModels(practiceId);
            return View(eyeExam);
        }

        [HttpPost]
        public ActionResult EditContactLensExam(EyeExam eyeExam, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            EyeExam existingEyeExam = db.EyeExams.Find(eyeExam.Id);
            CopyEyeExam(ref eyeExam, ref existingEyeExam);
            existingEyeExam.UpdatedTimestamp = DateTime.Now;
            existingEyeExam.appointmentCategory = Enums.AppointmentCategory.Contact_Lens_Exam;
            existingEyeExam.Optician = db.ApplicationUsers.Find(SelectedOptician);
            if (existingEyeExam.customer.PreviousContactLensExamDate <= existingEyeExam.TestDateAndTime)
            {
                existingEyeExam.customer.SymptomsAndHistory = existingEyeExam.SymptomsAndHistory;
                existingEyeExam.customer.RFV = existingEyeExam.RFV;
                existingEyeExam.customer.GH = existingEyeExam.GH;
                existingEyeExam.customer.MEDS = existingEyeExam.MEDS;
                existingEyeExam.customer.POH = existingEyeExam.POH;
                existingEyeExam.customer.FH = existingEyeExam.FH;
                existingEyeExam.customer.Allergies = existingEyeExam.Allergies;
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                db.Entry(existingEyeExam).State = EntityState.Modified;
                db.SaveChanges();
                UpdateCachedDates(existingEyeExam);
                existingEyeExam.customer.LastUpdated = DateTime.Now;
                db.SaveChanges();
                TempData["Message"] = "Contact lens exam saved";
                return RedirectToAction("ContactLensExam", new { eyeExam.Id });
            }

            return EditContactLensExam(eyeExam.Id);
        }

        #endregion

        /// <summary>
        /// Delete any type of eye exam inc contact lens exam
        /// </summary>
        /// <param name="eyeExam"></param>
        /// <returns></returns>
        public ActionResult Delete(EyeExam eyeExam)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                CustomersDataContext db = new CustomersDataContext();
                EyeExam existingEyeExam = db.EyeExams.Find(eyeExam.Id);
                existingEyeExam.Deleted = DateTime.Now;
                existingEyeExam.DeletedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
                db.SaveChanges();
                UpdateCachedDates(existingEyeExam);
                db.SaveChanges();
                TempData["Message"] = existingEyeExam.appointmentCategory == Enums.AppointmentCategory.Eye_Exam ? "Eye exam deleted" : "Contact lens exam deleted";
                return RedirectToAction("Customer", "Customers", new { ((Customer)HttpContext.Session["customer"]).Id });
            }
            else
            {
                //todo: show error
                return View();
            }
        }

        public EyeExam GetLatestEyeExam(CustomersDataContext db, Customer customer)
        {
            List<EyeExam> eyeExams = db.EyeExams.Where(e => 
                e.customer.Id == customer.Id 
                && e.appointmentCategory == Enums.AppointmentCategory.Eye_Exam 
                && e.Deleted == null).ToList();
            if (eyeExams.Count > 0)
            {
                return eyeExams.OrderByDescending(e => e.TestDateAndTime).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public EyeExam GetLatestEyeExam(CustomersDataContext db, Customer customer, DateTime ExcludeAfter)
        {
            ExcludeAfter = ExcludeAfter.Date.AddDays(1).AddMinutes(-1);
            List<EyeExam> eyeExams = db.EyeExams.Where(e => 
                e.customer.Id == customer.Id 
                && e.appointmentCategory == Enums.AppointmentCategory.Eye_Exam 
                && e.Deleted == null
                && e.TestDateAndTime <= ExcludeAfter
                ).ToList();

            if (eyeExams.Count > 0)
            {
                return eyeExams.OrderByDescending(e => e.TestDateAndTime).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public EyeExam GetLatestContactLensExam(Customer customer)
        {
            return new CustomersDataContext().EyeExams.Where(e => e.customer.Id == customer.Id && e.appointmentCategory == Enums.AppointmentCategory.Contact_Lens_Exam && e.Deleted == null).OrderByDescending(e => e.TestDateAndTime).FirstOrDefault();
        }

        #region New Eye Exam

        [HttpGet]
        public ActionResult AddEyeExam()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);
            return RedirectToActionPermanent("AddEyeExam" + practice.EyeExamScreenDesign.ToString());
        }

        public ActionResult AddEyeExamForCustomer(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();
            HttpContext.Session["customer"] = db.Customers.Find(Id);
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            Practice practice = db.Practices.Find(practiceId);

            return RedirectToActionPermanent("AddEyeExam" + practice.EyeExamScreenDesign.ToString());
        }

        #endregion

        private string Validate(Models.EyeExam eyeExam, string error)
        {
            string pattern = @"\b[0-9!-/:-@\[-`\{-~].*(/|\.).*";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            if (eyeExam.SubRVA != null && eyeExam.SubRVA.Length > 0 && !rgx.IsMatch(eyeExam.SubRVA))
            {
                error = "Invalid value for " + "SubRVA";
                ModelState.AddModelError("SubRVA", error);
            }
            return error;
        }

        public ActionResult GOSFormExport(Guid Id, string GOSForm)
        {
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            return this.Report(
                ReportFormat.PDF,
                new VisionDB.Controllers.ReportsController().GetReportPath(practiceId, GOSForm),
                new { EyeExamId = Id });
        }

        public ActionResult EyeExam1(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            EyeExam eyeExam = db.EyeExams.Find(Id);
            ViewBag.Customer = db.Customers.Find(eyeExam.customer.Id);

            return View(eyeExam);
        }

        public ActionResult EyeExam2(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            EyeExam eyeExam = db.EyeExams.Find(Id);
            ViewBag.Customer = db.Customers.Find(eyeExam.customer.Id);

            return View(eyeExam);
        }

        public ActionResult EyeExam3(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            CustomersDataContext db = new CustomersDataContext();

            EyeExam eyeExam = db.EyeExams.Find(Id);
            ViewBag.Customer = db.Customers.Find(eyeExam.customer.Id);

            return View(eyeExam);
        }

        [HttpGet]
        public ActionResult EditEyeExam1(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            CustomersDataContext db = new CustomersDataContext();

            var eyeExam = db.EyeExams.Find(Id);
            PopulateEyeExamForEdit(eyeExam, db, practiceId);
            return View(eyeExam);
        }

        public ActionResult EditEyeExam2(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            CustomersDataContext db = new CustomersDataContext();

            var eyeExam = db.EyeExams.Find(Id);
            PopulateEyeExamForEdit(eyeExam, db, practiceId);
            return View(eyeExam);
        }

        [HttpPost]
        public ActionResult EditEyeExam2(EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string error = null;
            if (EyeExamFrequencyValue == null)
            {
                error = "The eye exam frequency is required";
                ModelState.AddModelError("EyeExamFrequencyValue", error);
            }

            if (error == null)
            {
                CustomersDataContext db;
                EyeExam existingEyeExam;
                eyeExam = ExistingEyeExam(eyeExam, EyeExamFrequencyValue, EyeExamFrequencyUnit, SelectedOptician, out db, out existingEyeExam);
                if (ModelState.IsValid)
                {
                    db.Entry(existingEyeExam).State = EntityState.Modified;
                    db.SaveChanges();
                    UpdateCachedDates(existingEyeExam);
                    existingEyeExam.customer.LastUpdated = DateTime.Now;
                    db.SaveChanges();
                    TempData["Message"] = "Eye exam saved";
                    return RedirectToAction("EyeExam", new { eyeExam.Id });
                }
            }

            ViewBag.Error = error;
            return EditEyeExam2(eyeExam.Id);
        }

        public ActionResult EditEyeExam3(Guid Id)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;
            CustomersDataContext db = new CustomersDataContext();

            var eyeExam = db.EyeExams.Find(Id);
            PopulateEyeExamForEdit(eyeExam, db, practiceId);
            return View(eyeExam);
        }

        private void PopulateEyeExamForEdit(EyeExam eyeExam, CustomersDataContext db, Guid practiceId)
        {
            ViewBag.Customer = db.Customers.Find(eyeExam.customer.Id);
            ViewBag.OpticiansViewModels = new AccountController().GetOpticianViewModels(practiceId);
            List<IOP> iops = db.IOPs.Where(i =>
                    i.eyeExam.Id == eyeExam.Id && i.Deleted == null).OrderBy(i => i.Added).ToList();

            List<IOPViewModel> iopViewModels = new List<IOPViewModel>();

            foreach (IOP iop in iops)
            {
                iopViewModels.Add(new IOPViewModel
                {
                    Id = iop.Id,
                    Added = iop.Added,
                    eye = iop.eye,
                    Value1 = iop.Value1,
                    Value2 = iop.Value2,
                    Value3 = iop.Value3,
                    Value4 = iop.Value4,
                    Value5 = iop.Value5,
                    Average = iop.Average
                });
            }

            Session["iops"] = iopViewModels;
        }

        [HttpPost]
        public ActionResult EditEyeExam3(EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string error = null;
            if (EyeExamFrequencyValue == null)
            {
                error = "The eye exam frequency is required";
                ModelState.AddModelError("EyeExamFrequencyValue", error);
            }

            if (error == null)
            {
                CustomersDataContext db;
                EyeExam existingEyeExam;
                eyeExam = ExistingEyeExam(eyeExam, EyeExamFrequencyValue, EyeExamFrequencyUnit, SelectedOptician, out db, out existingEyeExam);
                if (ModelState.IsValid)
                {
                    db.Entry(existingEyeExam).State = EntityState.Modified;
                    db.SaveChanges();
                    SaveIOPs(existingEyeExam, db);
                    UpdateCachedDates(existingEyeExam);
                    existingEyeExam.customer.LastUpdated = DateTime.Now;
                    db.SaveChanges();
                    TempData["Message"] = "Eye exam saved";
                    return RedirectToAction("EyeExam", new { eyeExam.Id });
                }
            }

            ViewBag.Error = error;
            return EditEyeExam3(eyeExam.Id);
        }

        private void SaveIOPs(EyeExam eyeExam, CustomersDataContext db)
        {
            foreach (IOPViewModel iop in ((List<IOPViewModel>)Session["iops"]))
            {
                IOP existingIOP = db.IOPs.Find(iop.Id);
                if (existingIOP != null)
                {
                    existingIOP.Value1 = iop.Value1;
                    existingIOP.Value2 = iop.Value2;
                    existingIOP.Value3 = iop.Value3;
                    existingIOP.Value4 = iop.Value4;
                    existingIOP.Value5 = iop.Value5;
                    existingIOP.Average = iop.Average;
                    existingIOP.Deleted = iop.Deleted;
                    existingIOP.DeletedByUser = eyeExam.CreatedByUser;
                }
                else
                {
                    db.IOPs.Add(new IOP
                    {
                        Id = iop.Id,
                        Added = iop.Added,
                        eye = iop.eye,
                        eyeExam = eyeExam,
                        Value1 = iop.Value1,
                        Value2 = iop.Value2,
                        Value3 = iop.Value3,
                        Value4 = iop.Value4,
                        Value5 = iop.Value5,
                        Average = iop.Average,
                        Deleted = iop.Deleted,
                        DeletedByUser = eyeExam.CreatedByUser
                    });
                }
            }
        }

        private ActionResult NewEyeExam()
        {
            CustomersDataContext db = new CustomersDataContext();

            Guid practiceId = ((ApplicationUser)HttpContext.Session["user"]).practiceId;

            Customer customer = ((Customer)HttpContext.Session["customer"]);

            var validationResult = db.Entry(customer).GetValidationResult();

            if (!validationResult.IsValid)
            {
                TempData["Error"] = validationResult.ValidationErrors.ToList()[0].ErrorMessage;
                return RedirectToAction("Edit", "Customers", new { customer.Id });
            }

            ViewBag.Customer = customer;

            ViewBag.OpticiansViewModels = new AccountController().GetOpticianViewModels(practiceId);
            EyeExam eyeExam = new EyeExam();
            eyeExam.LastEyeExam = GetLatestEyeExam(db, customer);
            eyeExam.SymptomsAndHistory = customer.SymptomsAndHistory;
            eyeExam.RFV = customer.RFV;
            eyeExam.GH = customer.GH;
            eyeExam.MEDS = customer.MEDS;
            eyeExam.POH = customer.POH;
            eyeExam.FH = customer.FH;
            eyeExam.Allergies = customer.Allergies;
            eyeExam.TestDateAndTime = GetEyeExamDefaultDateAndTime(db, customer);
            
            return View(eyeExam);
        }

        [HttpGet]
        public ActionResult AddEyeExam1()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return NewEyeExam();
        }

        [HttpGet]
        public ActionResult AddEyeExam2()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return NewEyeExam();
        }

        public DateTime GetEyeExamDefaultDateAndTime(CustomersDataContext db, Customer customer)
        {
            DateTime cutOffDateStart = DateTime.Now.Date;
            DateTime cutOffDateEnd = DateTime.Now.Date.AddDays(1);
            Appointment appointment = db.Appointments.Where(a => a.customer.Id == customer.Id && a.Deleted == null && a.Start > cutOffDateStart && a.Start < cutOffDateEnd).OrderByDescending(a => a.Start).FirstOrDefault();
            if (appointment != null && customer.practice.DefaultEyeExamTimeToPatientsAppointment)
            {
                return appointment.Start;
            }
            else
            {
                return DateTime.Now;
            }
        }

        [HttpPost]
        public ActionResult AddEyeExam1(Models.EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string error = null;
            if (EyeExamFrequencyValue == null)
            {
                error = "The eye exam frequency is required";
                ModelState.AddModelError("EyeExamFrequencyValue", error);
            }

            error = Validate(eyeExam, error);

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                return SaveNewEyeExam(eyeExam, EyeExamFrequencyValue, EyeExamFrequencyUnit, SelectedOptician);
            }

            ViewBag.Error = error;
            return AddEyeExam1();
        }

        private ActionResult SaveNewEyeExam(Models.EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician)
        {
            CustomersDataContext db = new CustomersDataContext();

            if (eyeExam.Id == Guid.Empty)
            {
                eyeExam.Id = Guid.NewGuid();
            }

            eyeExam.UpdatedTimestamp = DateTime.Now;
            eyeExam.CreatedByUser = db.ApplicationUsers.Find(((ApplicationUser)HttpContext.Session["user"]).Id);
            eyeExam.customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
            eyeExam.customer.EyeExamFrequencyValue = EyeExamFrequencyValue;
            eyeExam.customer.EyeExamFrequencyUnit = EyeExamFrequencyUnit;
            eyeExam.appointmentCategory = Enums.AppointmentCategory.Eye_Exam;
            eyeExam.Optician = db.ApplicationUsers.Find(SelectedOptician);
            eyeExam.customer.RecallCount = 0;
            eyeExam.customer.LastUpdated = DateTime.Now;
            eyeExam.customer.SymptomsAndHistory = eyeExam.SymptomsAndHistory;
            eyeExam.customer.RFV = eyeExam.RFV;
            eyeExam.customer.GH = eyeExam.GH;
            eyeExam.customer.MEDS = eyeExam.MEDS;
            eyeExam.customer.POH = eyeExam.POH;
            eyeExam.customer.FH = eyeExam.FH;
            eyeExam.customer.Allergies = eyeExam.Allergies;
            db.EyeExams.Add(eyeExam);
            db.SaveChanges();
            UpdateCachedDates(eyeExam);

            if (Session["iops"] != null)
            {
                foreach (IOPViewModel iop in ((List<IOPViewModel>)Session["iops"]))
                {
                    db.IOPs.Add(new IOP
                    {
                        Id = iop.Id,
                        Added = iop.Added,
                        eye = iop.eye,
                        eyeExam = eyeExam,
                        Value1 = iop.Value1,
                        Value2 = iop.Value2,
                        Value3 = iop.Value3,
                        Value4 = iop.Value4,
                        Value5 = iop.Value5,
                        Average = iop.Average,
                        Deleted = iop.Deleted,
                        DeletedByUser = eyeExam.CreatedByUser
                    });
                }
            }

            DateTime startDate = eyeExam.TestDateAndTime.Date;
            DateTime endDate = eyeExam.TestDateAndTime.Date.AddDays(1).AddMinutes(-1);

            Appointment appointment = db.Appointments.Where(a => a.customer.Id == eyeExam.customer.Id && a.StatusEnum != Enums.AppointmentStatus.Arrived && a.Deleted == null && a.Start > startDate && a.Start < endDate).FirstOrDefault();
            if (appointment != null)
            {
                appointment.StatusEnum = Enums.AppointmentStatus.Arrived;
            }

            db.SaveChanges();
            TempData["Message"] = "Eye exam saved";
            return RedirectToAction("EyeExam", new { eyeExam.Id });
        }

        [HttpPost]
        public ActionResult AddEyeExam2(Models.EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string error = null;
            if (EyeExamFrequencyValue == null)
            {
                error = "The eye exam frequency is required";
                ModelState.AddModelError("EyeExamFrequencyValue", error);
            }

            error = Validate(eyeExam, error);

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                return SaveNewEyeExam(eyeExam, EyeExamFrequencyValue, EyeExamFrequencyUnit, SelectedOptician);
            }

            ViewBag.Error = error;
            return AddEyeExam2();
        }

        [HttpGet]
        public ActionResult AddEyeExam3()
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Session["iops"] = new List<IOPViewModel>();
            return NewEyeExam();
        }

        [HttpPost]
        public ActionResult AddEyeExam3(Models.EyeExam eyeExam, int? EyeExamFrequencyValue, Enums.FrequencyUnit EyeExamFrequencyUnit, string SelectedOptician)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string error = null;
            if (EyeExamFrequencyValue == null)
            {
                error = "The eye exam frequency is required";
                ModelState.AddModelError("EyeExamFrequencyValue", error);
            }

            error = Validate(eyeExam, error);

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                return SaveNewEyeExam(eyeExam, EyeExamFrequencyValue, EyeExamFrequencyUnit, SelectedOptician);
            }

            ViewBag.Error = error;
            return AddEyeExam2();
        }

        public ActionResult _ReadIOPs([DataSourceRequest] DataSourceRequest request, Enums.Eye eye, Guid? eyeExamIdToExclude, Guid? eyeExamIdToInclude)
        {
            if (HttpContext.Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (eyeExamIdToExclude != null && eyeExamIdToInclude != null)
            {
                AddAuditLogEntry((ApplicationUser)HttpContext.Session["user"], Enums.AuditLogEntryType.Eye_Exam_Error, Helper.ErrorHelper.GetErrorText("Unable to get IOPs", "113"), eyeExamIdToInclude, false);
                eyeExamIdToExclude = null;
                eyeExamIdToInclude = null;
            }

            CustomersDataContext db = new CustomersDataContext();
            Customer customer = db.Customers.Find(((Customer)HttpContext.Session["customer"]).Id);
            List<IOP> IOPs = null;

            if (eyeExamIdToExclude != null)
            {
                IOPs = db.IOPs.Where(i =>
                    i.eyeExam.customer.Id == customer.Id && i.Deleted == null && i.eye == eye
                    && i.eyeExam.Id != eyeExamIdToExclude).OrderBy(i => i.Added).ToList();
            }
            else if (eyeExamIdToInclude != null)
            {
                IOPs = db.IOPs.Where(i =>
                    i.eyeExam.customer.Id == customer.Id && i.Deleted == null && i.eye == eye
                    && i.eyeExam.Id == eyeExamIdToInclude).OrderBy(i => i.Added).ToList();
            }
            else
            {
                IOPs = db.IOPs.Where(i =>
                    i.eyeExam.customer.Id == customer.Id && i.Deleted == null && i.eye == eye).OrderBy(i => i.Added).ToList();
            }

            List<IOPViewModel> IOPViewModels = new List<IOPViewModel>();
            foreach (IOP iop in IOPs)
            {
                IOPViewModels.Add(new IOPViewModel
                {
                    Id = iop.Id,
                    Added = iop.Added,
                    Value1 = iop.Value1,
                    Value2 = iop.Value2,
                    Value3 = iop.Value3,
                    Value4 = iop.Value4,
                    Value5 = iop.Value5,
                    Average = iop.Average,
                    eye = iop.eye
                });
            }
            return Json(IOPViewModels.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult AddIOPR([DataSourceRequest] DataSourceRequest request, IOPViewModel iopViewModel, string Mode, string Added)
        {
            CustomersDataContext db = new CustomersDataContext();
            List<IOPViewModel> iopViewModels = ((List<IOPViewModel>)Session["iops"]);

            if (Mode != "Create")
            {
                iopViewModel.Id = Guid.NewGuid();
                iopViewModel.Added = DateHelper.ParseDate(Added);
                iopViewModel.eye = Enums.Eye.Right;

                iopViewModels.Add(iopViewModel);
            }
            else
            {
                throw new ApplicationException("Error adding IOP. Contact support.");
            }

            return Json(new[] { iopViewModel }.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult AddIOPL([DataSourceRequest] DataSourceRequest request, IOPViewModel iopViewModel, string Mode, string Added)
        {
            CustomersDataContext db = new CustomersDataContext();
            List<IOPViewModel> iopViewModels = ((List<IOPViewModel>)Session["iops"]);

            if (Mode != "Create")
            {
                iopViewModel.Id = Guid.NewGuid();
                iopViewModel.Added = DateHelper.ParseDate(Added);
                iopViewModel.eye = Enums.Eye.Left;

                iopViewModels.Add(iopViewModel);
            }
            else
            {
                throw new ApplicationException("Error adding IOP. Contact support.");
            }

            return Json(new[] { iopViewModel }.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateIOP([DataSourceRequest] DataSourceRequest request, IOPViewModel iopViewModel, string Added)
        {
            if (iopViewModel != null)
            {
                foreach (IOPViewModel existingIOP in ((List<IOPViewModel>)Session["iops"]))
                {
                    if (existingIOP.Id == iopViewModel.Id)
                    {
                        existingIOP.Added = DateHelper.ParseDate(Added);
                        existingIOP.Value1 = iopViewModel.Value1;
                        existingIOP.Value2 = iopViewModel.Value2;
                        existingIOP.Value3 = iopViewModel.Value3;
                        existingIOP.Value4 = iopViewModel.Value4;
                        existingIOP.Value5 = iopViewModel.Value5;
                        existingIOP.Average = iopViewModel.Average;
                    }
                }
            }

            return Json(new[] { iopViewModel }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DeleteIOP([DataSourceRequest] DataSourceRequest request, IOPViewModel iopViewModel)
        {
            if (iopViewModel != null)
            {
                foreach (IOPViewModel existingIOP in ((List<IOPViewModel>)Session["iops"]))
                {
                    if (existingIOP.Id == iopViewModel.Id)
                    {
                        existingIOP.Deleted = DateTime.Now;
                        existingIOP.DeletedByUserId = new Guid(((ApplicationUser)HttpContext.Session["user"]).Id);
                    }
                }
            }

            return Json(new[] { iopViewModel }.ToDataSourceResult(request, ModelState));
        }
    }
}
