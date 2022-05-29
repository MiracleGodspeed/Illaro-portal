using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Abundance_Nk.Model.Model;
using System.ComponentModel.DataAnnotations;
using Abundance_Nk.Business;
using Abundance_Nk.Web.Models;
using System.Web.Mvc;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Web.Areas.Applicant.ViewModels
{
    public class PostJAMBFormPaymentViewModel
    {
        public PostJAMBFormPaymentViewModel()
        {
            Programme = new Programme();
            AppliedCourse = new AppliedCourse();
            AppliedCourse.Programme = new Programme();
            AppliedCourse.Department = new Department();
            remitaPayment = new RemitaPayment();

            Person = new Person();
            Person.State = new State();
            StateSelectList = Utility.PopulateStateSelectListItem();
            ProgrammeSelectListItem = Utility.PopulateProgrammeSelectListItem();
            ApplicationProgrammeSelectListItem = Utility.PopulateApplicationProgrammeSelectListItem();
        }

        public AppliedCourse AppliedCourse { get; set; }
        public ApplicantJambDetail ApplicantJambDetail { get; set; }
        public Department Department { get; set; }
        public RemitaPayment remitaPayment { get; set; }
        [Display(Name = "JAMB Reg. No")]
        public string JambRegistrationNumber { get; set; }
        public Programme Programme { get; set; }
        
        public List<SelectListItem> StateSelectList { get; set; }
        public List<SelectListItem> ProgrammeSelectListItem { get; set; }
        public List<SelectListItem> ApplicationProgrammeSelectListItem { get; set; }
        public List<SelectListItem> DepartmentSelectListItem { get; set; }
        public List<SelectListItem> DepartmentOptionSelectListItem { get; set; }
        public FeeType FeeType { get; set; }
        public Person Person { get; set; }
        public decimal Amount { get; set; }
        public Session CurrentSession { get; set; }
        public PaymentType PaymentType { get; set; }
        public Payment Payment { get; set; }
        public ApplicationFormSetting ApplicationFormSetting { get; set; }
        public ApplicationProgrammeFee ApplicationProgrammeFee { get; set; }
        public PaymentEtranzactType PaymentEtranzactType { get; set; }
        public Remita Remita { get; internal set; }
        public string Hash { get; internal set; }
        public bool IsAdmitted { get; set; } = false;
        public bool IsJambLoaded { get; set; } = false;

        public void Initialise()
        {
            try
            {
                //CurrentSession = GetCurrentSession();
                
                if(Programme.Id==(int)Programmes.DrivingCertificate || Programme.Id == (int)Programmes.CiscoCertificate)
                {
                    SessionLogic sessionLogic = new SessionLogic();
                    CurrentSession=sessionLogic.GetModelBy(f => f.Session_Id == (int)Sessions._20202021);
                }
                else
                {
                    CurrentSession = GetApplicationSession();
                }
                //CurrentSession = new Session(){Id = (int)Sessions._20172018};

                if (CurrentSession != null && Programme.Id > 0)
                {
                    FeeType = GetFeeTypeBy(CurrentSession, Programme);
                    ApplicationFormSetting = GetApplicationFormSettingBy(CurrentSession);

                    Level level = null;
                    if (Programme.Id == (int)Programmes.NDFullTime || Programme.Id == (int)Programmes.NDPartTime || Programme.Id == (int)Programmes.NDDistance)
                    {
                        level = new Level(){Id = (int)Levels.NDI};
                    }
                    else
                    {
                        level = new Level() { Id = (int)Levels.HNDI };
                    }

                    PaymentMode paymentMode = new PaymentMode(){Id = (int)PaymentModes.Full};

                    //PaymentEtranzactType = GetPaymentTypeBy(FeeType, Programme, level, paymentMode, CurrentSession);
                    ApplicationProgrammeFee = GetApplicationProgrammeFeeBy(Programme, FeeType, CurrentSession);
                    if (ApplicationFormSetting != null)
                    {
                        PaymentType = ApplicationFormSetting.PaymentType;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private ApplicationProgrammeFee GetApplicationProgrammeFeeBy(Programme Programme, FeeType FeeType, Session CurrentSession)
        {
            try
            {
                ApplicationProgrammeFeeLogic applicationProgrammeFeeLogic = new ApplicationProgrammeFeeLogic();
                ApplicationProgrammeFee applicationProgrammeFee = applicationProgrammeFeeLogic.GetModelBy(p => p.Fee_Type_Id == FeeType.Id && p.Programme_Id == Programme.Id && p.Session_Id == CurrentSession.Id);
                return applicationProgrammeFee;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public Session GetCurrentSession()
        {
            try
            {
                //CurrentSessionSemesterLogic currentSessionLogic = new CurrentSessionSemesterLogic();
                //CurrentSessionSemester currentSessionSemester = currentSessionLogic.GetCurrentSessionTerm();
                //if (currentSessionSemester != null && currentSessionSemester.SessionSemester != null)
                //{
                //    return currentSessionSemester.SessionSemester.Session;
                //}

                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelsBy(a => a.Activated.Value == true).LastOrDefault();
                return session;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Session GetApplicationSession()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                Session session = sessionLogic.GetModelsBy(a => a.Active_For_Application.Value == true).LastOrDefault();
                return session;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public FeeType GetFeeTypeBy(Session session, Programme programme)
        //{
        //    try
        //    {
        //        ApplicationProgrammeFeeLogic programmeFeeLogic = new ApplicationProgrammeFeeLogic();
        //        ApplicationProgrammeFee = programmeFeeLogic.GetBy(programme, session);

        //        if (ApplicationProgrammeFee != null)
        //        {
        //            return ApplicationProgrammeFee.FeeType;
        //        }

        //        return null;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public FeeType GetFeeTypeBy(Session session, Programme programme)
        {
            try
            {
                ApplicationProgrammeFeeLogic programmeFeeLogic = new ApplicationProgrammeFeeLogic();
                List<ApplicationProgrammeFee> applicationProgrammeFess = programmeFeeLogic.GetListBy(programme, session);
                foreach (ApplicationProgrammeFee item in applicationProgrammeFess)
                {
                    //if (item.FeeType.Id <= 6)
                    //{
                        return item.FeeType;
                    //}
                }
              
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public PaymentEtranzactType GetPaymentTypeBy(FeeType feeType, Programme programme, Level level, PaymentMode paymentMode, Session session)
        {
            PaymentEtranzactTypeLogic paymentEtranzactTypeLogic = new PaymentEtranzactTypeLogic();
            //PaymentEtranzactType = paymentEtranzactTypeLogic.GetBy(feeType);
            PaymentEtranzactType = paymentEtranzactTypeLogic.GetModelsBy(p => p.Fee_Type_Id == feeType.Id && p.Programme_Id == programme.Id && p.Level_Id == level.Id && p.Payment_Mode_Id == paymentMode.Id && p.Session_Id == session.Id).LastOrDefault();

            if (PaymentEtranzactType != null)
            {
                return PaymentEtranzactType;
            }

            return null;
        }

        public ApplicationFormSetting GetApplicationFormSettingBy(Session session)
        {
            try
            {
                ApplicationFormSettingLogic applicationFormSettingLogic = new ApplicationFormSettingLogic();
                return applicationFormSettingLogic.GetBy(session);
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}