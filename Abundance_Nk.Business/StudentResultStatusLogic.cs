using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;

namespace Abundance_Nk.Business
{
    public class StudentResultStatusLogic:BusinessBaseLogic<StudentResultStatus,STUDENT_RESULT_STATUS>
    {
        public StudentResultStatusLogic()
        {
            translator = new StudentResultStatusTranslator();
        }

        public bool Modify(StudentResultStatus studentResultStatus)
        {
            try
            {
                Expression<Func<STUDENT_RESULT_STATUS, bool>> selector = a => a.Id == studentResultStatus.Id;
                STUDENT_RESULT_STATUS entity = GetEntityBy(selector);
                if (entity != null && entity.Id > 0)
                {
                    if (studentResultStatus.Faculty != null)
                    {
                        entity.Faculty_Id = studentResultStatus.Faculty.Id;
                    }

                    if (studentResultStatus.Session != null)
                    {
                        entity.Session_Id = studentResultStatus.Session.Id;
                    }

                    entity.RegistrarApproval = studentResultStatus.RegistrarApproval;
                    entity.DRAcademicsApproval = studentResultStatus.DRAcademicsApproval;
                    entity.RAndDCApproval = studentResultStatus.RAndDCApproval;

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
