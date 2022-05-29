using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using System.Linq.Expressions;

namespace Abundance_Nk.Business
{
    public class SessionSemesterLogic : BusinessBaseLogic<SessionSemester, SESSION_SEMESTER>
    {
        private CurrentSessionSemesterLogic currentSessionSemesterLogic;

        public SessionSemesterLogic()
        {
            translator = new SessionSemesterTranslator();
            currentSessionSemesterLogic = new CurrentSessionSemesterLogic();
        }

        public SessionSemester GetBy(int id)
        {
            try
            {
                Expression<Func<SESSION_SEMESTER, bool>> selector = s => s.Session_Semester_Id == id;
                return base.GetModelBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Modify(SessionSemester sessionSemester)
        {
            try
            {
                Expression<Func<SESSION_SEMESTER, bool>> selector = s => s.Session_Semester_Id == sessionSemester.Id;
                SESSION_SEMESTER entity = GetEntityBy(selector);

                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                entity.Session_Id = sessionSemester.Session.Id;
                entity.Semester_Id = sessionSemester.Semester.Id;
                entity.Start_Date = sessionSemester.StartDate;
                entity.End_Date = sessionSemester.EndDate;

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

        public SessionSemester GetBySessionSemester(int semesterId, int sessionId)
        {
            try
            {
                Expression<Func<SESSION_SEMESTER, bool>> selector = s => s.Session_Id == sessionId && s.Semester_Id == semesterId;
                return base.GetModelBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SessionSemester GetPreviousSession(int SessionSemesterId)
        {
            SessionSemester currentSession = GetModelBy(s => s.Session_Semester_Id == SessionSemesterId);
            SessionSemesterLogic sessionSemesterLogic = new SessionSemesterLogic();
            if (currentSession != null && currentSession.Id > 0)
            {
                int SessionName = Convert.ToInt32(currentSession.Session.Name.Substring(0, 4));
                var AllSessionSemesters = GetAll();
                foreach (SessionSemester ses in AllSessionSemesters)
                {
                    //if (Convert.ToInt32(ses.Session.Name.Substring(0, 4)) == SessionName && ses.Semester.Id != currentSession.Semester.Id && (currentSession.Semester.Id == 2 || currentSession.Semester.Id == 3))
                    //{

                    //        var previousSessionSemesterId = currentSession.Id - 1;
                    //        var sessionSemester = sessionSemesterLogic.GetModelsBy(f => f.Session_Semester_Id == previousSessionSemesterId).FirstOrDefault();
                    //        return sessionSemester;

                    //}
                    //else
                    //{
                    //    //return currentSession;
                    //}
                    //get previous SessionSemester
                    var previousSessionName = Convert.ToInt32(ses.Session.Name.Substring(5, 4));
                    if ( ses.Semester.Id!=currentSession.Semester.Id && ((currentSession.Semester.Id==1 && previousSessionName==SessionName) || ((currentSession.Semester.Id == 2 || currentSession.Semester.Id == 3) && Convert.ToInt32(ses.Session.Name.Substring(0, 4)) == SessionName)))
                    {
                        var previousSessionSemesterId = currentSession.Id - 1;
                              var sessionSemester = sessionSemesterLogic.GetModelsBy(f => f.Session_Semester_Id == previousSessionSemesterId).FirstOrDefault();
                                return sessionSemester;
                    }
                }
                return currentSession;
            }

            return GetAll().FirstOrDefault();


        }
        

    }
}
