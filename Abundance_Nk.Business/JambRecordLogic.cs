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
    public class JambRecordLogic : BusinessBaseLogic<JambRecord, JAMB_RECORD>
    {
        public JambRecordLogic()
        {
            translator = new JambRecordTranslator();
        }
        public bool Modify(JambRecord jambRecord)
        {

            try
            {

                Expression<Func<JAMB_RECORD, bool>> selector = a => a.Jamb_Registration_Number == jambRecord.JambRegistrationNumber;
                JAMB_RECORD record = GetEntityBy(selector);

                if (record != null)
                {
                    if (jambRecord.Subject1 != null)
                    {
                        record.Subject1 = jambRecord.Subject1;
                    }
                    if (jambRecord.Subject2 != null)
                    {
                        record.Subject2 = jambRecord.Subject2;
                    }
                    if (jambRecord.Subject3 != null)
                    {
                        record.Subject3 = jambRecord.Subject3;
                    }
                    if (jambRecord.Subject4 != null)
                    {
                        record.Subject4 = jambRecord.Subject4;
                    }
                    if (jambRecord.Score1 > 0)
                    {
                        record.Score1 = jambRecord.Score1;
                    }
                    if (jambRecord.Score2 > 0)
                    {
                        record.Score2 = jambRecord.Score2;
                    }
                    if (jambRecord.Score3 > 0)
                    {
                        record.Score3 = jambRecord.Score3;
                    }
                    if (jambRecord.Score4 > 0)
                    {
                        record.Score4 = jambRecord.Score4;
                    }
                    if (jambRecord.TotalJambScore > 0)
                    {
                        record.Total_Jamb_Score = jambRecord.TotalJambScore;
                    }
                    if (jambRecord.Session != null)
                    {
                        record.Session_Id = jambRecord.Session.Id;
                    }
                    if (jambRecord.Course != null)
                    {
                        record.Course_Id = jambRecord.Course.Id;
                    }


                    int modifiedRecordCount = Save();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}
