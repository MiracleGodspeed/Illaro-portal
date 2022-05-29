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
    public class ConfrencePaymentViewModels
    {
        public ConfrencePaymentViewModels()
        {
            Programme = new Programme();
            Confrence = new Confrence();
            AppliedCourse = new AppliedCourse();
            AppliedCourse.Programme = new Programme();
            AppliedCourse.Department = new Department();
            remitaPayment = new RemitaPayment();
            Title = new Title();
            Sex = new Sex();
            Person = new Person();
            Fee = new Fee();

            Person.State = new State();
            StateSelectList = Utility.PopulateStateSelectListItem();
            ProgrammeSelectListItem = Utility.PopulateProgrammeSelectListItem();
            TitleSelectListItem = Utility.PopulateTitleSelectListItem();
            SexSelectListItem = Utility.PopulateSexSelectListItem();
            Country = new Country();
            CountrySelectListItem = Utility.PopulateCountrySelectListItem();

        }
        
        [Display(Name = "Date Start")]
        [DataType(DataType.Date)]
        public DateTime DateStart { get; set; }

        [Display(Name = "Date End")]
        [DataType(DataType.Date)]
        public DateTime DateEnd { get; set; }

        public AppliedCourse AppliedCourse { get; set; }
        public ApplicantJambDetail ApplicantJambDetail { get; set; }
        public Department Department { get; set; }
        public RemitaPayment remitaPayment { get; set; }
        [Display(Name = "JAMB Reg. No")]
        public string JambRegistrationNumber { get; set; }
        [Display(Name = "Code")]
        public string PhoneCode { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNO { get; set; }

        [Display(Name = "Department")]
        public string InstDepartment { get; set; }


        [Display(Name = "Institution")]
        public string Institution { get; set; }

        [Display(Name = "City")]
        public string InCity { get; set; }

        [Display(Name = "State/ Province")]
        public string InstState { get; set; }

        [Display(Name = "Postal/ Zipcode")]
        public string InstPostal { get; set; }

        [Display(Name = "Country")]
        public string InstsCountry { get; set; }

        [Display(Name = "Confrence Registration Fee ₦20,000.00")]
        public bool ConfrenceRegFee { get; set; }

        [Display(Name = "Tourist Attraction Fee ₦10,000.00")]
        public bool TouristAttractionFee { get; set; }


        public Programme Programme { get; set; }
        public Title Title { get; set; }
        public Sex Sex { get; set; }
        public Country Country { get; set; }
        public Fee Fee { get; set; }
        public Confrence Confrence { get; set; }
        public List<SelectListItem> StateSelectList { get; set; }
        public List<SelectListItem> ProgrammeSelectListItem { get; set; }
        public List<SelectListItem> TitleSelectListItem { get; set; }
        public List<SelectListItem> SexSelectListItem { get; set; }
        public List<SelectListItem> DepartmentSelectListItem { get; set; }
        public List<SelectListItem> DepartmentOptionSelectListItem { get; set; }
        public List<SelectListItem> CountrySelectListItem { get; set; }
        public FeeType FeeType { get; set; }
        public Person Person { get; set; }
        public decimal Amount { get; set; }
        public Session CurrentSession { get; set; }
        public Paystack Paystack { get; set; }
        public PaymentType PaymentType { get; set; }
        public Payment Payment { get; set; }
        [Display(Name = "ABSTRACT/FULL PAPER SUBMISSION")]
        public HttpPostedFileBase File { get; set; }
        public ApplicationFormSetting ApplicationFormSetting { get; set; }
        public ApplicationProgrammeFee ApplicationProgrammeFee { get; set; }
        public PaymentEtranzactType PaymentEtranzactType { get; set; }
        public Remita Remita { get; internal set; }
        public List<Paystack> Paystacks = new List<Paystack>();
        public string Hash { get; internal set; }
       public List<Person> people = new List<Person>();
        public List<Confrence> confrences = new List<Confrence>();
        public void Initialise()
        {
            try
            {
                //CurrentSession = GetCurrentSession();
                CurrentSession = GetApplicationSession();
                //CurrentSession = new Session(){Id = (int)Sessions._20172018};

                if (CurrentSession != null && Programme.Id > 0)
                {
                    FeeType = GetFeeTypeBy(CurrentSession, Programme);
                    ApplicationFormSetting = GetApplicationFormSettingBy(CurrentSession);

                    Level level = null;
                    if (Programme.Id == (int)Programmes.NDFullTime || Programme.Id == (int)Programmes.NDPartTime)
                    {
                        level = new Level(){Id = (int)Levels.NDI};
                    }
                    else
                    {
                        level = new Level() { Id = (int)Levels.HNDI };
                    }

                    PaymentMode paymentMode = new PaymentMode(){Id = (int)PaymentModes.Full};

                    PaymentEtranzactType = GetPaymentTypeBy(FeeType, Programme, level, paymentMode, CurrentSession);
                    ApplicationProgrammeFee = GetApplicationProgrammeFeeBy(Programme, FeeType, CurrentSession);
                    if (ApplicationFormSetting != null)
                    {
                        PaymentType = ApplicationFormSetting.PaymentType;
                    }
                }
            }
            catch (Exception)
            {
                throw;
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