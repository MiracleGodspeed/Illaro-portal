using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class ClearanceLogLogic : BusinessBaseLogic<ClearanceLog, CLEARANCE_LOG>
    {
        public ClearanceLogLogic()
        {
            translator = new ClearanceLogTranslator();
        }
        public bool Modify(ClearanceLog model)
        {
            try
            {
                Expression<Func<CLEARANCE_LOG, bool>> selector = r => r.Id == model.Id;
                CLEARANCE_LOG entity = GetEntityBy(selector);

                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                entity.Remarks = model.Remarks;
                if (model.User != null)
                {
                    entity.User_Id = model.User.Id;
                }
                if (model.ClearanceStatus != null)
                {
                    entity.Clearance_Status_Id = model.ClearanceStatus.Id;
                }
                entity.Date_Cleared = model.DateCleared;
                entity.Closed = model.Closed;
                entity.Client = model.Client;

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
    }
}
