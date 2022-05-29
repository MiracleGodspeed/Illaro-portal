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
    public class SessionLogic : BusinessBaseLogic<Session, SESSION>
    {
        public SessionLogic()
        {
            translator = new SessionTranslator();
        }

        public bool Modify(Session session)
        {
            try
            {
                Expression<Func<SESSION, bool>> selector = s => s.Session_Id == session.Id;
                SESSION entity = GetEntityBy(selector);

                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                entity.Session_Name = session.Name;
                entity.Start_Date = session.StartDate;
                entity.End_date = session.EndDate;
                entity.Active_For_Result = session.ActiveForResult;
                entity.Active_For_Allocation = session.ActiveForAllocation;
                entity.Active_For_Application = session.ActiveForApplication;
                entity.Active_For_Hostel = session.ActiveForHostel;
                entity.Active_For_Fees = session.ActiveForFees;

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
        
        public List<Session> GetActiveSessions()
        {
            List<Session> sessions = new List<Session>();
            try
            {
              return  GetModelsBy(a => a.Activated == true).OrderByDescending(k => k.Name).ToList();
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public Session GetPreviousSession(Session session)
        {
            return GetPreviousSession(session.Id);
        }

        public Session GetPreviousSession(int sessionId)
        {
            Session currentSession = GetModelBy(s => s.Session_Id == sessionId);
            if (currentSession != null && currentSession.Id > 0)
            {
                int SessionName = Convert.ToInt32(currentSession.Name.Substring(0, 4));
                var AllSessions = GetAll();
                SessionName--;
                foreach (Session ses in AllSessions)
                {
                    if(Convert.ToInt32(ses.Name.Substring(0, 4)) == SessionName)
                    {
                        return ses;
                    }
                }
                
            }

            return GetAll().FirstOrDefault();
            
           
        }



        public List<Session> GetFeeSession()
        {
            try
            {
                return GetModelsBy(s => s.Active_For_Fees != null && s.Active_For_Fees.Value);
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
                return GetModelBy(s => s.Active_For_Application != null && s.Active_For_Application.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Session GetHostelSession()
        {
            try
            {
                return GetModelsBy(s => s.Active_For_Hostel.Value).LastOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

