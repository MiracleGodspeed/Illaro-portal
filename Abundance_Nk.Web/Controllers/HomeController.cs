using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Models;

namespace Abundance_Nk.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                DisplayMessageLogic messageLogic = new DisplayMessageLogic();
                List<DisplayMessage> displaymessage = new List<DisplayMessage>();

                displaymessage = messageLogic.GetModelsBy(b => b.Activated);

                List<string> messages = new List<string>();
                foreach (var message in displaymessage)
                {
                    messages.Add(message.Message);

                }

                ViewBag.message = messages;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        public ActionResult About()
        {
            //SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
            //SessionSemester sessionSemester = sessionSemesterLogic.GetBy(9);

            //LevelLogic levelLogic = new LevelLogic();
            //Level level = levelLogic.GetModelBy(a => a.Level_Id == 2);

            //ProgrammeLogic programmeLogic = new ProgrammeLogic();
            //Programme programme = programmeLogic.GetModelBy(a => a.Programme_Id == 1);

            //DepartmentLogic departmentLogic = new DepartmentLogic();
            //Department department = departmentLogic.GetModelBy(a => a.Department_Id == 25);

            //CourseModeLogic courseModeLogic = new CourseModeLogic();
            //CourseMode courseMode = courseModeLogic.GetModelBy(a => a.Course_Mode_Id == 1);

            //AggregateSheetLogic aggregateSheetLogic = new AggregateSheetLogic();
            //aggregateSheetLogic.GetMaterSheetDetailsByMode(sessionSemester, level,programme,department, courseMode);
            //ViewBag.Message = "";

            return View();
        }
        
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}