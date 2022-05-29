using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Applicant.ViewModels;
using Abundance_Nk.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;


namespace Abundance_Nk.Web.Areas.Applicant.Controllers
{
    [AllowAnonymous]
    public class ClearanceController : BaseController
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string TEXT = "Text";

        private ClearanceLogLogic clearanceLogLogic;
        private ClearanceStatusLogic clearanceStatusLogic;
        private ClearanceUnitLogic clearanceUnitLogic;
        private StudentLogic studentLogic;
        private ClearanceViewModel clearanceViewModel;
        private PersonLogic personLogic;
        private StudentLevelLogic studentLevelLogic;
        private ClearanceDisputesLogic clearanceDisputesLogic;
        private string appRoot = ConfigurationManager.AppSettings["AppRoot"];

        public ClearanceController()
        {
            clearanceLogLogic = new ClearanceLogLogic();
            clearanceStatusLogic = new ClearanceStatusLogic();
            clearanceUnitLogic = new ClearanceUnitLogic();
            studentLogic = new StudentLogic();
            clearanceViewModel = new ClearanceViewModel();
            personLogic = new PersonLogic();
            studentLevelLogic = new StudentLevelLogic();
            clearanceDisputesLogic = new ClearanceDisputesLogic();


        }
        // GET: Applicant/Clearance
        public ActionResult Index()
        {

            return View(clearanceViewModel);
        }
        [HttpPost]
        public ActionResult Index(ClearanceViewModel clearanceViewModel)
        {
            try
            {
                if (clearanceViewModel.Student.MatricNumber == null)
                {
                    SetMessage("Please, Enter Matric Number to Continue", Message.Category.Error);
                    return View(clearanceViewModel);
                }
                clearanceViewModel.ViewPanel = true;
                var student = studentLogic.GetModelBy(f => f.Matric_Number == clearanceViewModel.Student.MatricNumber);
                if (student != null)
                {
                    clearanceViewModel.ClearanceLog = clearanceLogLogic.GetModelsBy(f => f.Student_Id == student.Id).FirstOrDefault();
                    clearanceViewModel.Student = new Model.Model.Student();
                    clearanceViewModel.Student = student;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(clearanceViewModel);
        }
        public ActionResult ClearanceRequest(ClearanceViewModel clearanceViewModel)
        {
            try
            {
                if (clearanceViewModel.Student == null)
                {
                    SetMessage("Please, Enter Matric Number to Continue", Message.Category.Error);
                    return RedirectToAction("Index", clearanceViewModel);
                }
                var student = studentLogic.GetModelsBy(d => d.Matric_Number == clearanceViewModel.Student.MatricNumber).FirstOrDefault();
                var allActiveUnitForClearance = clearanceUnitLogic.GetModelsBy(p => p.Active);
                if (student != null)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {


                        if (allActiveUnitForClearance.Count > 0)
                        {
                            for (int i = 0; i < allActiveUnitForClearance.Count; i++)
                            {
                                var unitId = allActiveUnitForClearance[i].Id;
                                var ClearanceLog = clearanceLogLogic.GetModelsBy(f => f.Student_Id == student.Id && f.Clearance_Unit_Id == unitId);
                                if (ClearanceLog.Count == 0)
                                {
                                    ClearanceLog log = new ClearanceLog();
                                    log.Student = student;
                                    log.ClearanceStatus = new ClearanceStatus { Id = 1 };
                                    log.ClearanceUnit = new ClearanceUnit { Id = unitId };
                                    log.Closed = false;
                                    var createdLog = clearanceLogLogic.Create(log);
                                }

                                //var clearancelog = clearanceLogLogic.GetModelsBy(f => f.Student_Id == student.Id);
                            }
                        }
                        transaction.Complete();
                    }
                    clearanceViewModel.ViewPanel = false;
                    SetMessage("You have successfully requested for clearance", Message.Category.Information);
                    return RedirectToAction("Index", clearanceViewModel);
                }
                else
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {

                        Model.Model.Student newStudent = new Model.Model.Student();
                        Person person = new Person();
                        StudentLevel studentLevel = new StudentLevel();

                        newStudent = clearanceViewModel.Student;

                        Role role = new Role() { Id = 5 };
                        Nationality nationality = new Nationality() { Id = 1 };

                        person.LastName = newStudent.LastName;
                        person.FirstName = newStudent.FirstName;
                        person.OtherName = newStudent.OtherName;
                        person.State = new State() { Id = "OG" };
                        person.Role = role;
                        person.Nationality = nationality;
                        person.DateEntered = DateTime.Now;
                        person.Type = new PersonType() { Id = 3 };
                        person.Email = newStudent.Email;
                        person.MobilePhone = newStudent.MobilePhone;
                        person = personLogic.Create(person);
                        if (person != null && person.Id > 0)
                        {
                            string Type = newStudent.MatricNumber.Substring(0, 1);
                            if (Type == "H")
                            {
                                student.Type = new StudentType() { Id = 2 };
                            }
                            else
                            {
                                newStudent.Type = new StudentType() { Id = 1 };
                            }
                            newStudent.Id = person.Id;
                            newStudent.Category = new StudentCategory() { Id = 2 };
                            newStudent.Status = new StudentStatus() { Id = 1 };
                            newStudent = studentLogic.Create(newStudent);
                        }
                        if (newStudent != null)
                        {
                            if (allActiveUnitForClearance.Count > 0)
                            {
                                for (int i = 0; i < allActiveUnitForClearance.Count; i++)
                                {
                                    var unitId = allActiveUnitForClearance[i].Id;
                                    var ClearanceLog = clearanceLogLogic.GetModelsBy(f => f.Student_Id == newStudent.Id && f.Clearance_Unit_Id == unitId);
                                    if (ClearanceLog.Count == 0)
                                    {
                                        ClearanceLog log = new ClearanceLog();
                                        log.Student = newStudent;
                                        log.ClearanceStatus = new ClearanceStatus { Id = 1 };
                                        log.ClearanceUnit = new ClearanceUnit { Id = unitId };
                                        log.Closed = false;
                                        var createdLog = clearanceLogLogic.Create(log);
                                    }
                                }
                            }



                            transaction.Complete();



                        }
                        clearanceViewModel.ViewPanel = false;
                        SetMessage("You have successfully requested for clearance", Message.Category.Information);
                        return RedirectToAction("Index", clearanceViewModel);
                    }

                }


                    }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(clearanceViewModel);
        }
        public ActionResult ClearanceRequestProgress(long sid)
        {
            try
            {
                bool allcleared = true;
                List<ClearanceUnit> allActiveClearanceUnit = new List<ClearanceUnit>();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                if (sid > 0)
                {
                    var studentLevel=studentLevelLogic.GetModelsBy(d => d.Person_Id == sid).LastOrDefault();
                    var clearanceLogs = clearanceLogLogic.GetModelsBy(f => f.Student_Id == sid);
                    if (clearanceLogs.Count > 0)
                    {
                        allActiveClearanceUnit=clearanceLogs.Select(f => f.ClearanceUnit).ToList();
                    }
                    //var allActiveClearanceUnit = clearanceUnitLogic.GetModelsBy(f => f.Active);
                    for (int i = 0; i < allActiveClearanceUnit.Count; i++)
                    {
                        var id = allActiveClearanceUnit[i].Id;
                        var loggedUnit = clearanceLogs.Where(f => f.ClearanceUnit.Id == id).FirstOrDefault();
                        if (loggedUnit != null)
                        {
                            allActiveClearanceUnit[i].status = loggedUnit.ClearanceStatus.Name;
                            allActiveClearanceUnit[i].LogId = loggedUnit.Id;

                        }
                        else
                        {
                            allActiveClearanceUnit[i].status = "Pending";

                        }
                    }
                    clearanceViewModel.ClearanceUnits = new List<ClearanceUnit>();
                    clearanceViewModel.ClearanceUnits.AddRange(allActiveClearanceUnit);
                    for (int i = 0; i < clearanceViewModel.ClearanceUnits.Count; i++)
                    {
                        if (clearanceViewModel.ClearanceUnits[i].status != "Cleared")
                        {
                            allcleared = false;
                        }
                    }
                    if (allcleared == true)
                    {
                        clearanceViewModel.ViewPrintButton = true;
                        
                    }
                    clearanceViewModel.Student = clearanceLogs.FirstOrDefault().Student;
                    clearanceViewModel.StudentLevel = studentLevel;

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(clearanceViewModel);
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
        public ActionResult DisputeClearance(long logId)
        {
            try
            {
                clearanceViewModel.ClearanceDisputesList = clearanceDisputesLogic.GetModelsBy(f => f.IsStudent == false && f.Clearance_Log_Id == logId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(clearanceViewModel);
        }
        public ActionResult ReplyDisputeClearance(ClearanceViewModel clearanceViewModel)
        {
            try
            {
                var fileurl = TempData["imageUrl"] as string;
                var id = clearanceViewModel.ClearanceLog.Id;
                var log = new ClearanceLog();

                log = clearanceLogLogic.GetModelBy(f => f.Id == id);
                if (log != null)
                {
                    ClearanceDisputes clearanceDisputes = new ClearanceDisputes();
                    clearanceDisputes.Attachment = fileurl;
                    clearanceDisputes.ClearanceLog = log;
                    clearanceDisputes.DateSent = DateTime.Now;
                    clearanceDisputes.IsStudent = true;
                    clearanceDisputes.Remark = clearanceViewModel.ClearanceLog.Remarks;
                    clearanceDisputesLogic.Create(clearanceDisputes);
                    SetMessage("Your Response was saved successfully", Message.Category.Information);
                }
                return RedirectToAction("ClearanceRequestProgress", new { sid = log.Student.Id });


            }
            catch (Exception ex)
            {
                throw ex;
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
                    message = "File type '" + fileExtension + "' is invalid! File type must be any of the following: .pdf";
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
                //case ".pdf":
                //    return false;
                //default:
                //    return true;

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
        [HttpPost]
        public ActionResult EditCriteria(ClearanceViewModel criteriaModel)
        {
            try
            {
                //Add subjects
                AdmissionCriteriaForOLevelSubjectLogic criteriaSubjectLogic = new AdmissionCriteriaForOLevelSubjectLogic();
                AdmissionCriteriaForOLevelSubjectAlternativeLogic criteriaSubjectAlternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();
                criteriaSubjectLogic.Modify(criteriaModel.admissionCriteriaForOLevelSubject);

                SetMessage("Criteria was updated successfully", Message.Category.Information);
                return RedirectToAction("AdmissionCriteria");
            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("AdmissionCriteria");
        }
        public ActionResult EditCriteria(long Id)
        {
            clearanceViewModel = new ClearanceViewModel();
            try
            {
                AdmissionCriteriaForOLevelSubjectLogic OlevelLogic = new AdmissionCriteriaForOLevelSubjectLogic();
                List<AdmissionCriteriaForOLevelSubjectAlternative> olevelAltList = new List<AdmissionCriteriaForOLevelSubjectAlternative>();
                AdmissionCriteriaForOLevelSubjectAlternative olevelAlt = new AdmissionCriteriaForOLevelSubjectAlternative();
                AdmissionCriteriaForOLevelSubjectAlternativeLogic olevelAltLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();
                AdmissionCriteriaForOLevelTypeLogic olevelTypeLogic = new AdmissionCriteriaForOLevelTypeLogic();


                for (int i = 0; i < 25; i++)
                {
                    ViewData["FirstSittingOLevelSubjectId" + i] = clearanceViewModel.OLevelSubjectSelectList;
                    ViewData["SecondSittingOLevelSubjectId" + i] = clearanceViewModel.OLevelSubjectSelectList;
                    ViewData["OtherOLevelSubjectId" + i] = clearanceViewModel.OLevelSubjectSelectList;
                    ViewData["FirstSittingOLevelGradeId" + i] = clearanceViewModel.OLevelGradeSelectList;

                }

                clearanceViewModel.admissionCriteriaForOLevelSubject = OlevelLogic.GetModelsBy(a => a.Admission_Criteria_Id == Id);
                foreach (AdmissionCriteriaForOLevelSubject subject in clearanceViewModel.admissionCriteriaForOLevelSubject)
                {
                    subject.Alternatives = olevelAltLogic.GetModelsBy(o => o.Admission_Criteria_For_O_Level_Subject_Id == subject.Id);

                    if (subject.Alternatives.Count > 1)
                    {
                        List<AdmissionCriteriaForOLevelSubjectAlternative> alternativeList = new List<AdmissionCriteriaForOLevelSubjectAlternative>();
                        alternativeList.Add(subject.Alternatives[1]);
                        subject.OtherAlternatives = alternativeList;
                    }
                }

                int count = clearanceViewModel.admissionCriteriaForOLevelSubject.Count;

                for (int i = count; i < count + 6; i++)
                {
                    clearanceViewModel.admissionCriteriaForOLevelSubject.Add(new AdmissionCriteriaForOLevelSubject()
                    {
                        Alternatives = new List<AdmissionCriteriaForOLevelSubjectAlternative>(),
                        IsCompulsory = false,
                        MainCriteria = clearanceViewModel.admissionCriteriaForOLevelSubject[0].MainCriteria,
                        MinimumGrade = clearanceViewModel.admissionCriteriaForOLevelSubject[0].MinimumGrade,
                        Subject = new OLevelSubject(),
                        OtherAlternatives = new List<AdmissionCriteriaForOLevelSubjectAlternative>()
                    });
                }

                clearanceViewModel.admissionCriteriaForOLevelType = olevelTypeLogic.GetModelsBy(n => n.Admission_Criteria_Id == Id);
                SetSelectedSittingSubjectAndGrade(clearanceViewModel);


                TempData["ClearanceViewModel"] = clearanceViewModel;
                ViewBag.OLevelSubjects = clearanceViewModel.OLevelSubjectSelectList;
                return View(clearanceViewModel);

            }
            catch (Exception ex)
            {

                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }

            ViewBag.OLevelSubjects = clearanceViewModel.OLevelSubjectSelectList;
            return View();
        }
        private void SetSelectedSittingSubjectAndGrade(ClearanceViewModel existingViewModel)
        {
            try
            {
                if (existingViewModel != null && existingViewModel.admissionCriteriaForOLevelSubject != null && existingViewModel.admissionCriteriaForOLevelSubject.Count > 0)
                {
                    int i = 0;
                    foreach (AdmissionCriteriaForOLevelSubject subject in existingViewModel.admissionCriteriaForOLevelSubject)
                    {
                        if (subject.Subject.Name != null)
                        {
                            ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, subject.Subject.Id);
                            if (subject.Alternatives.Count > 0)
                            {
                                ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, subject.Alternatives[0].OLevelSubject.Id);
                            }
                            if (subject.OtherAlternatives != null && subject.OtherAlternatives.Count > 0)
                            {
                                ViewData["OtherOLevelSubjectId" + i] = new SelectList(existingViewModel.OLevelSubjectSelectList, VALUE, TEXT, subject.OtherAlternatives[0].OLevelSubject.Id);
                            }
                            ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, VALUE, TEXT, subject.MinimumGrade.Id);

                        }
                        else
                        {
                            ViewData["FirstSittingOLevelSubjectId" + i] = new SelectList(clearanceViewModel.OLevelSubjectSelectList, VALUE, TEXT, 0);
                            ViewData["SecondSittingOLevelSubjectId" + i] = new SelectList(clearanceViewModel.OLevelSubjectSelectList, VALUE, TEXT, 0);
                            ViewData["OtherOLevelSubjectId" + i] = new SelectList(clearanceViewModel.OLevelSubjectSelectList, VALUE, TEXT, 0);
                            ViewData["FirstSittingOLevelGradeId" + i] = new SelectList(existingViewModel.OLevelGradeSelectList, VALUE, TEXT, 0);

                        }

                        i++;
                    }

                    AdmissionCriteriaForOLevelSubject sub = new AdmissionCriteriaForOLevelSubject();
                    sub.Id = -1;
                    sub.MainCriteria = existingViewModel.admissionCriteriaForOLevelSubject[0].MainCriteria;
                    sub.Alternatives[0].OLevelSubject.Id = -1;
                    //sub.Alternatives[1].OLevelSubject.Id = -1;


                    for (int u = 0; u < 5; u++)
                    {
                        existingViewModel.admissionCriteriaForOLevelSubject.Add(sub);
                    }


                }


            }
            catch (Exception ex)
            {
                SetMessage("Error Occurred! " + ex.Message, Message.Category.Error);
            }
        }
        public JsonResult GetSubjectAlternatives(long criteriaForOLevelSubjectId)
        {
            CriteriaJsonResult result = new CriteriaJsonResult();
            try
            {
                if (criteriaForOLevelSubjectId > 0)
                {
                    AdmissionCriteriaForOLevelSubjectLogic criteriaForOLevelSubjectLogic = new AdmissionCriteriaForOLevelSubjectLogic();
                    AdmissionCriteriaForOLevelSubjectAlternativeLogic alternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();

                    AdmissionCriteriaForOLevelSubject criteriaForOLevelSubject = criteriaForOLevelSubjectLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId).LastOrDefault();
                    if (criteriaForOLevelSubject != null)
                    {
                        result.Subject = criteriaForOLevelSubject.Subject.Name;
                    }
                    List<AdmissionCriteriaForOLevelSubjectAlternative> alternatives = alternativeLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId);
                    if (alternatives.Count > 0)
                    {
                        for (int i = 0; i < alternatives.Count; i++)
                        {
                            result.Alternatives += alternatives[i].OLevelSubject.Name + ", ";
                        }
                    }
                    else
                    {
                        result.Alternatives = "";
                    }

                    result.IsError = false;
                }
                else
                {
                    result.IsError = true;
                    result.Message = "Criteria for this subject does not exist";
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddSubjectAlternative(long criteriaForOLevelSubjectId, int oLevelSubjectIdToAdd)
        {
            CriteriaJsonResult result = new CriteriaJsonResult();
            try
            {
                if (criteriaForOLevelSubjectId > 0 && oLevelSubjectIdToAdd > 0)
                {
                    AdmissionCriteriaForOLevelSubjectLogic criteriaForOLevelSubjectLogic = new AdmissionCriteriaForOLevelSubjectLogic();
                    AdmissionCriteriaForOLevelSubjectAlternativeLogic alternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();

                    AdmissionCriteriaForOLevelSubject criteriaForOLevelSubject = criteriaForOLevelSubjectLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId).LastOrDefault();
                    if (criteriaForOLevelSubject != null)
                    {
                        result.Subject = criteriaForOLevelSubject.Subject.Name;

                        AdmissionCriteriaForOLevelSubjectAlternative existingAlternative = alternativeLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId && a.O_Level_Subject_Id == oLevelSubjectIdToAdd).LastOrDefault();
                        if (existingAlternative == null)
                        {
                            existingAlternative = new AdmissionCriteriaForOLevelSubjectAlternative();
                            existingAlternative.OLevelSubject = new OLevelSubject() { Id = oLevelSubjectIdToAdd };
                            existingAlternative.Alternative = criteriaForOLevelSubject;

                            alternativeLogic.Create(existingAlternative);

                            List<AdmissionCriteriaForOLevelSubjectAlternative> alternatives = alternativeLogic.GetModelsBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId);
                            if (alternatives.Count > 0)
                            {
                                for (int i = 0; i < alternatives.Count; i++)
                                {
                                    if (alternatives[i].OLevelSubject != null)
                                    {
                                        result.Alternatives += alternatives[i].OLevelSubject.Name + ", ";
                                    }
                                    else
                                    {
                                        OLevelSubjectLogic oLevelSubjectLogic = new OLevelSubjectLogic();
                                        OLevelSubject oLevelSubject = oLevelSubjectLogic.GetModelBy(o => o.O_Level_Subject_Id == oLevelSubjectIdToAdd);
                                        if (oLevelSubject != null)
                                        {
                                            result.Alternatives += oLevelSubject.Name + ", ";
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            result.IsError = true;
                            result.Message = "Alternative exist.";
                        }
                    }
                    else
                    {
                        result.IsError = true;
                        result.Message = "Criteria for this OLevel Subject does not exist.";
                    }
                }
                else
                {
                    result.IsError = true;
                    result.Message = "No OLevel Subject was selected.";
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClearSubjectAlternatives(long criteriaForOLevelSubjectId)
        {
            CriteriaJsonResult result = new CriteriaJsonResult();
            try
            {
                if (criteriaForOLevelSubjectId > 0)
                {
                    AdmissionCriteriaForOLevelSubjectLogic criteriaForOLevelSubjectLogic = new AdmissionCriteriaForOLevelSubjectLogic();
                    AdmissionCriteriaForOLevelSubjectAlternativeLogic alternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();

                    AdmissionCriteriaForOLevelSubject criteriaForOLevelSubject = criteriaForOLevelSubjectLogic.GetModelBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId);
                    if (criteriaForOLevelSubject != null)
                    {
                        alternativeLogic.Delete(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId);
                    }

                    result.Message = "Operation Successful!";
                    result.IsError = false;
                }
                else
                {
                    result.IsError = true;
                    result.Message = "Criteria for this subject does not exist";
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteAdmissionCriteria(long criteriaForOLevelSubjectId)
        {
            CriteriaJsonResult result = new CriteriaJsonResult();
            try
            {
                if (criteriaForOLevelSubjectId > 0)
                {
                    AdmissionCriteriaForOLevelSubjectLogic criteriaForOLevelSubjectLogic = new AdmissionCriteriaForOLevelSubjectLogic();
                    AdmissionCriteriaForOLevelSubjectAlternativeLogic alternativeLogic = new AdmissionCriteriaForOLevelSubjectAlternativeLogic();

                    AdmissionCriteriaForOLevelSubject criteriaForOLevelSubject = criteriaForOLevelSubjectLogic.GetModelBy(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId);
                    if (criteriaForOLevelSubject != null)
                    {
                        alternativeLogic.Delete(a => a.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId);
                        criteriaForOLevelSubjectLogic.Delete(c => c.Admission_Criteria_For_O_Level_Subject_Id == criteriaForOLevelSubjectId);
                    }

                    result.Message = "Operation Successful!";
                    result.IsError = false;
                }
                else
                {
                    result.IsError = true;
                    result.Message = "Criteria for this subject does not exist";
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}