using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using Abundance_Nk.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class StudentClearanceController : BaseController
    {
        private ClearanceLogLogic clearanceLogLogic;
        private ClearanceStatusLogic clearanceStatusLogic;
        private ClearanceUnitLogic clearanceUnitLogic;
        private StudentLogic studentLogic;
        private StudentClearanceViewModel studentClearanceViewModel;
        private PersonLogic personLogic;
        private StudentLevelLogic studentLevelLogic;
        private UserLogic userLogic;
        private ClearanceDisputesLogic clearanceDisputesLogic;
        private string appRoot = ConfigurationManager.AppSettings["AppRoot"];

        public StudentClearanceController()
        {
            clearanceLogLogic = new ClearanceLogLogic();
            clearanceStatusLogic = new ClearanceStatusLogic();
            clearanceUnitLogic = new ClearanceUnitLogic();
            studentLogic = new StudentLogic();
            studentClearanceViewModel = new StudentClearanceViewModel();
            personLogic = new PersonLogic();
            studentLevelLogic = new StudentLevelLogic();
            userLogic = new UserLogic();
            clearanceDisputesLogic = new ClearanceDisputesLogic();

        }
        // GET: Admin/StudentClearance
        public ActionResult BursaryStudentClearance()
        {
            ClearanceDisputesLogic clearanceDisputesLogic = new ClearanceDisputesLogic();
            try
            {
                studentClearanceViewModel.ClearanceLogs=clearanceLogLogic.GetModelsBy(g => g.Clearance_Unit_Id == 1);
                if (studentClearanceViewModel.ClearanceLogs.Count > 0)
                {
                    for(int i=0; i< studentClearanceViewModel.ClearanceLogs.Count; i++)
                    {
                        var logId = studentClearanceViewModel.ClearanceLogs[i].Id;
                        var disputeExist=clearanceDisputesLogic.GetModelsBy(f => f.Clearance_Log_Id == logId);
                        if (disputeExist.Count > 0)
                        {
                            studentClearanceViewModel.ClearanceLogs[i].IsDisputed = true;
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        public ActionResult LibraryStudentClearance()
        {

            ClearanceDisputesLogic clearanceDisputesLogic = new ClearanceDisputesLogic();
            try
            {
                studentClearanceViewModel.ClearanceLogs = clearanceLogLogic.GetModelsBy(g => g.Closed == false && g.Clearance_Unit_Id == 2);
                if (studentClearanceViewModel.ClearanceLogs.Count > 0)
                {
                    for (int i = 0; i < studentClearanceViewModel.ClearanceLogs.Count; i++)
                    {
                        var logId = studentClearanceViewModel.ClearanceLogs[i].Id;
                        var disputeExist = clearanceDisputesLogic.GetModelsBy(f => f.Clearance_Log_Id == logId);
                        if (disputeExist.Count > 0)
                        {
                            studentClearanceViewModel.ClearanceLogs[i].IsDisputed = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        public ActionResult StudentAffairClearance()
        {

            ClearanceDisputesLogic clearanceDisputesLogic = new ClearanceDisputesLogic();
            try
            {
                studentClearanceViewModel.ClearanceLogs = clearanceLogLogic.GetModelsBy(g => g.Closed == false && g.Clearance_Unit_Id == 3);
                if (studentClearanceViewModel.ClearanceLogs.Count > 0)
                {
                    for (int i = 0; i < studentClearanceViewModel.ClearanceLogs.Count; i++)
                    {
                        var logId = studentClearanceViewModel.ClearanceLogs[i].Id;
                        var disputeExist = clearanceDisputesLogic.GetModelsBy(f => f.Clearance_Log_Id == logId);
                        if (disputeExist.Count > 0)
                        {
                            studentClearanceViewModel.ClearanceLogs[i].IsDisputed = true;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        public ActionResult HealthStudentClearance()
        {
            ClearanceDisputesLogic clearanceDisputesLogic = new ClearanceDisputesLogic();
            try
            {
                studentClearanceViewModel.ClearanceLogs = clearanceLogLogic.GetModelsBy(g => g.Closed == false && g.Clearance_Unit_Id == 4);
                if (studentClearanceViewModel.ClearanceLogs.Count > 0)
                {
                    for (int i = 0; i < studentClearanceViewModel.ClearanceLogs.Count; i++)
                    {
                        var logId = studentClearanceViewModel.ClearanceLogs[i].Id;
                        var disputeExist = clearanceDisputesLogic.GetModelsBy(f => f.Clearance_Log_Id == logId);
                        if (disputeExist.Count > 0)
                        {
                            studentClearanceViewModel.ClearanceLogs[i].IsDisputed = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        public ActionResult DepartmentClearance()
        {
            ClearanceDisputesLogic clearanceDisputesLogic = new ClearanceDisputesLogic();
            try
            {
                studentClearanceViewModel.ClearanceLogs = clearanceLogLogic.GetModelsBy(g => g.Closed == false && g.Clearance_Unit_Id == 5);
                if (studentClearanceViewModel.ClearanceLogs.Count > 0)
                {
                    for (int i = 0; i < studentClearanceViewModel.ClearanceLogs.Count; i++)
                    {
                        var logId = studentClearanceViewModel.ClearanceLogs[i].Id;
                        var disputeExist = clearanceDisputesLogic.GetModelsBy(f => f.Clearance_Log_Id == logId);
                        if (disputeExist.Count > 0)
                        {
                            studentClearanceViewModel.ClearanceLogs[i].IsDisputed = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        public ActionResult ClearStudent(long Id)
        {
            try
            {
                if (Id > 0)
                {
                    var userName = User.Identity.Name;
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                    var user = userLogic.GetModelsBy(u => u.User_Name == userName).FirstOrDefault();
                    var log=clearanceLogLogic.GetModelBy(f => f.Id == Id);
                    if (log != null)
                    {
                        log.DateCleared = DateTime.Now;
                        log.ClearanceStatus = new Model.Entity.Model.ClearanceStatus { Id = 2 };
                        log.Closed = true;
                        log.User = user;
                        log.Client = client;
                        clearanceLogLogic.Modify(log);
                        SetMessage("Operation was Successful", Message.Category.Information);
                        if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.Bursary)
                        {
                            return RedirectToAction("BursaryStudentClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.Library)
                        {
                            return RedirectToAction("LibraryStudentClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.StudentAffair)
                        {
                            return RedirectToAction("StudentAffairClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.HealthCentre)
                        {
                            return RedirectToAction("HealthStudentClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.Department)
                        {
                            return RedirectToAction("DepartmentClearance");
                        }
                    }
                  
                }
            }
            catch(Exception ex)
            {
                throw ex;
                
            }
            return View();
        }
        public ActionResult IncompleteClearance(long Id)
        {
            try
            {
                if (Id > 0)
                {
                    var log = clearanceLogLogic.GetModelBy(f => f.Id == Id);
                    if (log != null)
                    {
                        studentClearanceViewModel.ClearanceLog = log;
                        studentClearanceViewModel.ClearanceDisputesList= clearanceDisputesLogic.GetModelsBy(h => h.Clearance_Log_Id == log.Id).OrderBy(h=>h.DateSent).ToList();
                        studentClearanceViewModel.ClearanceLog.Remarks = null;
                    }
                    

                }
                //TempData["unit"] = unit;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        [HttpPost]
        public ActionResult ExtrainformationClearance(StudentClearanceViewModel studentClearanceViewModel)
        {
            try
            {
                //var unit = TempData["unit"] as string;
                //var fileurl = TempData["imageUrl"] as string;
                if (studentClearanceViewModel.ClearanceLog.Id > 0)
                {
                    var userName = User.Identity.Name;
                    string client = Request.LogonUserIdentity.Name + " (" + HttpContext.Request.UserHostAddress + ")";
                    var user = userLogic.GetModelsBy(u => u.User_Name == userName).FirstOrDefault();
                    var log = clearanceLogLogic.GetModelBy(f => f.Id == studentClearanceViewModel.ClearanceLog.Id);
                    if (log != null)
                    {
                        using (TransactionScope transaction = new TransactionScope())
                        {
                            log.DateCleared = DateTime.Now;
                            log.ClearanceStatus = new Model.Entity.Model.ClearanceStatus { Id = 3 };
                            log.Closed = false;
                            log.User = user;
                            log.Client = client;
                            log.Remarks = studentClearanceViewModel.ClearanceLog.Remarks;

                            var modified=clearanceLogLogic.Modify(log);
                            if (modified)
                            {
                                ClearanceDisputes clearanceDisputes = new ClearanceDisputes();
                                //clearanceDisputes.Attachment = fileurl;
                                clearanceDisputes.ClearanceLog = log;
                                clearanceDisputes.DateSent = DateTime.Now;
                                clearanceDisputes.IsStudent = false;
                                clearanceDisputes.Remark = studentClearanceViewModel.ClearanceLog.Remarks;
                                clearanceDisputesLogic.Create(clearanceDisputes);
                            }
                            transaction.Complete();
                        }
                        SetMessage("Operation was Successful", Message.Category.Information);
                        if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.Bursary)
                        {
                            return RedirectToAction("BursaryStudentClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.Library)
                        {
                            return RedirectToAction("LibraryStudentClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.StudentAffair)
                        {
                            return RedirectToAction("StudentAffairClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.HealthCentre)
                        {
                            return RedirectToAction("HealthStudentClearance");
                        }
                        else if (log.ClearanceUnit.Id == (int)ClearanceUnitEnum.Department)
                        {
                            return RedirectToAction("DepartmentClearance");
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        public ActionResult ViewDisputedClearances(long logId)
        {
            try
            {
                if (logId > 0)
                {
                    studentClearanceViewModel.ClearanceDisputesList= clearanceDisputesLogic.GetModelsBy(f => f.Clearance_Log_Id == logId).OrderBy(i => i.DateSent).ToList();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View(studentClearanceViewModel);
        }
        public ActionResult DownloadAttachment(long disputeId)
        {
            try
            {
                var dispute = clearanceDisputesLogic.GetModelBy(o => o.Id == disputeId);
                return File(Server.MapPath(dispute.Attachment), "application/pdf");
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured. " + ex.Message, Message.Category.Error);
                return RedirectToAction("Home", "Account", new { Area = "Security" });
            }
        }
        public virtual ActionResult UploadFile(FormCollection form)
        {
            HttpPostedFileBase file = Request.Files["MyFile"];

            bool isUploaded = false;
            string logId = form["ClearanceLog.Id"].ToString();
            string documentUrl = form["ClearanceDisputes.Attachment"].ToString();
            string message = "File upload failed";

            string path = null;
            string imageUrl = null;
            string imageUrlDisplay = null;

            try
            {
                if (file != null && file.ContentLength != 0)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string fileExtension = fileInfo.Extension;
                    string newFile = logId + "__";
                    string newFileName = newFile + DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "") + fileExtension;

                    decimal sizeAllowed = 50; //50kb
                    string invalidFileMessage = InvalidFile(file.ContentLength, fileExtension, sizeAllowed);
                    if (!string.IsNullOrEmpty(invalidFileMessage))
                    {
                        isUploaded = false;
                        TempData["imageUrl"] = null;
                        return Json(new { isUploaded = isUploaded, message = invalidFileMessage, imageUrl = documentUrl }, "text/html", JsonRequestBehavior.AllowGet);
                    }

                    string pathForSaving = Server.MapPath("~/Content/Junk");
                    if (this.CreateFolderIfNeeded(pathForSaving))
                    {
                        DeleteFileIfExist(pathForSaving, logId);

                        file.SaveAs(Path.Combine(pathForSaving, newFileName));

                        isUploaded = true;
                        message = "File uploaded successfully!";

                        path = Path.Combine(pathForSaving, newFileName);
                        if (path != null)
                        {
                            imageUrl = "/Content/Junk/" + newFileName;
                            imageUrlDisplay = appRoot + imageUrl + "?t=" + DateTime.Now;

                            //imageUrlDisplay = "/ilaropoly" + imageUrl + "?t=" + DateTime.Now;
                            TempData["imageUrl"] = imageUrl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = string.Format("File upload failed: {0}", ex.Message);
            }
            TempData.Keep();
            return Json(new { isUploaded = isUploaded, message = message, imageUrl = imageUrl }, "text/html", JsonRequestBehavior.AllowGet);
        }
        private string InvalidFile(decimal uploadedFileSize, string fileExtension, decimal sizeAllowed)
        {
            try
            {
                string message = null;
                decimal oneKiloByte = 1024;
                decimal maximumFileSize = sizeAllowed * oneKiloByte;

                decimal actualFileSizeToUpload = Math.Round(uploadedFileSize / oneKiloByte, 1);
                if (InvalidFileType(fileExtension))
                {
                    //message = "File type '" + fileExtension + "' is invalid! File type must be any of the following: .jpg, .jpeg, .png, .pdf or .gif ";
                    message = "File type '" + fileExtension + "' is invalid! File type must be any of the following: .pdf ";
                }
                else if (actualFileSizeToUpload > (maximumFileSize / oneKiloByte))
                {
                    message = "Your file size of " + actualFileSizeToUpload.ToString("0.#") + " Kb is too large, maximum allowed size is " + (maximumFileSize / oneKiloByte) + " Kb";
                }

                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string InvalidFilePdf(decimal uploadedFileSize, string fileExtension, decimal sizeAllowed)
        {
            try
            {
                string message = null;
                decimal oneKiloByte = 1024;
                decimal maximumFileSize = sizeAllowed * oneKiloByte;

                decimal actualFileSizeToUpload = Math.Round(uploadedFileSize / oneKiloByte, 1);
                if (InvalidFileTypePdf(fileExtension))
                {
                    message = "File type '" + fileExtension + "' is invalid! File type must be .pdf ";
                }
                else if (actualFileSizeToUpload > (maximumFileSize / oneKiloByte))
                {
                    message = "Your file size of " + actualFileSizeToUpload.ToString("0.#") + " Kb is too large, maximum allowed size is " + (maximumFileSize / oneKiloByte) + " Kb";
                }

                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool InvalidFileType(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                //case ".jpg":
                //    return false;
                //case ".png":
                //    return false;
                //case ".gif":
                //    return false;
                //case ".jpeg":
                //    return false;
                case ".pdf":
                    return false;
                default:
                    return true;
            }
        }
        private bool InvalidFileTypePdf(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".pdf":
                    return false;
                default:
                    return true;
            }
        }

        private void DeleteFileIfExist(string folderPath, string fileName)
        {
            try
            {
                string wildCard = fileName + "*.*";
                IEnumerable<string> files = Directory.EnumerateFiles(folderPath, wildCard, SearchOption.TopDirectoryOnly);

                if (files != null && files.Count() > 0)
                {
                    foreach (string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CreateFolderIfNeeded(string path)
        {
            try
            {
                bool result = true;
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception)
                    {
                        /*TODO: You must process this exception.*/
                        result = false;
                    }
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}