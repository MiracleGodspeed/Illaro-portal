﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Abundance_Nk.Model.Model;
using System.Globalization;
using System.Linq.Expressions;
using Abundance_Nk.Business;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System.IO;
using OfficeOpenXml;
using System.Data;
using System.Reflection;

namespace Abundance_Nk.Web.Models
{
    public class Utility
    {
        public const string ID = "Id";
        public const string NAME = "Name";
        public const string VALUE = "Value";
        public const string TEXT = "Text";
        public const string Select = "-- Select --";
        public const string DEFAULT_AVATAR = "/Content/Images/default_avatar.png";
        public const string DEFAULT_SIGNATURE = "/Content/Images/signSample.png";
        public const string SelectDepartment = "-- Select Department --";
        public const string SelectDepartmentOption = "-- Select Department Option --";
        public const string SelectAdmissiontype = "-- Select Admission Type --";
        public const string SelectSession = "-- Select Session --";
        public const string SelectSemester = "-- Select Semester --";
        public const string SelectLevel = "-- Select Level --";
        public const string SelectProgramme = "-- Select Programme --";
        public const string SelectState = "-- Select State --";
        public const string SelectCourse = "-- Select Course --";

        public const string FIRST_SITTING = "FIRST SITTING";
        public const string SECOND_SITTING = "SECOND SITTING";

        public static void BindDropdownItem<T>(DropDownList dropDownList, T items, string dataValueField, string dataTextField)
        {
            dropDownList.Items.Clear();

            dropDownList.DataValueField = dataValueField;
            dropDownList.DataTextField = dataTextField;
           

            dropDownList.DataSource = items;
            dropDownList.DataBind();
        }

        public static List<Value> CreateYearListFrom(int startYear)
        {
            List<Value> years = new List<Value>();

            try
            {
                DateTime currentDate = DateTime.Now;
                int currentYear = currentDate.Year;

                for (int i = startYear; i <= currentYear; i++)
                {
                    Value value = new Value();
                    value.Id = i;
                    value.Name = i.ToString();
                    years.Add(value);
                }

                //years.Insert(0, new Value() { Id = 0, Name = Select });
                return years;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<Value> CreateYearListFrom(int startYear, int endYear)
        {
            List<Value> years = new List<Value>();

            try
            {



                for (int i = startYear; i <= endYear; i++)
                {
                    Value value = new Value();
                    value.Id = i;
                    value.Name = i.ToString();
                    years.Add(value);
                }

                //years.Insert(0, new Value() { Id = 0, Name = Select });
                return years;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static List<Value> CreateNumberListFrom(int startValue, int endValue)
        {
            List<Value> values = new List<Value>();

            try
            {
                for (int i = startValue; i <= endValue; i++)
                {
                    Value value = new Value();
                    value.Id = i;
                    value.Name = i.ToString();
                    values.Add(value);
                }

                //values.Insert(0, new Value() { Id = 0, Name = Select });
                return values;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool CheckExtraYearStudent(Model.Model.Student student, Session session)
        {
            bool extraYearStatus = false;
            try
            {
                if (student != null && !string.IsNullOrEmpty(student.MatricNumber) && session != null && session.Id > 0)
                {
                    SessionLogic sessionLogic = new SessionLogic();
                    StudentLogic studentLogic = new StudentLogic();

                    session = sessionLogic.GetModelBy(s => s.Session_Id == session.Id);
                    student = studentLogic.GetModelsBy(s => s.Matric_Number == student.MatricNumber).LastOrDefault();

                    int sessionWildCard = Convert.ToInt32(session.Name.Substring(2, 2));
                    int matricNumberWildCard = Convert.ToInt32(student.MatricNumber.Split('/')[2]);

                    if (matricNumberWildCard == sessionWildCard || matricNumberWildCard == (sessionWildCard - 1))
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return extraYearStatus;
        }
        public static bool HasCompletedSchoolFees(Student student, Session session)
        {
            bool hasCompletedFees = false;
            decimal optionalAmount = 6000;
            bool isCISCOPaymentCompulsory = false;
            bool isRoboticsPaymentCompulsory = false;

            try
            {
                if (student != null && student.Id > 0 && session != null && session.Id > 0)
                {
                    RemitaPayment remitaPayment = null;
                    PaymentEtranzact paymentEtranzact = null;

                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    SessionLogic sessionLogic = new SessionLogic();
                    bool isExtraYear = false;

                    StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == session.Id).LastOrDefault();
                    if (studentLevel == null)
                    {
                        throw new Exception("No Student Level record found for this session");
                    }

                    decimal amountPaid = 0M;
                    decimal amountToPay = 0M;

                    List<FeeDetail> feeDetails = new List<FeeDetail>();
                    if (studentLevel.Programme.Id != (int)Programmes.NDDistance || studentLevel.Programme.Id != (int)Programmes.HNDDistance)
                    {
                        isExtraYear = CheckExtraYearStudent(student, session);

                    }
                    if (isExtraYear)
                    {
                        remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.CarryOverSchoolFees &&
                                                                            r.PAYMENT.Session_Id == session.Id && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();
                        feeDetails = feeDetailLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Fee_Type_Id == (int)FeeTypes.CarryOverSchoolFees &&
                                                    s.Level_Id == studentLevel.Level.Id && s.Payment_Mode_Id == (int)PaymentModes.Full && s.Programme_Id == studentLevel.Programme.Id &&
                                                    s.Session_Id == session.Id);

                        int matricSession = Convert.ToInt32(student.MatricNumber.Split('/')[2]);
                        var sessionName = sessionLogic.GetModelBy(g => g.Session_Id == session.Id);
                        int currentSession = Convert.ToInt32(sessionName.Name.Substring(2, 2));
                        int NoOfOutstandingSession = currentSession - (matricSession + 1);
                        if (NoOfOutstandingSession == 0)
                        {
                            NoOfOutstandingSession = 1;
                        }

                        amountToPay = feeDetails.Sum(p => p.Fee.Amount) * NoOfOutstandingSession;

                        if (remitaPayment != null)
                        {
                            if (remitaPayment.TransactionAmount < amountToPay)
                            {
                                return CheckShortFallRemita(remitaPayment, amountToPay);
                            }
                            return remitaPayment.TransactionAmount >= amountToPay;
                        }
                        else
                        {
                            paymentEtranzact = paymentEtranzactLogic.GetModelsBy(r => r.ONLINE_PAYMENT.PAYMENT.Person_Id == student.Id && r.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.CarryOverSchoolFees &&
                                                                            r.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id).LastOrDefault();
                            if (paymentEtranzact != null)
                            {
                                return paymentEtranzact.TransactionAmount >= amountToPay;
                            }
                        }
                    }
                    else
                    {
                        remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                           r.PAYMENT.Session_Id == session.Id && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();






                        feeDetails = feeDetailLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                    s.Level_Id == studentLevel.Level.Id && s.Payment_Mode_Id == (int)PaymentModes.Full && s.Programme_Id == studentLevel.Programme.Id &&
                                                    s.Session_Id == session.Id);
                        if (studentLevel.Department.Id == 44)
                        {
                            if (studentLevel.DepartmentOption.Id == 16)
                            {
                                decimal toPay = Convert.ToDecimal(Fees.GraduateDrivingTrainingSchoolFees);
                                if (remitaPayment?.TransactionAmount >= toPay)
                                {
                                    return true;
                                }
                            }
                            if (studentLevel.DepartmentOption.Id == 17)
                            {
                                decimal toPay = Convert.ToDecimal(Fees.TechnicalCertificateDrivingSchooFees);
                                if (remitaPayment?.TransactionAmount >= toPay)
                                {
                                    return true;
                                }
                            }
                            if (studentLevel.DepartmentOption.Id == 18)
                            {
                                decimal toPay = Convert.ToDecimal(Fees.ProfessionalDiplomaDrivingSchooFees);
                                if (remitaPayment?.TransactionAmount >= toPay)
                                {
                                    return true;
                                }
                            }
                            return false;
                        }

                        amountToPay = feeDetails.Sum(p => p.Fee.Amount);

                        if (remitaPayment != null)
                        {
                            if (remitaPayment.TransactionAmount < amountToPay)
                            {
                                if (remitaPayment.payment.Session.Id == 12 && remitaPayment.payment.FeeType.Id == (int)FeeTypes.SchoolFees && (remitaPayment.TransactionAmount < amountToPay))
                                {
                                    decimal newAmountToPay = 0;
                                    if (!IsReturningStudent(studentLevel.Student))
                                    {
                                        //Check if CISCO payment is compulsory for student
                                        if (!IsCISCOPaymentApplicaple(studentLevel))
                                        {
                                            newAmountToPay = amountToPay - (int)Fees.CISCOFeeAmount;
                                            if(remitaPayment.TransactionAmount >= newAmountToPay)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                var shortfall = CheckShortFallRemita(remitaPayment, newAmountToPay);
                                                if (shortfall)
                                                    return shortfall;
                                                
                                            }
                                        }
                                        //Check if Robotics payment is compulsory for student
                                        else if (!IsRoboticsPaymentApplicaple(studentLevel))
                                        {
                                            newAmountToPay = amountToPay - (int)Fees.RoboticsFeeAmount;
                                            if (remitaPayment.TransactionAmount >= newAmountToPay)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                var shortfall = CheckShortFallRemita(remitaPayment, newAmountToPay);
                                                if (shortfall)
                                                    return shortfall;

                                            }
                                        }
                                        else if (!IsRoboticsPaymentApplicaple(studentLevel) && !IsCISCOPaymentApplicaple(studentLevel))
                                        {
                                            decimal optionalSum = (int)Fees.RoboticsFeeAmount + (int)Fees.CISCOFeeAmount;
                                            newAmountToPay = amountToPay - optionalSum;
                                            if (remitaPayment.TransactionAmount >= newAmountToPay)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                var shortfall = CheckShortFallRemita(remitaPayment, newAmountToPay);
                                                if (shortfall)
                                                    return shortfall;

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (remitaPayment.TransactionAmount >= (amountToPay - (int)Fees.RoboticsFeeAmount))
                                            return true;
                                        else if (remitaPayment.TransactionAmount >= (amountToPay - (int)Fees.CISCOFeeAmount))
                                            return true;
                                        else
                                        {
                                            decimal optionalSum = (int)Fees.RoboticsFeeAmount + (int)Fees.CISCOFeeAmount;
                                            newAmountToPay = amountToPay - optionalSum;
                                            return remitaPayment.TransactionAmount >= newAmountToPay;
                                        }
                                            
                                    }


                                    //return remitaPayment.TransactionAmount >= amountToPay;

                                }
                                return CheckShortFallRemita(remitaPayment, amountToPay);
                            }
                            //Temp

                            return remitaPayment.TransactionAmount >= amountToPay;
                        }
                        else
                        {
                            paymentEtranzact = paymentEtranzactLogic.GetModelsBy(r => r.ONLINE_PAYMENT.PAYMENT.Person_Id == student.Id && r.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.CarryOverSchoolFees &&
                                                                            r.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id).LastOrDefault();
                            if (paymentEtranzact != null)
                            {
                                return paymentEtranzact.TransactionAmount >= amountToPay;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return hasCompletedFees;
        }

        public static bool IsReturningStudent(Student student)
        {
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                StudentLevel studentLevel = studentLevelLogic.GetModelsBy(x => x.STUDENT.Person_Id == student.Id).LastOrDefault();
                if(studentLevel != null)
                {
                    if (studentLevel.Level.Id == (int)Levels.HNDII || (studentLevel.Level.Id == (int)Levels.NDII))
                        return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsCISCOPaymentApplicaple(StudentLevel studentLevel)
        {
            try
            {
                if (studentLevel.Level.Id == (int)Levels.HNDII || studentLevel.Level.Id == (int)Levels.NDII)
                    return false;
                else
                {
                    if (studentLevel.Department.Faculty.Id == (int)Schools.CommunicationAndTechnology)
                        return true;
                    else
                        return false;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        public static bool IsRoboticsPaymentApplicaple(StudentLevel studentLevel)
        {
            try
            {
                if (studentLevel.Level.Id == (int)Levels.HNDII || studentLevel.Level.Id == (int)Levels.NDII)
                    return false;
                else
                {
                    if (studentLevel.Department.Faculty.Id == (int)Schools.CommunicationAndTechnology || studentLevel.Department.Faculty.Id == (int)Schools.ManagementStudies)
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool HasCompletedSchoolFeesDistantLearning(Student student, Session session)
        {
            bool hasCompletedFees = false;
            try
            {
                if (student != null && student.Id > 0 && session != null && session.Id > 0)
                {
                    RemitaPayment remitaPayment = null;
                    PaymentEtranzact paymentEtranzact = null;

                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    SessionLogic sessionLogic = new SessionLogic();
                    bool isExtraYear = false;

                    StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == session.Id).LastOrDefault();
                    if (studentLevel == null)
                    {
                        throw new Exception("No Student Level record found for this session");
                    }

                    decimal amountPaid = 0M;
                    decimal amountToPay = 0M;

                    List<FeeDetail> feeDetails = new List<FeeDetail>();
                    //if (studentLevel.Programme.Id != (int)Programmes.NDDistance || studentLevel.Programme.Id != (int)Programmes.HNDDistance)
                    //{
                    //    isExtraYear = CheckExtraYearStudent(student, session);

                    //}
                    if (isExtraYear)
                    {
                        remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.CarryOverSchoolFees &&
                                                                            r.PAYMENT.Session_Id == session.Id && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();
                        feeDetails = feeDetailLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Fee_Type_Id == (int)FeeTypes.CarryOverSchoolFees &&
                                                    s.Level_Id == studentLevel.Level.Id && s.Payment_Mode_Id == (int)PaymentModes.Full && s.Programme_Id == studentLevel.Programme.Id &&
                                                    s.Session_Id == session.Id);

                        int matricSession = Convert.ToInt32(student.MatricNumber.Split('/')[2]);
                        var sessionName = sessionLogic.GetModelBy(g => g.Session_Id == session.Id);
                        int currentSession = Convert.ToInt32(sessionName.Name.Substring(2, 2));
                        int NoOfOutstandingSession = currentSession - (matricSession + 1);
                        if (NoOfOutstandingSession == 0)
                        {
                            NoOfOutstandingSession = 1;
                        }

                        amountToPay = feeDetails.Sum(p => p.Fee.Amount) * NoOfOutstandingSession;

                        if (remitaPayment != null)
                        {
                            if (remitaPayment.TransactionAmount < amountToPay)
                            {
                                return CheckShortFallRemita(remitaPayment, amountToPay);
                            }
                            return remitaPayment.TransactionAmount >= amountToPay;
                        }
                        else
                        {
                            paymentEtranzact = paymentEtranzactLogic.GetModelsBy(r => r.ONLINE_PAYMENT.PAYMENT.Person_Id == student.Id && r.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == (int)FeeTypes.CarryOverSchoolFees &&
                                                                            r.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id).LastOrDefault();
                            if (paymentEtranzact != null)
                            {
                                return paymentEtranzact.TransactionAmount >= amountToPay;
                            }
                        }
                    }
                    else
                    {
                        feeDetails = feeDetailLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                  s.Level_Id == studentLevel.Level.Id && s.Payment_Mode_Id == (int)PaymentModes.Full && s.Programme_Id == studentLevel.Programme.Id &&
                                                  s.Session_Id == session.Id);

                        remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                           r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Payment_Mode_Id == (int)PaymentModes.Full && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();

                        
                        amountToPay = feeDetails.Sum(p => p.Fee.Amount);

                        if (remitaPayment == null)
                        {
                            //if (remitaPayment.TransactionAmount < amountToPay)
                            //{
                                
                                decimal totalAmountPaid = 0M;
                                


                                //First Installment Payment
                                var remitaPaymentFirst = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                                   r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Payment_Mode_Id == (int)PaymentModes.FirstInstallment && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();

                                //Second Installment Payment
                                var remitaPaymentSecond = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                                  r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Payment_Mode_Id == (int)PaymentModes.SecondInstallment && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();

                                //Third Installment Payment
                                var remitaPaymentThird = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                                  r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Payment_Mode_Id == (int)PaymentModes.ThirdInstallment && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();



                                if(remitaPaymentFirst != null && remitaPaymentSecond != null && remitaPaymentThird != null)
                                {
                                    amountToPay = feeDetails.Sum(p => p.Fee.Amount);
                                    totalAmountPaid = remitaPaymentFirst.TransactionAmount + remitaPaymentSecond.TransactionAmount + remitaPaymentThird.TransactionAmount;

                                    if(totalAmountPaid >= amountToPay)
                                    {
                                        return true;
                                    }
                                }
                            else
                            {
                                return false;
                            }



                                //Shortfall check
                                amountToPay = feeDetails.Sum(p => p.Fee.Amount);
                                return CheckShortFallRemita(remitaPayment, amountToPay);
                            //}
                            //return remitaPayment.TransactionAmount >= amountToPay;
                        }
                        else
                        {
                            return remitaPayment.TransactionAmount >= amountToPay; 
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return hasCompletedFees;
        }



        public static bool HasCompletedSchoolFirstInstallment(Student student, Session session)
        {
            bool hasCompletedFees = false;
            try
            {
                if (student != null && student.Id > 0 && session != null && session.Id > 0)
                {
                    RemitaPayment remitaPayment = null;

                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    SessionLogic sessionLogic = new SessionLogic();

                    StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == session.Id).LastOrDefault();
                    if (studentLevel == null)
                    {
                        throw new Exception("No Student Level record found for this session");
                    }

                    decimal amountToPay = 0M;

                    List<FeeDetail> feeDetails = new List<FeeDetail>();
                   
                        feeDetails = feeDetailLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                  s.Level_Id == studentLevel.Level.Id && s.Payment_Mode_Id == (int)PaymentModes.FirstInstallment && s.Programme_Id == studentLevel.Programme.Id &&
                                                  s.Session_Id == session.Id);


                        remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                           r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Payment_Mode_Id == (int)PaymentModes.FirstInstallment && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();


                        amountToPay = feeDetails.Sum(p => p.Fee.Amount);

                        if (remitaPayment == null)
                        {

                            return false;
                            //Shortfall check
                            //amountToPay = feeDetails.Sum(p => p.Fee.Amount);
                            //return CheckShortFallRemita(remitaPayment, amountToPay);
                           
                        }
                        else
                        {
                            return remitaPayment.TransactionAmount >= amountToPay;
                        }
                    
                }
            }
            catch (Exception)
            {
                throw;
            }

            return hasCompletedFees;
        }


        public static bool HasCompletedSchoolSecondInstallment(Student student, Session session)
        {
            bool hasCompletedFees = false;
            try
            {
                if (student != null && student.Id > 0 && session != null && session.Id > 0)
                {
                    RemitaPayment remitaPayment = null;

                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    SessionLogic sessionLogic = new SessionLogic();

                    StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == session.Id).LastOrDefault();
                    if (studentLevel == null)
                    {
                        throw new Exception("No Student Level record found for this session");
                    }

                    decimal amountToPay = 0M;

                    List<FeeDetail> feeDetails = new List<FeeDetail>();

                    feeDetails = feeDetailLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                              s.Level_Id == studentLevel.Level.Id && s.Payment_Mode_Id == (int)PaymentModes.SecondInstallment && s.Programme_Id == studentLevel.Programme.Id &&
                                              s.Session_Id == session.Id);


                    remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                       r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Payment_Mode_Id == (int)PaymentModes.SecondInstallment && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();


                    amountToPay = feeDetails.Sum(p => p.Fee.Amount);

                    if (remitaPayment == null)
                    {
                        return false;
                        //Shortfall check
                        //amountToPay = feeDetails.Sum(p => p.Fee.Amount);
                        //return CheckShortFallRemita(remitaPayment, amountToPay);
                    }
                    else
                    {
                        return remitaPayment.TransactionAmount >= amountToPay;
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

            return hasCompletedFees;
        }

        public static bool HasCompletedSchoolThirdInstallment(Student student, Session session)
        {
            bool hasCompletedFees = false;
            try
            {
                if (student != null && student.Id > 0 && session != null && session.Id > 0)
                {
                    RemitaPayment remitaPayment = null;

                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    FeeDetailLogic feeDetailLogic = new FeeDetailLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    SessionLogic sessionLogic = new SessionLogic();

                    StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == student.Id && s.Session_Id == session.Id).LastOrDefault();
                    if (studentLevel == null)
                    {
                        throw new Exception("No Student Level record found for this session");
                    }

                    decimal amountToPay = 0M;

                    List<FeeDetail> feeDetails = new List<FeeDetail>();

                    feeDetails = feeDetailLogic.GetModelsBy(s => s.Department_Id == studentLevel.Department.Id && s.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                              s.Level_Id == studentLevel.Level.Id && s.Payment_Mode_Id == (int)PaymentModes.ThirdInstallment && s.Programme_Id == studentLevel.Programme.Id &&
                                              s.Session_Id == session.Id);


                    remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == student.Id && r.PAYMENT.Fee_Type_Id == (int)FeeTypes.SchoolFees &&
                                                                       r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Payment_Mode_Id == (int)PaymentModes.ThirdInstallment && (r.Status.Contains("01") || r.Description.ToLower().Contains("manual"))).LastOrDefault();


                    amountToPay = feeDetails.Sum(p => p.Fee.Amount);

                    if (remitaPayment == null)
                    {
                        return false;
                        //Shortfall check
                        //amountToPay = feeDetails.Sum(p => p.Fee.Amount);
                        //return CheckShortFallRemita(remitaPayment, amountToPay);
                    }
                    else
                    {
                        return remitaPayment.TransactionAmount >= amountToPay;
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

            return hasCompletedFees;
        }
        private static decimal ReturnShortFallRemitaAmount(RemitaPayment remitaPayment, decimal amountToPay)
        {
            try
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                List<RemitaPayment> remitaPaymentShortfalls = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == remitaPayment.payment.Person.Id && r.PAYMENT.Fee_Type_Id == 12 &&
                                                                                        r.Status.Contains("01") && r.PAYMENT.Session_Id == remitaPayment.payment.Session.Id);

                decimal shortFallAmount = 0M;

                if (remitaPaymentShortfalls.Count > 0)
                {
                    for (int i = 0; i < remitaPaymentShortfalls.Count; i++)
                    {
                        shortFallAmount += remitaPaymentShortfalls[i].TransactionAmount;
                    }

                    return shortFallAmount + remitaPayment.TransactionAmount;
                }
                else
                    return 0;
            }
            catch (Exception)
            {
                throw;
            }

            return 0;
        }

        private static bool CheckShortFallRemita(RemitaPayment remitaPayment, decimal amountToPay)
        {
            try
            {
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                List<RemitaPayment> remitaPaymentShortfalls = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Person_Id == remitaPayment.payment.Person.Id && r.PAYMENT.Fee_Type_Id == 12 &&
                                                                                        r.Status.Contains("01") && r.PAYMENT.Session_Id == remitaPayment.payment.Session.Id);

                decimal shortFallAmount = 0M;

                if (remitaPaymentShortfalls.Count > 0)
                {
                    for (int i = 0; i < remitaPaymentShortfalls.Count; i++)
                    {
                        shortFallAmount += remitaPaymentShortfalls[i].TransactionAmount;
                    }

                    return shortFallAmount + remitaPayment.TransactionAmount >= amountToPay;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }

            return false;
        }
        public static List<Value> GetMonthsInYear()
        {
            List<Value> values = new List<Value>();

            try
            {
                string[] names = DateTimeFormatInfo.CurrentInfo.MonthNames;

                for (int i = 0; i < names.Length; i++)
                {
                    int j = i + 1;
                    Value value = new Value();
                    value.Id = j;
                    value.Name = names[i];
                    values.Add(value);
                }

                //values.Insert(0, new Value() { Id = 0, Name = Select });
                return values;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateGraduationMonthSelectListItem()
        {
            try
            {
                List<Value> months = GetMonthsInYear();
                if (months == null || months.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> monthList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                monthList.Add(list);

                foreach (Value month in months)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = month.Id.ToString();
                    selectList.Text = month.Name;

                    monthList.Add(selectList);
                }

                return monthList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static List<SelectListItem> PopulateApplicantStatusSelectListItem()
        {
            try
            {
                ApplicantStatusLogic studentStatusLogic = new ApplicantStatusLogic();
                List<ApplicantStatus> studentStatuses = studentStatusLogic.GetAll();

                if (studentStatuses == null || studentStatuses.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> studentStatusList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                studentStatusList.Add(list);

                foreach (ApplicantStatus studentStatus in studentStatuses)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = studentStatus.Id.ToString();
                    selectList.Text = studentStatus.Name;

                    studentStatusList.Add(selectList);
                }

                return studentStatusList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateAdmissionSessionSelectListItem()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                //List<Session> sessions = sessionLogic.GetApplicationSession();
                Session sessions = sessionLogic.GetApplicationSession();

               // if (sessions == null || sessions.Count <= 0)
                if (sessions == null)
                    {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectSession;
                selectItemList.Add(list);

                //foreach (Session session in sessions)
                //{
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = sessions.Id.ToString();
                    selectList.Text = sessions.Name;

                    selectItemList.Add(selectList);
                //}

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateExamYearSelectListItem(int startYear)
        {
            try
            {
                List<Value> years = CreateYearListFrom(startYear);
                if (years == null || years.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> yearList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "0";
                list.Text = Select;
                yearList.Add(list);

                foreach (Value year in years)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = year.Id.ToString();
                    selectList.Text = year.Name;

                    yearList.Add(selectList);
                }

                return yearList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateJambScoreSelectListItem(int startScore, int endScore)
        {
            try
            {
                List<Value> scores = CreateNumberListFrom(startScore, endScore);
                if (scores == null || scores.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> yearList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                yearList.Add(list);

                foreach (Value score in scores)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = score.Id.ToString();
                    selectList.Text = score.Name;

                    yearList.Add(selectList);
                }

                return yearList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateYearSelectListItem(int startYear, bool withSelect)
        {
            try
            {
                int END_YEAR = DateTime.Now.Year + 2;
                List<Value> years = CreateYearListFrom(startYear, END_YEAR);
                if (years == null || years.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> yearList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                if (withSelect)
                {
                    list.Text = Select;
                }
                else
                {
                    list.Text = "--YY--";
                }

                yearList.Add(list);

                foreach (Value year in years)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = year.Id.ToString();
                    selectList.Text = year.Name;

                    yearList.Add(selectList);
                }

                return yearList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateYearofGraduationSelectListItem(int startYear,int endYear, bool withSelect)
        {
            try
            {
                List<Value> years = CreateYearListFrom(startYear, endYear);
                if (years == null || years.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> yearList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                if (withSelect)
                {
                    list.Text = Select;
                }
                else
                {
                    list.Text = "--YY--";
                }

                yearList.Add(list);

                foreach (Value year in years)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = year.Id.ToString();
                    selectList.Text = year.Name;

                    yearList.Add(selectList);
                }

                return yearList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateLicenceTypeSelectListItem()
        {
            try
            {
                LicenseTypeLogic licenseTypeLogic = new LicenseTypeLogic();
                List<LicenseType> licenseType = licenseTypeLogic.GetAll();
                if (licenseType == null || licenseType.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> licenseTypeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                licenseTypeList.Add(list);

                foreach (LicenseType license_type in licenseType)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = license_type.Id.ToString();
                    selectList.Text = license_type.Name;

                    licenseTypeList.Add(selectList);
                }

                return licenseTypeList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateSexSelectListItem()
        {
            try
            {
                SexLogic sexLogic = new SexLogic();
                List<Sex> genders = sexLogic.GetAll();
                if (genders == null || genders.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> sexList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                sexList.Add(list);

                foreach (Sex sex in genders)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = sex.Id.ToString();
                    selectList.Text = sex.Name;

                    sexList.Add(selectList);
                }

                return sexList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateVerificationFeeTypeSelectListItem()
        {
            try
            {
                FeeTypeLogic feeTypeLogic = new FeeTypeLogic();
                Expression<Func<FEE_TYPE, bool>> selector = f => f.Fee_Type_Id == 14 || f.Fee_Type_Id == 15 || f.Fee_Type_Id == 16|| f.Fee_Type_Id == 17 || f.Fee_Type_Id == 18 || f.Fee_Type_Id == 19 || f.Fee_Type_Id == 20 || f.Fee_Type_Id == 24; 
                List<FeeType> feeTypes = feeTypeLogic.GetModelsBy(selector);
                if(feeTypes == null || feeTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> sexList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                sexList.Add(list);

                foreach(FeeType feeType in feeTypes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = feeType.Id.ToString();
                    selectList.Text = feeType.Name;

                    sexList.Add(selectList);
                }

                return sexList;
            }
            catch(Exception)
            {
                throw;
            }
        }
      
        public static List<SelectListItem> PopulateFeeTypeSelectListItem()
        {
            try
            {
                FeeTypeLogic feeTypeLogic = new FeeTypeLogic();
                List<FeeType> feeTypes = feeTypeLogic.GetAll();
                if (feeTypes == null || feeTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> sexList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                sexList.Add(list);

                foreach (FeeType feeType in feeTypes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = feeType.Id.ToString();
                    selectList.Text = feeType.Name;

                    sexList.Add(selectList);
                }

                return sexList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateAcceptanceSchoolFeesSelectListItem()
        {
            try
            {
                FeeTypeLogic feeTypeLogic = new FeeTypeLogic();
                List<FeeType> feeTypes = feeTypeLogic.GetAll();
                if (feeTypes == null || feeTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> sexList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                sexList.Add(list);

                foreach (FeeType feeType in feeTypes)
                {
                    if(feeType.Id == 2 || feeType.Id == 3)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = feeType.Id.ToString();
                        selectList.Text = feeType.Name;

                        sexList.Add(selectList);
                    }
                  
                }

                return sexList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateFeeSelectListItem()
        {
            try
            {
                FeeLogic feeLogic = new FeeLogic();
                List<Fee> fees = feeLogic.GetAll();
                if (fees == null || fees.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> feeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                feeList.Add(list);

                foreach (Fee fee in fees)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = fee.Id.ToString();
                    selectList.Text = fee.Name + " - " + fee.Amount;

                    feeList.Add(selectList);
                }

                return feeList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static List<SelectListItem> PopulateEducationalQualificationSelectListItem()
        {
            try
            {
                EducationalQualificationLogic qualificationLogic = new EducationalQualificationLogic();
                List<EducationalQualification> educationalQualifications = qualificationLogic.GetAll();
                if (educationalQualifications == null || educationalQualifications.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> educationalQualificationList = new List<SelectListItem>();
                
                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                educationalQualificationList.Add(list);

                foreach (EducationalQualification educationalQualification in educationalQualifications)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = educationalQualification.Id.ToString();
                    selectList.Text = educationalQualification.ShortName;

                    educationalQualificationList.Add(selectList);
                }

                return educationalQualificationList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateAbilitySelectListItem()
        {
            try
            {
                AbilityLogic abilityLogic = new AbilityLogic();
                List<Ability> abilities = abilityLogic.GetAll();
                if (abilities == null && abilities.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> abilityList = new List<SelectListItem>();

                if (abilities != null || abilities.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    abilityList.Add(list);

                    foreach (Ability ability in abilities)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = ability.Id.ToString();
                        selectList.Text = ability.Name;

                        abilityList.Add(selectList);
                    }
                }

                return abilityList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateReligionSelectListItem()
        {
            try
            {
                ReligionLogic religionLogic = new ReligionLogic();
                List<Religion> religions = religionLogic.GetAll();
                if (religions == null || religions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> religionList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                religionList.Add(list);

                foreach (Religion religion in religions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = religion.Id.ToString();
                    selectList.Text = religion.Name;

                    religionList.Add(selectList);
                }

                return religionList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateStateSelectListItem()
        {
            try
            {
                StateLogic stateLogic = new StateLogic();
                List<State> states = stateLogic.GetAll();
                if (states == null || states.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                stateList.Add(list);

                foreach (State state in states)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = state.Id;
                    selectList.Text = state.Name;

                    stateList.Add(selectList);
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateLocalGovernmentSelectListItem()
        {
            try
            {
                LocalGovernmentLogic localGovernmentLogic = new LocalGovernmentLogic();
                List<LocalGovernment> lgas = localGovernmentLogic.GetAll();

                if (lgas == null || lgas.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                stateList.Add(list);

                foreach (LocalGovernment lga in lgas)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = lga.Id.ToString();
                    selectList.Text = lga.Name;

                    stateList.Add(selectList);
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateLocalGovernmentSelectListItemByStateId(string id)
        {
            try
            {
                LocalGovernmentLogic localGovernmentLogic = new LocalGovernmentLogic();
                List<LocalGovernment> lgas = localGovernmentLogic.GetModelsBy(l => l.State_Id == id);

                if (lgas == null || lgas.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                stateList.Add(list);

                foreach (LocalGovernment lga in lgas)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = lga.Id.ToString();
                    selectList.Text = lga.Name;

                    stateList.Add(selectList);
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateFacultySelectListItem()
        {
            try
            {
                FacultyLogic facultyLogic = new FacultyLogic();
                List<Faculty> faculties = facultyLogic.GetAll();
                if (faculties == null || faculties.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                stateList.Add(list);

                foreach (Faculty faculty in faculties)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = faculty.Id.ToString();
                    selectList.Text = faculty.Name;


                    stateList.Add(selectList);
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateAllDepartmentSelectListItem()
        {
            try
            {
                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetModelsBy(y=>y.Active);

                if (departments == null || departments.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();
                if (departments != null && departments.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    stateList.Add(list);

                    foreach (Department department in departments)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = department.Id.ToString();
                        selectList.Text = department.Name;
                        stateList.Add(selectList);
                    }
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public static List<SelectListItem> PopulateDepartmentSelectListItem(Programme programme)
        {
            try
            {
                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetBy(programme);

                if (departments == null || departments.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();
                if (departments != null && departments.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    stateList.Add(list);


                    foreach (Department department in departments)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = department.Id.ToString();
                        selectList.Text = department.Name;

                        stateList.Add(selectList);
                    }
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateDepartmentSelectListItem()
        {
            try
            {
                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetModelsBy(d=>d.Active);

                if (departments == null || departments.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();
                if (departments != null && departments.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    stateList.Add(list);


                    foreach (Department department in departments)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = department.Id.ToString();
                        selectList.Text = department.Name;

                        stateList.Add(selectList);
                    }
                }

                return stateList.OrderBy(f=>f.Text).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateDepartmentSelectListItemByFacultyId(int id)
        {
            try
            {
                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetModelsBy(l => l.Faculty_Id == id);

                if (departments == null || departments.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                stateList.Add(list);

                foreach (Department department in departments)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = department.Id.ToString();
                    selectList.Text = department.Name;

                    stateList.Add(selectList);
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateRelationshipSelectListItem()
        {
            try
            {
                RelationshipLogic relationshipLogic = new RelationshipLogic();
                List<Relationship> relationships = relationshipLogic.GetAll();

                if (relationships == null || relationships.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                stateList.Add(list);

                foreach (Relationship relationship in relationships)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = relationship.Id.ToString();
                    selectList.Text = relationship.Name;

                    stateList.Add(selectList);
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateOLevelTypeSelectListItem()
        {
            try
            {
                OLevelTypeLogic oLevelTypeLogic = new OLevelTypeLogic();
                List<OLevelType> oLevelTypes = oLevelTypeLogic.GetAll();

                if (oLevelTypes == null || oLevelTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> stateList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "0";
                list.Text = Select;
                stateList.Add(list);

                foreach (OLevelType oLevelType in oLevelTypes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = oLevelType.Id.ToString();
                    selectList.Text = oLevelType.Name;

                    stateList.Add(selectList);
                }

                return stateList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateOLevelGradeSelectListItem()
        {
            try
            {
                OLevelGradeLogic oLevelGradeLogic = new OLevelGradeLogic();
                List<OLevelGrade> oLevelGrades = oLevelGradeLogic.GetAll();

                if (oLevelGrades == null || oLevelGrades.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "0";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (OLevelGrade oLevelGrade in oLevelGrades)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = oLevelGrade.Id.ToString();
                    selectList.Text = oLevelGrade.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateOLevelSubjectSelectListItem()
        {
            try
            {
                OLevelSubjectLogic oLevelSubjectLogic = new OLevelSubjectLogic();
                List<OLevelSubject> oLevelSubjects = oLevelSubjectLogic.GetAll();

                if (oLevelSubjects == null || oLevelSubjects.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "0";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (OLevelSubject oLevelSubject in oLevelSubjects)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = oLevelSubject.Id.ToString();
                    selectList.Text = oLevelSubject.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        

        public static List<SelectListItem> PopulateResultGradeSelectListItem()
        {
            try
            {
                ResultGradeLogic resultGradeLogic = new ResultGradeLogic();
                List<ResultGrade> resultGrades = resultGradeLogic.GetAll();

                if (resultGrades == null || resultGrades.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (ResultGrade resultGrade in resultGrades)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = resultGrade.Id.ToString();
                    selectList.Text = resultGrade.LevelOfPass;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateAllProgrammeSelectListItem()
        {
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                List<Programme> programmes = programmeLogic.GetAll();

                if (programmes == null || programmes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectProgramme;
                selectItemList.Add(list);

                foreach (Programme programme in programmes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = programme.Id.ToString();
                    selectList.Text = programme.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateProgrammeSelectListItem()
        {
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                List<Programme> programmes = programmeLogic.GetModelsBy(p => p.Activated == true);

                if (programmes == null || programmes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectProgramme;
                selectItemList.Add(list);

                foreach (Programme programme in programmes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = programme.Id.ToString();
                    selectList.Text = programme.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateNBTEProgrammeSelectListItem()
        {
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                List<Programme> programmes = programmeLogic.GetModelsBy(p => p.Activated == true && (p.Programme_Id == (int)Programmes.HNDDistance || p.Programme_Id == (int)Programmes.NDDistance));

                if (programmes == null || programmes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectProgramme;
                selectItemList.Add(list);

                foreach (Programme programme in programmes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = programme.Id.ToString();
                    selectList.Text = programme.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateApplicationProgrammeSelectListItem()
        {
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                List<Programme> programmes = programmeLogic.GetModelsBy(p => p.ActiveFor_Application == true);

                if (programmes == null || programmes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectProgramme;
                selectItemList.Add(list);

                foreach (Programme programme in programmes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = programme.Id.ToString();
                    selectList.Text = programme.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateInstitutionChoiceSelectListItem()
        {
            try
            {
                InstitutionChoiceLogic institutionChoiceLogic = new InstitutionChoiceLogic();
                List<InstitutionChoice> institutionChoices = institutionChoiceLogic.GetAll();

                if (institutionChoices == null || institutionChoices.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "0";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (InstitutionChoice institutionChoice in institutionChoices)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = institutionChoice.Id.ToString();
                    selectList.Text = institutionChoice.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateITDurationSelectListItem()
        {
            try
            {
                ITDurationLogic itDurationLogic = new ITDurationLogic();
                List<ITDuration> iTDurations = itDurationLogic.GetAll();

                if (iTDurations == null || iTDurations.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (ITDuration iTDuration in iTDurations)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = iTDuration.Id.ToString();
                    selectList.Text = iTDuration.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
         public static List<SelectListItem> PopulateSessionSelectListItemById(int id)
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                List<Session> sessions = sessionLogic.GetModelsBy(a => a.Session_Id == id);

                if (sessions == null || sessions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectSession;
                selectItemList.Add(list);

                foreach (Session session in sessions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = session.Id.ToString();
                    selectList.Text = session.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
         public static List<SelectListItem> PopulateResultSessionSelectListItem()
         {
             try
             {
                 SessionLogic sessionLogic = new SessionLogic();
                 List<Session> sessions = sessionLogic.GetModelsBy(a => a.Active_For_Result == true);

                 if (sessions == null || sessions.Count <= 0)
                 {
                     return new List<SelectListItem>();
                 }

                 List<SelectListItem> selectItemList = new List<SelectListItem>();

                 SelectListItem list = new SelectListItem();
                 list.Value = "";
                 list.Text = SelectSession;
                 selectItemList.Add(list);

                 foreach (Session session in sessions)
                 {
                     SelectListItem selectList = new SelectListItem();
                     selectList.Value = session.Id.ToString();
                     selectList.Text = session.Name;

                     selectItemList.Add(selectList);
                 }

                 return selectItemList;
             }
             catch (Exception)
             {
                 throw;
             }
         }
         public static List<SelectListItem> PopulateAllocationSessionSelectListItem()
         {
             try
             {
                 SessionLogic sessionLogic = new SessionLogic();
                 List<Session> sessions = sessionLogic.GetModelsBy(a => a.Active_For_Allocation == true);

                 if (sessions == null || sessions.Count <= 0)
                 {
                     return new List<SelectListItem>();
                 }

                 List<SelectListItem> selectItemList = new List<SelectListItem>();

                 SelectListItem list = new SelectListItem();
                 list.Value = "";
                 list.Text = SelectSession;
                 selectItemList.Add(list);

                 foreach (Session session in sessions)
                 {
                     SelectListItem selectList = new SelectListItem();
                     selectList.Value = session.Id.ToString();
                     selectList.Text = session.Name;

                     selectItemList.Add(selectList);
                 }

                 return selectItemList;
             }
             catch (Exception)
             {
                 throw;
             }
         }
         public static List<SelectListItem> PopulatePastSessionSelectListItem()
         {
             try
             {
                 SessionLogic sessionLogic = new SessionLogic();
                 List<Session> sessions = sessionLogic.GetModelsBy(a => a.Session_Id == 1 || a.Session_Id == 7 || a.Active_For_Result == true);

                 if (sessions == null || sessions.Count <= 0)
                 {
                     return new List<SelectListItem>();
                 }

                 List<SelectListItem> selectItemList = new List<SelectListItem>();

                 SelectListItem list = new SelectListItem();
                 list.Value = "";
                 list.Text = SelectSession;
                 selectItemList.Add(list);

                 foreach (Session session in sessions)
                 {
                     SelectListItem selectList = new SelectListItem();
                     selectList.Value = session.Id.ToString();
                     selectList.Text = session.Name;

                     selectItemList.Add(selectList);
                 }

                 return selectItemList;
             }
             catch (Exception)
             {
                 throw;
             }
         }
       
        public static List<SelectListItem> PopulateSessionSelectListItem()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                List<Session> sessions = sessionLogic.GetActiveSessions();

                if (sessions == null || sessions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectSession;
                selectItemList.Add(list);

                foreach (Session session in sessions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = session.Id.ToString();
                    selectList.Text = session.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateAllSessionSelectListItem()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                List<Session> sessions = sessionLogic.GetAll().OrderByDescending(k => k.Name).ToList();

                if (sessions == null || sessions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectSession;
                selectItemList.Add(list);

                foreach (Session session in sessions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = session.Id.ToString();
                    selectList.Text = session.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateAllActivatedSessionSelectListItem()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                List<Session> sessions = sessionLogic.GetActiveSessions().OrderByDescending(k => k.Name).ToList();

                if (sessions == null || sessions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectSession;
                selectItemList.Add(list);

                foreach (Session session in sessions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = session.Id.ToString();
                    selectList.Text = session.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static List<SelectListItem> PopulateSemesterSelectListItem()
        {
            try
            {
                SemesterLogic semesterLogic = new SemesterLogic();
                List<Semester> semesters = semesterLogic.GetAll();

                if (semesters == null || semesters.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = "--Select Semester--";
                selectItemList.Add(list);

                foreach (Semester semester in semesters)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = semester.Id.ToString();
                    selectList.Text = semester.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateSingleSessionSelectListItem(int Id)
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                List<Session> sessions = sessionLogic.GetModelsBy(k => k.Session_Id == Id).ToList();

                if (sessions == null || sessions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectSession;
                selectItemList.Add(list);

                foreach (Session session in sessions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = session.Id.ToString();
                    selectList.Text = session.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateFeeSessionSelectListItem()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                List<Session> sessions = sessionLogic.GetFeeSession();

                if (sessions == null || sessions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectSession;
                selectItemList.Add(list);

                foreach (Session session in sessions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = session.Id.ToString();
                    selectList.Text = session.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        } 
        
        public static List<SelectListItem> PopulateHostels()
        {
            try
            {
                HostelLogic hostelLogic = new HostelLogic();
                List<Hostel> hostels = hostelLogic.GetModelsBy(a => a.Activated == true).OrderByDescending(k => k.Name).ToList();

                if (hostels == null || hostels.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = "-- Select Hostel --";
                selectItemList.Add(list);

                foreach (Hostel hostel in hostels)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = hostel.Id.ToString();
                    selectList.Text = hostel.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateHostelSeries(Hostel hosetl)
        {
            try
            {
                HostelSeriesLogic hostelSeriesLogic = new HostelSeriesLogic();
                List<HostelSeries> hostelSeries = hostelSeriesLogic.GetModelsBy(a => a.Hostel_Id == hosetl.Id && a.Activated == true).OrderByDescending(k => k.Name).ToList();

                if (hostelSeries == null || hostelSeries.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (HostelSeries series in hostelSeries)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = series.Id.ToString();
                    selectList.Text = series.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateHostelRoomCorners(HostelRoom hostelRoom)
        {
            try
            {
                HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();
                List<HostelRoomCorner> hostelRoomCorners = hostelRoomCornerLogic.GetModelsBy(a => a.Room_Id == hostelRoom.Id && a.Activated == true).OrderByDescending(k => k.Name).ToList();

                if (hostelRoomCorners == null || hostelRoomCorners.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (HostelRoomCorner roomCorner in hostelRoomCorners)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = roomCorner.Id.ToString();
                    selectList.Text = roomCorner.Name;
                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateHostelRooms(HostelSeries HostelSeries)
        {
            try
            {
                HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                List<HostelRoom> hostelRooms = hostelRoomLogic.GetModelsBy(a => a.Series_Id == HostelSeries.Id && a.Activated == true).OrderByDescending(k => k.Number).ToList();

                if (hostelRooms == null || hostelRooms.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (HostelRoom room in hostelRooms)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = room.Id.ToString();
                    selectList.Text = room.Number;
                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        //public static List<SelectListItem> PopulateHostels()
        //{
        //    try
        //    {
        //        HostelLogic hostelLogic = new HostelLogic();
        //        List<Hostel> hostels = hostelLogic.GetModelsBy(a=> a.Activated == true).OrderByDescending(k => k.Name).ToList();

        //        if (hostels == null || hostels.Count <= 0)
        //        {
        //            return new List<SelectListItem>();
        //        }

        //        List<SelectListItem> selectItemList = new List<SelectListItem>();

        //        SelectListItem list = new SelectListItem();
        //        list.Value = "";
        //        list.Text = Select;
        //        selectItemList.Add(list);

        //        foreach (Hostel hostel in hostels)
        //        {
        //            SelectListItem selectList = new SelectListItem();
        //            selectList.Value = hostel.Id.ToString();
        //            selectList.Text = hostel.Name;

        //            selectItemList.Add(selectList);
        //        }

        //        return selectItemList;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //public static List<SelectListItem> PopulateHostelSeries(Hostel hosetl)
        //{
        //    try
        //    {
        //        HostelSeriesLogic hostelSeriesLogic = new HostelSeriesLogic();
        //        List<HostelSeries> hostelSeries = hostelSeriesLogic.GetModelsBy(a=> a.Hostel_Id == hosetl.Id && a.Activated == true).OrderByDescending(k => k.Name).ToList();

        //        if (hostelSeries == null || hostelSeries.Count <= 0)
        //        {
        //            return new List<SelectListItem>();
        //        }

        //        List<SelectListItem> selectItemList = new List<SelectListItem>();

        //        SelectListItem list = new SelectListItem();
        //        list.Value = "";
        //        list.Text = Select;
        //        selectItemList.Add(list);

        //        foreach (HostelSeries series in hostelSeries)
        //        {
        //            SelectListItem selectList = new SelectListItem();
        //            selectList.Value = series.Id.ToString();
        //            selectList.Text = series.Name;

        //            selectItemList.Add(selectList);
        //        }

        //        return selectItemList;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public static List<SelectListItem> PopulateHostelRooms(HostelSeries HostelSeries)
        //{
        //    try
        //    {
        //        HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
        //        List<HostelRoom> hostelRooms = hostelRoomLogic.GetModelsBy(a=> a.Series_Id == HostelSeries.Id && a.Activated == true).OrderByDescending(k => k.Number).ToList();

        //        if (hostelRooms == null || hostelRooms.Count <= 0)
        //        {
        //            return new List<SelectListItem>();
        //        }

        //        List<SelectListItem> selectItemList = new List<SelectListItem>();

        //        SelectListItem list = new SelectListItem();
        //        list.Value = "";
        //        list.Text = Select;
        //        selectItemList.Add(list);

        //        foreach (HostelRoom room in hostelRooms)
        //        {
        //            SelectListItem selectList = new SelectListItem();
        //            selectList.Value = room.Id.ToString();
        //            selectList.Text = room.Number;
        //            selectItemList.Add(selectList);
        //        }

        //        return selectItemList;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

     
     
        public static List<Session> GetAllSessions()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                List<Session> sessions = sessionLogic.GetAll();

                if (sessions != null && sessions.Count > 0)
                {
                    sessions.Insert(0, new Session() { Id = 0, Name = SelectSession });
                }

                return sessions;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SessionSemester> GetAllSessionSemesters()
        {
            try
            {
                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                List<SessionSemester> sessionSemesters = sessionSemesterLogic.GetAll();

                if (sessionSemesters != null && sessionSemesters.Count > 0)
                {
                    sessionSemesters.Insert(0, new SessionSemester() { Id = 0, Name = SelectSemester });
                }

                return sessionSemesters;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<Level> GetAllLevels()
        {
            try
            {
                LevelLogic levelLogic = new LevelLogic();
                List<Level> levels = levelLogic.GetAll();

                if (levels != null && levels.Count > 0)
                {
                    levels.Insert(0, new Level() { Id = 0, Name = SelectLevel });
                }

                return levels;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<Level> GetFinalLevels()
        {
            try
            {
                LevelLogic levelLogic = new LevelLogic();
                int[] finalYearlevels = {(int) Levels.HNDII, (int) Levels.NDII};
                List<Level> levels = levelLogic.GetModelsBy(l => finalYearlevels.Contains(l.Level_Id));

                if (levels != null && levels.Count > 0)
                {
                    levels.Insert(0, new Level() { Id = 0, Name = SelectLevel });
                }

                return levels;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<Level> GetFirstYearLevels()
        {
            try
            {
                LevelLogic levelLogic = new LevelLogic();
                int[] finalYearlevels = { (int)Levels.HNDI, (int)Levels.NDI };
                List<Level> levels = levelLogic.GetModelsBy(l => finalYearlevels.Contains(l.Level_Id));

                if (levels != null && levels.Count > 0)
                {
                    levels.Insert(0, new Level() { Id = 0, Name = SelectLevel });
                }

                return levels;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<Programme> GetAllProgrammes()
        {
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                List<Programme> programmes = programmeLogic.GetAll();

                if (programmes != null && programmes.Count > 0)
                {
                    //programmes.Add(new Programme() { Id = -100, Name = "All" });
                    programmes.Insert(0, new Programme() { Id = 0, Name = SelectProgramme });
                }

                return programmes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<Programme> GetODfelProgrammes()
        {
            try
            {
                ProgrammeLogic programmeLogic = new ProgrammeLogic();
                List<Programme> programmes = programmeLogic.GetModelsBy(x => x.Programme_Id == (int)Programmes.HNDDistance || x.Programme_Id == (int)Programmes.NDDistance);

                if (programmes != null && programmes.Count > 0)
                {
                    //programmes.Add(new Programme() { Id = -100, Name = "All" });
                    programmes.Insert(0, new Programme() { Id = 0, Name = SelectProgramme });
                }

                return programmes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<State> GetAllStates()
        {
            try
            {
                StateLogic programmeLogic = new StateLogic();
                List<State> states = programmeLogic.GetAll();

                if (states != null && states.Count > 0)
                {
                    states.Insert(0, new State() { Id = "", Name = SelectState });
                }

                return states;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateMonthSelectListItem()
        {
            try
            {
                List<Value> months = new List<Value>();
                Value january = new Value() { Id = 1, Name = "January" };
                Value february = new Value() { Id = 2, Name = "February" };
                Value march = new Value() { Id = 3, Name = "March" };
                Value april = new Value() { Id = 4, Name = "April" };
                Value may = new Value() { Id = 5, Name = "May" };
                Value june = new Value() { Id = 6, Name = "June" };
                Value july = new Value() { Id = 7, Name = "July" };
                Value august = new Value() { Id = 8, Name = "August" };
                Value september = new Value() { Id = 9, Name = "September" };
                Value october = new Value() { Id = 10, Name = "October" };
                Value november = new Value() { Id = 11, Name = "November" };
                Value december = new Value() { Id = 12, Name = "December" };

                months.Add(january);
                months.Add(february);
                months.Add(march);
                months.Add(april);
                months.Add(may);
                months.Add(june);
                months.Add(july);
                months.Add(august);
                months.Add(september);
                months.Add(october);
                months.Add(november);
                months.Add(december);
                              
                List<SelectListItem> monthList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = "--MM--";
                monthList.Add(list);

                foreach (Value month in months)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = month.Id.ToString();
                    selectList.Text = month.Name;

                    monthList.Add(selectList);
                }

                return monthList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateDaySelectListItem(Value month, Value year)
        {
            try
            {
                List<Value> days = GetNumberOfDaysInMonth(month, year);

                List<SelectListItem> dayList = new List<SelectListItem>();
                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = "--DD--";

                dayList.Add(list);
                foreach (Value day in days)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = day.Id.ToString();
                    selectList.Text = day.Name;

                    dayList.Add(selectList);
                }

                return dayList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<Value> GetNumberOfDaysInMonth(Value month, Value year)
        {
            try
            {
                int noOfDays = DateTime.DaysInMonth(year.Id, month.Id);
                List<Value> days = CreateNumberListFrom(1, noOfDays);
                return days;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static bool IsDateInTheFuture(DateTime date)
        {
            try
            {
                TimeSpan difference = DateTime.Now - date;
                if (difference.Days <= 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsStartDateGreaterThanEndDate(DateTime startDate, DateTime endDate)
        {
            try
            {
                TimeSpan difference = endDate - startDate;
                if (difference.Days <= 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsDateOutOfRange(DateTime startDate, DateTime endDate, int noOfDays)
        {
            try
            {
                TimeSpan difference = endDate - startDate;
                if (difference.Days < noOfDays)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateTitleSelectListItem()
        {
            try
            {
                TitleLogic titleLogic = new TitleLogic();
                List<Title> titles = titleLogic.GetAll();

                if (titles == null || titles.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> titlesList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                titlesList.Add(list);

                foreach (Title title in titles)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = title.Id.ToString();
                    selectList.Text = title.Name;

                    titlesList.Add(selectList);
                }

                return titlesList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateNBTECoursesSelectListItem(long programmeId)
        {
            try
            {
                CourseAllocationLogic allocationLogic = new CourseAllocationLogic();
                List<CourseAllocation> courseAllocations = allocationLogic.GetModelsBy(x => x.Programme_Id == programmeId && x.SESSION.Activated == true);

                if (courseAllocations == null || courseAllocations.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> allocationList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                allocationList.Add(list);

                foreach (CourseAllocation title in courseAllocations)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = title.Id.ToString();
                    selectList.Text = title.Course.Name;

                    allocationList.Add(selectList);
                }

                return allocationList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateCourseAllocationNBTESelectListItem()
        {
            try
            {
                CourseAllocationLogic allocationLogic = new CourseAllocationLogic();
                List<CourseAllocation> courseAllocations = allocationLogic.GetModelsBy(x => (x.Programme_Id == (int)Programmes.HNDDistance || x.Programme_Id == (int)Programmes.NDDistance) && x.SESSION.Activated == true);

                if (courseAllocations == null || courseAllocations.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> allocationList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                allocationList.Add(list);

                foreach (CourseAllocation title in courseAllocations)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = title.Id.ToString();
                    selectList.Text = title.Course.Name;

                    allocationList.Add(selectList);
                }

                return allocationList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateCourseAllocationSelectListItem(long userId)
        {
            try
            {
                CourseAllocationLogic allocationLogic = new CourseAllocationLogic();
                List<CourseAllocation> courseAllocations = allocationLogic.GetModelsBy(x => x.User_Id == userId && x.SESSION.Activated == true);

                if (courseAllocations == null || courseAllocations.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> allocationList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                allocationList.Add(list);

                foreach (CourseAllocation title in courseAllocations)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = title.Id.ToString();
                    selectList.Text = title.Course.Name;

                    allocationList.Add(selectList);
                }

                return allocationList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateMaritalStatusSelectListItem()
        {
            try
            {
                MaritalStatusLogic maritalStatusLogic = new MaritalStatusLogic();
                List<MaritalStatus> maritalStatuses = maritalStatusLogic.GetAll();

                if (maritalStatuses == null || maritalStatuses.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> maritalStatusesList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                maritalStatusesList.Add(list);

                foreach (MaritalStatus title in maritalStatuses)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = title.Id.ToString();
                    selectList.Text = title.Name;

                    maritalStatusesList.Add(selectList);
                }

                return maritalStatusesList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateBloodGroupSelectListItem()
        {
            try
            {
                BloodGroupLogic bloodGroupLogic = new BloodGroupLogic();
                List<BloodGroup> bloodGroups = bloodGroupLogic.GetAll();

                if (bloodGroups == null || bloodGroups.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> bloodGroupsList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "0";
                list.Text = Select;
                bloodGroupsList.Add(list);

                foreach (BloodGroup bloodGroup in bloodGroups)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = bloodGroup.Id.ToString();
                    selectList.Text = bloodGroup.Name;

                    bloodGroupsList.Add(selectList);
                }

                return bloodGroupsList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateGenotypeSelectListItem()
        {
            try
            {
                GenotypeLogic genotypeLogic = new GenotypeLogic();
                List<Genotype> genotypes = genotypeLogic.GetAll();

                if (genotypes == null || genotypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> genotypeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "0";
                list.Text = Select;
                genotypeList.Add(list);

                foreach (Genotype genotype in genotypes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = genotype.Id.ToString();
                    selectList.Text = genotype.Name;

                    genotypeList.Add(selectList);
                }

                return genotypeList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateModeOfEntrySelectListItem()
        {
            try
            {
                ModeOfEntryLogic modeOfEntryLogic = new ModeOfEntryLogic();
                List<ModeOfEntry> modeOfEntries = modeOfEntryLogic.GetAll();

                if (modeOfEntries == null || modeOfEntries.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> modeOfEntryList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                modeOfEntryList.Add(list);

                foreach (ModeOfEntry modeOfEntry in modeOfEntries)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = modeOfEntry.Id.ToString();
                    selectList.Text = modeOfEntry.Name;

                    modeOfEntryList.Add(selectList);
                }

                return modeOfEntryList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateModeOfStudySelectListItem()
        {
            try
            {
                ModeOfStudyLogic modeOfStudyLogic = new ModeOfStudyLogic();
                List<ModeOfStudy> modeOfStudies = modeOfStudyLogic.GetAll();

                if (modeOfStudies == null || modeOfStudies.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> modeOfStudyList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                modeOfStudyList.Add(list);

                foreach (ModeOfStudy modeOfStudy in modeOfStudies)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = modeOfStudy.Id.ToString();
                    selectList.Text = modeOfStudy.Name;

                    modeOfStudyList.Add(selectList);
                }

                return modeOfStudyList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateStudentTypeSelectListItem()
        {
            try
            {
                StudentTypeLogic studentTypeLogic = new StudentTypeLogic();
                List<StudentType> studentTypes = studentTypeLogic.GetAll();

                if (studentTypes == null || studentTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> studentTypeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                studentTypeList.Add(list);

                foreach (StudentType studentType in studentTypes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = studentType.Id.ToString();
                    selectList.Text = studentType.Name;

                    studentTypeList.Add(selectList);
                }

                return studentTypeList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateStudentStatusSelectListItem()
        {
            try
            {
                StudentStatusLogic studentStatusLogic = new StudentStatusLogic();
                List<StudentStatus> studentStatuses = studentStatusLogic.GetAll();

                if (studentStatuses == null || studentStatuses.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> studentStatusList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                studentStatusList.Add(list);

                foreach (StudentStatus studentStatus in studentStatuses)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = studentStatus.Id.ToString();
                    selectList.Text = studentStatus.Name;

                    studentStatusList.Add(selectList);
                }

                return studentStatusList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateLevelSelectListItem()
        {
            try
            {
                LevelLogic levelLogic = new LevelLogic();
                List<Level> levels = levelLogic.GetAll();

                if (levels == null || levels.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> levelList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                levelList.Add(list);

                foreach (Level level in levels)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = level.Id.ToString();
                    selectList.Text = level.Name;

                    levelList.Add(selectList);
                }

                return levelList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateModeOfFinanceSelectListItem()
        {
            try
            {
                ModeOfFinanceLogic modeOfFinanceLogic = new ModeOfFinanceLogic();
                List<ModeOfFinance> modeOfFinances = modeOfFinanceLogic.GetAll();

                if (modeOfFinances == null || modeOfFinances.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> modeOfFinanceList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                modeOfFinanceList.Add(list);

                foreach (ModeOfFinance modeOfFinance in modeOfFinances)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = modeOfFinance.Id.ToString();
                    selectList.Text = modeOfFinance.Name;

                    modeOfFinanceList.Add(selectList);
                }


                return modeOfFinanceList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulatePaymentModeSelectListItemDistantLearning(Payment payment)
        {
            try
            {
                PaymentModeLogic paymentModeLogic = new PaymentModeLogic();
                RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                PaymentLogic paymentLogic = new PaymentLogic();
                //var payments = paymentLogic.GetModelsBy(x => x.Person_Id == payment.Person.Id && x.Fee_Type_Id == 3 && x.Session_Id == payment.Session.Id);
                var payments = remitaPaymentLogic.GetModelsBy(x => x.PAYMENT.Person_Id == payment.Person.Id && x.PAYMENT.Fee_Type_Id == 3 && x.PAYMENT.Session_Id == payment.Session.Id && (x.Status.Contains("01") || x.Description.Contains("manual")));
                List<PaymentMode> paymentModes = paymentModeLogic.GetAll();

                if (paymentModes == null || paymentModes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> paymentModeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                paymentModeList.Add(list);
                if(payments != null && payments.Count == 1)
                {
                    if(payments[0].payment.PaymentMode.Id == 1)
                    {
                        return paymentModeList;
                    }
                }
                foreach (PaymentMode paymentmode in paymentModes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = paymentmode.Id.ToString();
                    selectList.Text = paymentmode.Name;
                       
                        if (paymentmode.Id != 2)
                         {
                            paymentModeList.Add(selectList);
                         }                     
                }
               if(payments != null && payments.Count > 0)
                {
                    foreach (var paymentMode in payments)
                    {
                        PaymentMode paymentmode = new PaymentMode();
                        paymentmode = paymentMode.payment.PaymentMode;
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = paymentmode.Id.ToString();
                        selectList.Text = paymentmode.Name;

                        if (paymentmode.Id == paymentmode.Id || paymentmode.Id == 1)
                        {
                            paymentModeList.Remove(selectList);
                        }
                    }

                }

                return paymentModeList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulatePaymentModeSelectListItemOdfel()
        {
            try
            {
                PaymentModeLogic paymentModeLogic = new PaymentModeLogic();
                List<PaymentMode> paymentModes = paymentModeLogic.GetAll();

                if (paymentModes == null || paymentModes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> paymentModeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                paymentModeList.Add(list);

                foreach (PaymentMode paymentmode in paymentModes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = paymentmode.Id.ToString();
                    selectList.Text = paymentmode.Name;
                    if(paymentmode.Id != 2)
                    {
                        paymentModeList.Add(selectList);
                    }
                }


                return paymentModeList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        



        public static List<SelectListItem> PopulatePaymentModeSelectListItem()
        {
            try
            {
                PaymentModeLogic paymentModeLogic = new PaymentModeLogic();
                List<PaymentMode> paymentModes = paymentModeLogic.GetAll();

                if (paymentModes == null || paymentModes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> paymentModeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                paymentModeList.Add(list);

                foreach (PaymentMode paymentmode in paymentModes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = paymentmode.Id.ToString();
                    selectList.Text = paymentmode.Name;

                    paymentModeList.Add(selectList);
                }


                return paymentModeList;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public static List<SelectListItem> PopulateDepartmentSelectListItemBy(Programme programme)
        {
            try
            {
                ProgrammeDepartmentLogic programmeDepartmentLogic = new ProgrammeDepartmentLogic();
                List<Department> departments = programmeDepartmentLogic.GetBy(programme);

                if (departments == null || departments.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> departmentList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                departmentList.Add(list);

                foreach (Department department in departments)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = department.Id.ToString();
                    selectList.Text = department.Name;

                    departmentList.Add(selectList);
                }
                
                return departmentList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateDepartmentOptionSelectListItem(Department department, Programme programme)
        {
            try
            {
                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                List<DepartmentOption> departmentOPtions = departmentOptionLogic.GetBy(department, programme);

                if (departmentOPtions == null && departmentOPtions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> departmentOptionList = new List<SelectListItem>();
                if (departmentOPtions != null && departmentOPtions.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    departmentOptionList.Add(list);
                    
                    foreach (DepartmentOption departmentOption in departmentOPtions)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = departmentOption.Id.ToString();
                        selectList.Text = departmentOption.Name;

                        departmentOptionList.Add(selectList);
                    }
                }

                return departmentOptionList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateStudentCategorySelectListItem()
        {
            try
            {
                StudentCategoryLogic studentCategoryLogic = new StudentCategoryLogic();
                List<StudentCategory> studentCategories = studentCategoryLogic.GetAll();

                if (studentCategories == null && studentCategories.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> studentCategoryList = new List<SelectListItem>();
                if (studentCategories != null && studentCategories.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    studentCategoryList.Add(list);

                    foreach (StudentCategory studentCategory in studentCategories)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = studentCategory.Id.ToString();
                        selectList.Text = studentCategory.Name;

                        studentCategoryList.Add(selectList);
                    }
                }

                return studentCategoryList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public static List<SelectListItem> PopulateStudentResultTypeSelectListItem()
        {
            try
            {
                StudentResultTypeLogic studentResultTypeLogic = new StudentResultTypeLogic();
                List<StudentResultType> studentResultTypes = studentResultTypeLogic.GetAll();

                if (studentResultTypes == null && studentResultTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> studentResultTypeList = new List<SelectListItem>();
                if (studentResultTypes != null && studentResultTypes.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    studentResultTypeList.Add(list);

                    foreach (StudentResultType studentResultType in studentResultTypes)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = studentResultType.Id.ToString();
                        selectList.Text = studentResultType.Name;

                        studentResultTypeList.Add(selectList);
                    }
                }

                return studentResultTypeList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateSessionSemesterSelectListItem()
        {
            try
            {
                SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
                List<SessionSemester> sessionSemesters = sessionSemesterLogic.GetAll();

                if (sessionSemesters == null && sessionSemesters.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> sessionSemesterList = new List<SelectListItem>();
                if (sessionSemesters != null && sessionSemesters.Count > 0)
                {
                    SelectListItem list = new SelectListItem();
                    list.Value = "";
                    list.Text = Select;
                    sessionSemesterList.Add(list);

                    foreach (SessionSemester sessionSemester in sessionSemesters)
                    {
                        SelectListItem selectList = new SelectListItem();
                        selectList.Value = sessionSemester.Id.ToString();
                        selectList.Text = sessionSemester.Name;

                        sessionSemesterList.Add(selectList);
                    }
                }

                return sessionSemesterList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<Department> GetDepartmentByProgramme(Programme programme)
        {
            try
            {
                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetBy(programme);

                if (departments != null && departments.Count > 0)
                {
                    departments.Insert(0, new Department() { Id = 0, Name = SelectDepartment });
                }

                return departments;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<DepartmentOption> GetDepartmentByOptionByDepartmentProgramme(Department department,Programme programme)
        {
            try
            {
                DepartmentOptionLogic departmentOptionLogic = new DepartmentOptionLogic();
                List<DepartmentOption> departments = departmentOptionLogic.GetBy(department,programme);

                if (departments != null && departments.Count > 0)
                {
                    departments.Insert(0, new DepartmentOption() { Id = 0, Name = SelectDepartmentOption });
                }

                return departments;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static List<SelectListItem> PopulateAdmissionListTypeSelectListItem()
        {
            try
            {

                AdmissionListTypeLogic admissionListTypeLogic = new AdmissionListTypeLogic();
                List<AdmissionListType> admissionListTypes = admissionListTypeLogic.GetAll();

                if (admissionListTypes == null || admissionListTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> admissionListTypeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = SelectAdmissiontype;
                admissionListTypeList.Add(list);

                foreach (AdmissionListType admissionListType in admissionListTypes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = admissionListType.Id.ToString();
                    selectList.Text = admissionListType.Name;

                    admissionListTypeList.Add(selectList);
                }

                return admissionListTypeList;
            }
            catch (Exception)
            {
                throw;
            }
        }
 
    
        public static List<Course> GetCoursesByLevelDepartmentAndSemester(Programme programme,Level level, Department department, Semester semester)
        {
            try
            {
                CourseLogic courseLogic = new CourseLogic();
                List<Course> courses = courseLogic.GetBy(programme,department, level, semester);

                if (courses != null && courses.Count > 0)
                {
                    courses.Insert(0, new Course() { Id = 0, Name = SelectCourse });
                }

                return courses;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<Course> GetCoursesByOptionLevelDepartmentAndSemester(Programme programme,DepartmentOption option, Level level, Department department, Semester semester)
        {
            try
            {
                CourseLogic courseLogic = new CourseLogic();
                List<Course> courses = new List<Course>();
                if (option != null && option.Id > 0)
                {
                    courses = courseLogic.GetBy(programme,department, option, level, semester, true);
                }
                else
                {
                    courses = courseLogic.GetBy(programme,department, level, semester, true);
                }

                if (courses != null && courses.Count > 0)
                {
                    courses.Insert(0, new Course() { Id = 0, Name = SelectCourse });
                }

                return courses;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public static List<Course> GetCoursesByLevelDepartmentAndSemester(Level level, Department department, Semester semester)
        //{
        //    try
        //    {
        //        CourseLogic courseLogic = new CourseLogic();
        //        List<Course> courses = courseLogic.GetBy(department, level, semester);

        //        if (courses != null && courses.Count > 0)
        //        {
        //            courses.Insert(0, new Course() { Id = 0, Name = SelectCourse });
        //        }

        //        return courses;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}


        public static List<Student> GetStudentsBy(Level level, Programme programme, Department department, Session session)
        {
            try
            {
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                List<StudentLevel> studentLevels = studentLevelLogic.GetBy(level, programme, department, session);
                //List<StudentLevel> studentLevels = studentLevelLogic.GetModelsBy(f => f.Person_Id == 145351);
                List<Student> students = new List<Student>();
                foreach (StudentLevel studentLevel in studentLevels)
                {
                    if (studentLevel.Student.Activated == false && (studentLevel.Student.Reason!=null && !studentLevel.Student.Reason.Contains("System deactivated")))
                    {
                        continue;
                    }

                    studentLevel.Student.FirstName = studentLevel.Student.MatricNumber + " - " + studentLevel.Student.Name;
                    students.Add(studentLevel.Student);
                }

               return students.OrderBy(s => s.MatricNumber).ToList();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Encrypt(string encrypData)
        {
            string data = "";
            try
            {
                string CharData = "";
                string ConChar = "";
                for (int i = 0; i < encrypData.Length; i++)
                {
                    CharData = Convert.ToString(encrypData.Substring(i, 1));
                    ConChar = char.ConvertFromUtf32(char.ConvertToUtf32(CharData, 0) + 128);
                    data = data + ConChar;

                }

            }
            catch (Exception ex)
            {
                data = "1";
                return data;
            }
            return data;


        }

        public static string Decrypt(string encrypData)
        {
            string data = "";
            try
            {
                string CharData = "";
                string ConChar = "";
                for (int i = 0; i < encrypData.Length; i++)
                {
                    CharData = Convert.ToString(encrypData.Substring(i, 1));
                    ConChar = char.ConvertFromUtf32(char.ConvertToUtf32(CharData, 0) - 128);
                    data = data + ConChar;

                }

            }
            catch (Exception ex)
            {
                data = "1";
                return data;
            }
            return data;


        }

        public static List<SelectListItem> PopulateTranscriptStatusSelectListItem()
        {
            try
            {
                TranscriptStatusLogic transcriptStatusLogic = new TranscriptStatusLogic();
                List<TranscriptStatus> transcripts = transcriptStatusLogic.GetAll();
                if (transcripts == null || transcripts.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> transcriptsList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                transcriptsList.Add(list);

                foreach (TranscriptStatus state in transcripts)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = state.TranscriptStatusId.ToString();
                    selectList.Text = state.TranscriptStatusName;

                    transcriptsList.Add(selectList);
                }

                return transcriptsList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateTranscriptClearanceStatusSelectListItem()
        {
            try
            {
                TranscriptClearanceStatusLogic transcriptStatusLogic = new TranscriptClearanceStatusLogic();
                List<TranscriptClearanceStatus> transcripts = transcriptStatusLogic.GetAll();
                if (transcripts == null || transcripts.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> transcriptsList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                transcriptsList.Add(list);

                foreach (TranscriptClearanceStatus state in transcripts)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = state.TranscriptClearanceStatusId.ToString();
                    selectList.Text = state.TranscriptClearanceStatusName;

                    transcriptsList.Add(selectList);
                }

                return transcriptsList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateCountrySelectListItem()
        {
            try
            {
                CountryLogic countryLogic = new CountryLogic();
                List<Country> countries = countryLogic.GetAll();
                if (countries == null || countries.Count <= 0)
                {
                    return new List<SelectListItem>();
                }
                Country others = countries.Where(x => x.Id == "OTH").LastOrDefault();
                List<SelectListItem> countryList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                countryList.Add(list);

                foreach (Country country in countries)
                {
                    SelectListItem selectList = new SelectListItem();
                    if(country.Id != "OTH")
                    {
                        selectList.Value = country.Id;
                        selectList.Text = country.CountryName;

                        countryList.Add(selectList);
                    }
                   
                }
                if(others != null)
                {
                    SelectListItem othersList = new SelectListItem();
                    othersList.Value = others.Id;
                    othersList.Text = others.CountryName;
                    countryList.Add(othersList);
                }
                return countryList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateStaffSelectListItem()
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                List<User> users = userLogic.GetModelsBy(p => p.ROLE.Role_Name == "CourseStaff").OrderBy(a => a.Username).ToList();
                if (users == null || users.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> userList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                userList.Add(list);

                foreach (User user in users)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = user.Id.ToString();
                    selectList.Text = user.Username;

                    userList.Add(selectList);
                }

                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateStaffAndHODSelectListItem()
        {
            try
            {
                UserLogic userLogic = new UserLogic();
                List<User> users = userLogic.GetModelsBy(p => p.Role_Id == (int)UserRoles.CourseStaff || p.Role_Id == (int)UserRoles.HOD).OrderBy(a => a.Username).ToList();
                if (users == null || users.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> userList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                userList.Add(list);

                foreach (User user in users)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = user.Id.ToString();
                    selectList.Text = user.Username;

                    userList.Add(selectList);
                }

                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateRoleSelectListItem()
        {
            try
            {
                RoleLogic roleLogic = new RoleLogic();
                List<Role> roles = roleLogic.GetAll();
                if (roles == null || roles.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> roleList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                roleList.Add(list);

                foreach (Role role in roles)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = role.Id.ToString();
                    selectList.Text = role.Name;

                    roleList.Add(selectList);
                }

                return roleList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateSecurityQuestionSelectListItem()
        {
            try
            {
                SecurityQuestionLogic securityQuestionLogic = new SecurityQuestionLogic();
                List<SecurityQuestion> questions = securityQuestionLogic.GetAll();
                if (questions == null || questions.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> roleList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                roleList.Add(list);

                foreach (SecurityQuestion question in questions)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = question.Id.ToString();
                    selectList.Text = question.Name;

                    roleList.Add(selectList);
                }

                return roleList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<InstitutionChoice> GetAllInstitutionChoices()
        {
            try
            {
                InstitutionChoiceLogic institutionChoiceLogic = new InstitutionChoiceLogic();
                List<InstitutionChoice> institutionChoices = institutionChoiceLogic.GetAll();

                if (institutionChoices != null && institutionChoices.Count > 0)
                {
                    institutionChoices.Insert(0, new InstitutionChoice() { Id = 0, Name = "-- Select Institution Choice --" });
                }

                return institutionChoices;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateStaffRoleSelectListItem()
        {
            try
            {
                RoleLogic roleLogic = new RoleLogic();
                List<Role> roles = roleLogic.GetAll().OrderBy(x=>x.Id).ToList();
                roles.RemoveAt(0);
                if (roles == null || roles.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> roleList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = "-- Select Role --" ;
                roleList.Add(list);

                foreach (Role role in roles)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = role.Id.ToString();
                    selectList.Text = role.Name;

                    roleList.Add(selectList);
                }

                return roleList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal static List<SelectListItem> PopulateCourseModeSelectListItem()
        {
            try
            {
                CourseModeLogic courseModeLogic = new CourseModeLogic();
                List<CourseMode> courseModes = courseModeLogic.GetAll();
                if (courseModes == null || courseModes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> courseModeList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = "-- Select Course Mode --";
                courseModeList.Add(list);

                foreach (CourseMode courseMode in courseModes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = courseMode.Id.ToString();
                    selectList.Text = courseMode.Name;

                    courseModeList.Add(selectList);
                }

                return courseModeList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    
         public static List<SelectListItem> PopulateMenuGroupSelectListItem()
        {
            try
            {
                MenuGroupLogic menuGroupLogic = new MenuGroupLogic();
                List<MenuGroup> menuGroups = menuGroupLogic.GetAll();
                if (menuGroups == null || menuGroups.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> MenuGroupList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                MenuGroupList.Add(list);

                foreach (MenuGroup role in menuGroups)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = role.Id.ToString();
                    selectList.Text = role.Name;

                    MenuGroupList.Add(selectList);
                }

                return MenuGroupList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateMenuSelectListItem()
        {
            try
            {
                MenuLogic menuLogic = new MenuLogic();
                List<Abundance_Nk.Model.Model.Menu> menuList = menuLogic.GetAll();
                if (menuList == null || menuList.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> MenuList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                MenuList.Add(list);

                foreach (Abundance_Nk.Model.Model.Menu menu in menuList)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = menu.Id.ToString();
                    selectList.Text = menu.DisplayName + ", In " + menu.MenuGroup.Name;

                    MenuList.Add(selectList);
                }

                return MenuList.OrderBy(m => m.Text).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateResultTypeSelectListItem()
        {
            try
            {
                StudentResultTypeLogic studentResultTypeLogic = new StudentResultTypeLogic();
                List<StudentResultType> resultTypeList = studentResultTypeLogic.GetAll();
                if (resultTypeList == null || resultTypeList.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> ResultList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                ResultList.Add(list);

                foreach (StudentResultType resultType in resultTypeList)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = resultType.Id.ToString();
                    selectList.Text = resultType.Name;

                    ResultList.Add(selectList);
                }

                return ResultList.OrderBy(m => m.Text).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<SelectListItem> PopulateHostelTypes()
        {
            try
            {
                HostelTypeLogic hostelTypeLogic = new HostelTypeLogic();
                List<HostelType> hostelTypes = hostelTypeLogic.GetAll();

                if (hostelTypes == null || hostelTypes.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (HostelType hostelType in hostelTypes)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = hostelType.Hostel_Type_Id.ToString();
                    selectList.Text = hostelType.Hostel_Type_Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<Semester> GetAllSemesters()
        {
            try
            {
                var sessionSemesterLogic = new SemesterLogic();
                List<Semester> sessionSemesters = sessionSemesterLogic.GetAll();

                if (sessionSemesters != null && sessionSemesters.Count > 0)
                {
                    sessionSemesters.Insert(0, new Semester { Id = 0, Name = SelectSemester });
                }

                return sessionSemesters;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateDeliveryServiceSelectListItem()
        {
            try
            {
                DeliveryServiceLogic deliveryServiceLogic = new DeliveryServiceLogic();
                List<DeliveryService> deliveryServices = deliveryServiceLogic.GetModelsBy(d => d.Activated);

                if (deliveryServices == null || deliveryServices.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                List<SelectListItem> selectItemList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                selectItemList.Add(list);

                foreach (DeliveryService deliveryService in deliveryServices)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = deliveryService.Id.ToString();
                    selectList.Text = deliveryService.Name;

                    selectItemList.Add(selectList);
                }

                return selectItemList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<FeeType> GetAllFeeTypes()
        {
            try
            {
                var feeTypeLogic = new FeeTypeLogic();
                List<FeeType> feeTypes = feeTypeLogic.GetAll();

                if (feeTypes != null && feeTypes.Count > 0)
                {
                    feeTypes.Insert(0, new FeeType { Id = 0, Name = "-- Select Fee Type --" });
                }

                return new List<FeeType>(feeTypes.OrderBy(x => x.Name));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Student GetStudent(string MatricNumber)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                return studentLogic.GetBy(MatricNumber);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static StudentLevel GetStudentLevel(string MatricNumber)
        {
            try
            {
                StudentLogic studentLogic = new StudentLogic();
                StudentLevelLogic levelLogic = new StudentLevelLogic();
                var student = studentLogic.GetBy(MatricNumber);
                return levelLogic.GetBy(student.Id);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static StudentExtraYearSession GetExtraYearStudent(Student student)
        {
            try
            {
                StudentExtraYearLogic extraYearLogic = new StudentExtraYearLogic();
                StudentExtraYearSession extraYearSession = extraYearLogic.GetModelsBy(s => s.Person_Id == student.Id).LastOrDefault();

                return extraYearSession;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static List<SelectListItem> PopulateEContentTypeSelectListItem()
        {
            try
            {
                var contentTypeLogic = new EContentTypeLogic();
                List<EContentType> contentTypeList = contentTypeLogic.GetAll();

                if (contentTypeList == null || contentTypeList.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                var contentTypeSelectList = new List<SelectListItem>();

                var list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                contentTypeSelectList.Add(list);

                foreach (EContentType type in contentTypeList)
                {
                    var selectList = new SelectListItem();
                    selectList.Value = type.Id.ToString();
                    selectList.Text = type.Name;

                    contentTypeSelectList.Add(selectList);
                }

                return contentTypeSelectList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateEContentTypeSelectListItemByCourseAllocation(long courseAllocationId)
        {
            try
            {
                var contentTypeLogic = new EContentTypeLogic();
                List<EContentType> contentTypeList = contentTypeLogic.GetModelsBy(f=>f.Course_Allocation_Id==courseAllocationId && f.IsDelete==false);

                if (contentTypeList == null || contentTypeList.Count <= 0)
                {
                    return new List<SelectListItem>();
                }

                var contentTypeSelectList = new List<SelectListItem>();

                var list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                contentTypeSelectList.Add(list);

                foreach (EContentType type in contentTypeList)
                {
                    var selectList = new SelectListItem();
                    selectList.Value = type.Id.ToString();
                    selectList.Text = type.Name;

                    contentTypeSelectList.Add(selectList);
                }

                return contentTypeSelectList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool HasMoreThanFourRegistrations(Person person)
        {
            bool status = false;
            try
            {
                if (person != null && person.Id > 0)
                {
                    CourseRegistrationLogic courseRegistrationLogic = new CourseRegistrationLogic();
                    List<CourseRegistration> courseRegistrations = courseRegistrationLogic.GetModelsBy(c => c.Person_Id == person.Id);
                    if (courseRegistrations != null && courseRegistrations.Count > 4)
                    {
                        status = true;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return status;
        }

        public static string Export<T>(List<T> dataToExport,string folder ,string FileName = null)
        {
            string excelName = FileName ?? $"ResultSheetDownload-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xls";

            FileInfo file = new FileInfo(Path.Combine(folder, excelName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(folder, excelName));
            }

            using (var package = new ExcelPackage(file))
            {
                package.Workbook.Properties.Author = "VirtualSchool-Lloydant";
                package.Workbook.Properties.Created = DateTime.Now;
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells.LoadFromCollection(dataToExport,true);
                package.Save();
            }

            return file.Name;
        }

        public static DataTable Import(string FilePath)
        {
            FileInfo file = new FileInfo(FilePath);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
                DataTable table = new DataTable();
                foreach (var firstRowCell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
                {
                    table.Columns.Add(firstRowCell.Text);
                }
                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                    var newRow = table.NewRow();
                    foreach (var cell in row)
                    {
                        newRow[cell.Start.Column - 1] = cell.Text;
                    }
                    table.Rows.Add(newRow);
                }

                return table;
            }
        }

        public static List<T> Import<T>(string FilePath)
        {
            FileInfo file = new FileInfo(FilePath);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
                DataTable table = new DataTable();
                foreach (var firstRowCell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
                {
                    table.Columns.Add(firstRowCell.Text);
                }
                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                    var newRow = table.NewRow();
                    foreach (var cell in row)
                    {
                        newRow[cell.Start.Column - 1] = cell.Text;
                    }
                    table.Rows.Add(newRow);
                }

                return ConvertDataTable<T>(table);
            }
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        pro.SetValue(obj, Convert.ChangeType(dr[column.ColumnName], pro.PropertyType),null);
                    }
                    else
                        continue;
                }
            }
            return obj;
        }

        public static List<SelectListItem> GradeGuideSelectListItem(int Max)
        {
            try
            {
                var contentTypeLogic = new EContentTypeLogic();
                //int Max = 100;

                var gradeSelectList = new List<SelectListItem>();

                var list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                gradeSelectList.Add(list);



                for (int i = 0; i <= Max; i++)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Text = i.ToString();
                    selectList.Value = i.ToString();
                    gradeSelectList.Add(selectList);
                }

                return gradeSelectList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool hasDoneMoreThanFourSessions(Model.Model.Student student, Session session)
        {
            try
            {
                if (student != null && !string.IsNullOrEmpty(student.MatricNumber) && session != null && session.Id > 0)
                {

                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    var allStudentLevel=studentLevelLogic.GetModelsBy(f => f.Person_Id == student.Id);
                    List<int> yearsInschool = new List<int>();
                    if (allStudentLevel?.Count >= 4)
                    {
                        foreach(var item in allStudentLevel)
                        {
                            var sessionStartYear=Convert.ToInt32(item.Session.Name.Split('/')[0]);
                            yearsInschool.Add(sessionStartYear);

                        }
                       yearsInschool.Sort();
                        var maxYear=yearsInschool.LastOrDefault();
                        if (!string.IsNullOrEmpty(session.Name))
                        {
                            var activeSessionYear = Convert.ToInt32(session.Name.Split('/')[0]);
                            if (activeSessionYear > maxYear)
                                return true;
                        }

                    }

                    
                }
            }
            catch (Exception)
            {
                throw;
            }

            return false;
        }
        public static List<SelectListItem> PopulateApplicantApplicationApproval()
        {
            try
            {
                List<SelectListItem> studentStatusList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                studentStatusList.Add(list);
                SelectListItem listApprove = new SelectListItem();
                listApprove.Text = "Approve";
                listApprove.Value = "true";
                studentStatusList.Add(listApprove);
                SelectListItem lisReject = new SelectListItem();
                lisReject.Text = "Reject";
                lisReject.Value = "false";
                studentStatusList.Add(lisReject);
                

                return studentStatusList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static List<SelectListItem> PopulateOptionSelectListItem()
        {
            try
            {
                List<OptionItems> optionItemlist = new List<OptionItems>();
                optionItemlist.Add(new OptionItems
                {
                    Text = "Yes",
                    value = "true"
                });
                optionItemlist.Add(new OptionItems
                {
                    Text = "No",
                    value = "false"
                });

                List<SelectListItem> optionList = new List<SelectListItem>();

                SelectListItem list = new SelectListItem();
                list.Value = "";
                list.Text = Select;
                optionList.Add(list);

                foreach (var item in optionItemlist)
                {
                    SelectListItem selectList = new SelectListItem();
                    selectList.Value = item.value.ToString();
                    selectList.Text = item.Text;
                    optionList.Add(selectList);
                }


                return optionList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    public class OptionItems
    {
        public string value { get; set; }
        public string Text { get; set; }
    }
}