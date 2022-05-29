using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Business;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class CourseUnitController : Controller
    {
        private Abundance_NkEntities db = new Abundance_NkEntities();

        // GET: Admin/CourseUnit
        public ActionResult Index()
        {
            var cOURSE_UNIT = db.COURSE_UNIT.Include(c => c.DEPARTMENT).Include(c => c.LEVEL).Include(c => c.SEMESTER).Include(c => c.DEPARTMENT_OPTION).Include(c => c.PROGRAMME);
            return View(cOURSE_UNIT.ToList());
        }

        // GET: Admin/CourseUnit/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            COURSE_UNIT cOURSE_UNIT = db.COURSE_UNIT.Find(id);
            if (cOURSE_UNIT == null)
            {
                return HttpNotFound();
            }
            return View(cOURSE_UNIT);
        }

        // GET: Admin/CourseUnit/Create
        public ActionResult Create()
        {
            ViewBag.Department_Id = new SelectList(db.DEPARTMENT, "Department_Id", "Department_Name");
            ViewBag.Level_Id = new SelectList(db.LEVEL, "Level_Id", "Level_Name");
            ViewBag.Semester_Id = new SelectList(db.SEMESTER, "Semester_Id", "Semester_Name");
            DEPARTMENT_OPTION dEPARTMENT_OPTION = new DEPARTMENT_OPTION { Department_Option_Id = 0, Department_Option_Name = "----Select Department Option----" };
            List<DEPARTMENT_OPTION> allDepartmentOption = new List<DEPARTMENT_OPTION>();
            allDepartmentOption.Add(dEPARTMENT_OPTION);
            allDepartmentOption.AddRange(db.DEPARTMENT_OPTION);
            ViewBag.Department_Option_Id = new SelectList(allDepartmentOption, "Department_Option_Id", "Department_Option_Name");
            ViewBag.Programme_Id = new SelectList(db.PROGRAMME, "Programme_Id", "Programme_Name");
            return View();
        }

        // POST: Admin/CourseUnit/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Course_Unit_Id,Department_Id,Level_Id,Semester_Id,Minimum_Unit,Maximum_Unit,Department_Option_Id,Programme_Id")] COURSE_UNIT cOURSE_UNIT)
        {
            if (ModelState.IsValid)
            {
                DepartmentOptionLogic optionLogic = new DepartmentOptionLogic();
                var options = optionLogic.GetBy(new Department() { Id = cOURSE_UNIT.Department_Id }, new Programme { Id = (int)Programmes.HNDFullTime });

                if (options == null || options.Count <= 0 || cOURSE_UNIT.Level_Id <= 2 || cOURSE_UNIT.Department_Option_Id==0)
                {
                    cOURSE_UNIT.Department_Option_Id = null;
                }

                db.COURSE_UNIT.Add(cOURSE_UNIT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Department_Id = new SelectList(db.DEPARTMENT, "Department_Id", "Department_Name", cOURSE_UNIT.Department_Id);
            ViewBag.Level_Id = new SelectList(db.LEVEL, "Level_Id", "Level_Name", cOURSE_UNIT.Level_Id);
            ViewBag.Semester_Id = new SelectList(db.SEMESTER, "Semester_Id", "Semester_Name", cOURSE_UNIT.Semester_Id);
            DEPARTMENT_OPTION dEPARTMENT_OPTION = new DEPARTMENT_OPTION { Department_Option_Id = 0, Department_Option_Name = "----Select Department Option----" };
            List<DEPARTMENT_OPTION> allDepartmentOption = new List<DEPARTMENT_OPTION>();
            allDepartmentOption.Add(dEPARTMENT_OPTION);
            allDepartmentOption.AddRange(db.DEPARTMENT_OPTION);
            ViewBag.Department_Option_Id = new SelectList(allDepartmentOption, "Department_Option_Id", "Department_Option_Name");

            //ViewBag.Department_Option_Id = new SelectList(db.DEPARTMENT_OPTION, "Department_Option_Id", "Department_Option_Name", cOURSE_UNIT.Department_Option_Id);
            ViewBag.Programme_Id = new SelectList(db.PROGRAMME, "Programme_Id", "Programme_Name", cOURSE_UNIT.Programme_Id);
            return View(cOURSE_UNIT);
        }

        // GET: Admin/CourseUnit/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            COURSE_UNIT cOURSE_UNIT = db.COURSE_UNIT.Find(id);
            if (cOURSE_UNIT == null)
            {
                return HttpNotFound();
            }
            ViewBag.Department_Id = new SelectList(db.DEPARTMENT, "Department_Id", "Department_Name", cOURSE_UNIT.Department_Id);
            ViewBag.Level_Id = new SelectList(db.LEVEL, "Level_Id", "Level_Name", cOURSE_UNIT.Level_Id);
            ViewBag.Semester_Id = new SelectList(db.SEMESTER, "Semester_Id", "Semester_Name", cOURSE_UNIT.Semester_Id);
            DEPARTMENT_OPTION dEPARTMENT_OPTION = new DEPARTMENT_OPTION { Department_Option_Id = 0, Department_Option_Name = "----Select Department Option----" };
            List<DEPARTMENT_OPTION> allDepartmentOption = new List<DEPARTMENT_OPTION>();
            allDepartmentOption.Add(dEPARTMENT_OPTION);
            allDepartmentOption.AddRange(db.DEPARTMENT_OPTION);
            ViewBag.Department_Option_Id = new SelectList(allDepartmentOption, "Department_Option_Id", "Department_Option_Name");
            //ViewBag.Department_Option_Id = new SelectList(db.DEPARTMENT_OPTION, "Department_Option_Id", "Department_Option_Name", cOURSE_UNIT.Department_Option_Id);
            ViewBag.Programme_Id = new SelectList(db.PROGRAMME, "Programme_Id", "Programme_Name", cOURSE_UNIT.Programme_Id);
            return View(cOURSE_UNIT);
        }

        // POST: Admin/CourseUnit/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Course_Unit_Id,Department_Id,Level_Id,Semester_Id,Minimum_Unit,Maximum_Unit,Department_Option_Id,Programme_Id")] COURSE_UNIT cOURSE_UNIT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cOURSE_UNIT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Department_Id = new SelectList(db.DEPARTMENT, "Department_Id", "Department_Name", cOURSE_UNIT.Department_Id);
            ViewBag.Level_Id = new SelectList(db.LEVEL, "Level_Id", "Level_Name", cOURSE_UNIT.Level_Id);
            ViewBag.Semester_Id = new SelectList(db.SEMESTER, "Semester_Id", "Semester_Name", cOURSE_UNIT.Semester_Id);
            DEPARTMENT_OPTION dEPARTMENT_OPTION = new DEPARTMENT_OPTION { Department_Option_Id = 0, Department_Option_Name = "----Select Department Option----" };
            List<DEPARTMENT_OPTION> allDepartmentOption = new List<DEPARTMENT_OPTION>();
            allDepartmentOption.Add(dEPARTMENT_OPTION);
            allDepartmentOption.AddRange(db.DEPARTMENT_OPTION);
            ViewBag.Department_Option_Id = new SelectList(allDepartmentOption, "Department_Option_Id", "Department_Option_Name");
            //ViewBag.Department_Option_Id = new SelectList(db.DEPARTMENT_OPTION, "Department_Option_Id", "Department_Option_Name", cOURSE_UNIT.Department_Option_Id);
            ViewBag.Programme_Id = new SelectList(db.PROGRAMME, "Programme_Id", "Programme_Name", cOURSE_UNIT.Programme_Id);
            return View(cOURSE_UNIT);
        }

        // GET: Admin/CourseUnit/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            COURSE_UNIT cOURSE_UNIT = db.COURSE_UNIT.Find(id);
            if (cOURSE_UNIT == null)
            {
                return HttpNotFound();
            }
            return View(cOURSE_UNIT);
        }

        // POST: Admin/CourseUnit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            COURSE_UNIT cOURSE_UNIT = db.COURSE_UNIT.Find(id);
            db.COURSE_UNIT.Remove(cOURSE_UNIT);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
