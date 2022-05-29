using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;

namespace Abundance_Nk.Business
{
    public class StudentMatricNumberAssignmentLogic : BusinessBaseLogic<StudentMatricNumberAssignment, STUDENT_MATRIC_NUMBER_ASSIGNMENT>
    {
        public StudentMatricNumberAssignmentLogic()
        {
            translator = new StudentMatricNumberAssignmentTranslator();
        }

        public StudentMatricNumberAssignment GetBy(Faculty faculty, Department department,Programme programme, Level level, Session session)
        {
            try
            {
                Expression<Func<STUDENT_MATRIC_NUMBER_ASSIGNMENT, bool>> selector = s => s.Department_Id == department.Id && s.Programme_Id == programme.Id && s.Level_Id == level.Id && s.Session_Id == session.Id;
                return GetModelBy(selector);
            }
            catch(Exception)
            {
                throw;
            }
        }

        public bool IsInvalid(string matricNo)
        {
            try
            {
                Expression<Func<STUDENT_MATRIC_NUMBER_ASSIGNMENT, bool>> selector = s => s.Matric_Number_Start_From.Contains(matricNo);
                List<StudentMatricNumberAssignment> studentMatricNos = GetModelsBy(selector);
                if (studentMatricNos == null || studentMatricNos.Count <= 0)
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

        public bool MarkAsUsed(StudentMatricNumberAssignment studentMatricNumberAssignment)
        {
            try
            {
                Expression<Func<STUDENT_MATRIC_NUMBER_ASSIGNMENT, bool>> selector = s => s.Faculty_Id == studentMatricNumberAssignment.Faculty.Id && s.Department_Id == studentMatricNumberAssignment.Department.Id && s.Programme_Id == studentMatricNumberAssignment.Programme.Id && s.Level_Id == studentMatricNumberAssignment.Level.Id && s.Session_Id == studentMatricNumberAssignment.Session.Id;
                STUDENT_MATRIC_NUMBER_ASSIGNMENT entity = GetEntityBy(selector);
                
                entity.Used = true;

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    throw new Exception(NoItemModified);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool Modify(StudentMatricNumberAssignment model)
        {
            try
            {
                Expression<Func<STUDENT_MATRIC_NUMBER_ASSIGNMENT, bool>> selector = a => a.Department_Id == model.Department.Id && a.Programme_Id == model.Programme.Id && a.Level_Id == 
                                                                        model.Level.Id && a.Session_Id == model.Session.Id;

                STUDENT_MATRIC_NUMBER_ASSIGNMENT entity = GetEntityBy(selector);

                if (entity != null)
                {
                    entity.Matric_Serial_Number_Start_From = model.MatricSerialNoStartFrom;

                    int modifiedRecordCount = Save();

                    if (modifiedRecordCount > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }



}
