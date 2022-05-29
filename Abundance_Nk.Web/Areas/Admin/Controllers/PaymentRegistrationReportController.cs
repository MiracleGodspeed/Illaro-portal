using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class PaymentRegistrationReportController : BaseController
    {
        public ActionResult Payment()
        {
            try
            {
                List<SelectListItem> FeeTypeList = Utility.PopulateFeeTypeSelectListItem();
                ViewBag.FeeType = new SelectList(FeeTypeList, "Id", "Name");
            }
            catch (Exception ex)
            {                
                SetMessage("Error: " + ex.Message, Message.Category.Error);
            }

            return View();
        }
        public ActionResult PaymentReport(FeeType model)
        {
            ViewBag.FeeType = model.Id;
            return View();
        }
        public ActionResult BursaryPaymentReport()
        {
            return View();
        }
        public JsonResult GetPaymentSummary(string gatewayString, string dateFromString, string dateToString)
        {
            PaymentJsonResult singleResult = new PaymentJsonResult();
            List<PaymentJsonResult> result = new List<PaymentJsonResult>();
            try
            {
                if (!string.IsNullOrEmpty(gatewayString) && !string.IsNullOrEmpty(dateFromString) && !string.IsNullOrEmpty(dateToString))
                {
                    //DateTime dateFrom = new DateTime(Convert.ToInt32(dateFromString.Split('-')[0]), Convert.ToInt32(dateFromString.Split('-')[1]), Convert.ToInt32(dateFromString.Split('-')[2]));
                    //DateTime dateTo = new DateTime(Convert.ToInt32(dateToString.Split('-')[0]), Convert.ToInt32(dateToString.Split('-')[1]), Convert.ToInt32(dateToString.Split('-')[2]));
                    
                    DateTime dateFrom = new DateTime();
                    DateTime dateTo = new DateTime();

                    if (!DateTime.TryParse(dateFromString, out dateFrom))
                        dateFrom = DateTime.Now;
                    if (!DateTime.TryParse(dateToString, out dateTo))
                        dateTo = DateTime.Now;

                    dateTo = dateTo.AddHours(23.999);

                    PaymentLogic paymentLogic = new PaymentLogic();
                    List<PaymentSummary> paymentSummary = paymentLogic.GetPaymentSummary(dateFrom, dateTo, gatewayString);
                    if (paymentSummary != null && paymentSummary.Count > 0)
                    {
                        if (gatewayString == "Etranzact")
                            paymentSummary = paymentSummary.Where(p => p.RRR == null && p.PaymentEtranzactId != null).ToList();
                        else
                            paymentSummary = paymentSummary.Where(p => p.RRR != null && p.Status.Contains("01")).ToList();

                        //decimal overallAmount = Convert.ToDecimal(paymentSummary.Sum(p => p.TransactionAmount));
                        decimal overallAmount = 0M;

                        List<int> distinctFeeTypeId = paymentSummary.Select(p => p.FeeTypeId).Distinct().ToList();
                        for (int i = 0; i < distinctFeeTypeId.Count; i++)
                        {
                            int currentFeeTypeId = distinctFeeTypeId[i];

                            //List<PaymentSummary> feeTypeSummary = paymentSummary.Where(p => p.FeeTypeId == currentFeeTypeId).ToList();

                            List<PaymentSummary> feeTypeSummary = paymentLogic.GetPaymentSummaryByFeeType(dateFrom, dateTo, currentFeeTypeId, gatewayString);

                            //decimal feeTypeTotalAmount = Convert.ToDecimal(feeTypeSummary.Sum(p => p.TransactionAmount));
                            decimal feeTypeTotalAmount = paymentLogic.GetTotalPaymentByFeeType(dateFrom, dateTo, currentFeeTypeId, gatewayString);

                            PaymentJsonResult paymentJsonResult = new PaymentJsonResult();
                            paymentJsonResult.FeeTypeId = currentFeeTypeId;
                            paymentJsonResult.FeeTypeName = feeTypeSummary.FirstOrDefault().FeeTypeName;
                            paymentJsonResult.TotalAmount = String.Format("{0:N}", feeTypeTotalAmount);
                            paymentJsonResult.TotalCount = feeTypeSummary.Count();
                            paymentJsonResult.IsError = false;
                            //paymentJsonResult.OverallAmount = String.Format("{0:N}", feeTypeSummary.FirstOrDefault().OverallAmount);
                            overallAmount += feeTypeTotalAmount;

                            result.Add(paymentJsonResult);
                        }

                        for (int i = 0; i < result.Count; i++)
                        {
                            result[i].OverallAmount = String.Format("{0:N}", overallAmount);
                        }
                    }

                    if (paymentSummary == null || paymentSummary.Count <= 0)
                    {
                        singleResult.IsError = true;
                        singleResult.Message = "No records found! ";

                        return Json(singleResult, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    singleResult.IsError = true;
                    singleResult.Message = "Invalid parametr! ";
                }
            }
            catch (Exception ex)
            {
                singleResult.IsError = true;
                singleResult.Message = "Error! " + ex.Message;
            }

            return Json(singleResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPaymentBreakdown(string gatewayString, string dateFromString, string dateToString, int feeTypeId)
        {
            PaymentJsonResult singleResult = new PaymentJsonResult();
            List<PaymentJsonResult> result = new List<PaymentJsonResult>();
            try
            {
                if (!string.IsNullOrEmpty(gatewayString) && !string.IsNullOrEmpty(dateFromString) && !string.IsNullOrEmpty(dateToString) && feeTypeId > 0)
                {
                    //DateTime dateFrom = new DateTime(Convert.ToInt32(dateFromString.Split('-')[0]), Convert.ToInt32(dateFromString.Split('-')[1]), Convert.ToInt32(dateFromString.Split('-')[2]));
                    //DateTime dateTo = new DateTime(Convert.ToInt32(dateToString.Split('-')[0]), Convert.ToInt32(dateToString.Split('-')[1]), Convert.ToInt32(dateToString.Split('-')[2]));
                    
                    DateTime dateFrom = new DateTime();
                    DateTime dateTo = new DateTime();

                    if (!DateTime.TryParse(dateFromString, out dateFrom))
                        dateFrom = DateTime.Now;
                    if (!DateTime.TryParse(dateToString, out dateTo))
                        dateTo = DateTime.Now;

                    dateTo = dateTo.AddHours(23.999);

                    PaymentLogic paymentLogic = new PaymentLogic();
                    //List<PaymentSummary> paymentSummary = paymentLogic.GetPaymentSummary(dateFrom, dateTo, gatewayString);
                    List<PaymentSummary> paymentSummary = paymentLogic.GetPaymentSummaryByFeeType(dateFrom, dateTo, feeTypeId, gatewayString);

                    if (paymentSummary != null && paymentSummary.Count > 0)
                    {
                        //if (gatewayString == "Etranzact")
                        //    paymentSummary = paymentSummary.Where(p => p.RRR == null && p.PaymentEtranzactId != null && p.FeeTypeId == feeTypeId).ToList();
                        //else
                        //    paymentSummary = paymentSummary.Where(p => p.RRR != null && p.Status.Contains("01") && p.FeeTypeId == feeTypeId).ToList();

                        PaymentJsonResult paymentJsonResult;
                        for (int i = 0; i < paymentSummary.Count; i++)
                        {
                            paymentJsonResult = new PaymentJsonResult();
                            paymentJsonResult.TransactionDate = Convert.ToDateTime(paymentSummary[i].TransactionDate).ToShortDateString();
                            paymentJsonResult.MatricNumber = paymentSummary[i].MatricNumber;
                            paymentJsonResult.Name = paymentSummary[i].Name;
                            paymentJsonResult.Level = paymentSummary[i].LevelName;
                            paymentJsonResult.Department = paymentSummary[i].DepartmentName;
                            paymentJsonResult.Faculty = paymentSummary[i].FacultyName;
                            paymentJsonResult.Programme = paymentSummary[i].ProgrammeName;
                            paymentJsonResult.Session = paymentSummary[i].SessionName;
                            paymentJsonResult.InvoiceNumber = paymentSummary[i].InvoiceNumber;
                            paymentJsonResult.ConfirmationNumber = paymentSummary[i].ConfirmationNumber;
                            paymentJsonResult.FeeTypeName = paymentSummary[i].FeeTypeName;
                            paymentJsonResult.Amount = String.Format("{0:N}", paymentSummary[i].TransactionAmount);

                            result.Add(paymentJsonResult);
                        }
                    }

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    singleResult.IsError = true;
                    singleResult.Message = "Invalid parametr! ";
                }
            }
            catch (Exception ex)
            {
                singleResult.IsError = true;
                singleResult.Message = "Error! " + ex.Message;
            }

            return Json(singleResult, JsonRequestBehavior.AllowGet);
        }
    }
}