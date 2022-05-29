using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class JambOlevelDetailLogic : BusinessBaseLogic<JambOlevelDetail, JAMB_O_LEVEL_DETAIL>
    {
        public JambOlevelDetailLogic()
        {
            translator = new JambOlevelDetailTranslator();
        }
        public bool Modify(JambOlevelDetail jambOlevelDetail)
        {
            try
            {
                Expression<Func<JAMB_O_LEVEL_DETAIL, bool>> selector = n => n.Id == jambOlevelDetail.Id;
                JAMB_O_LEVEL_DETAIL entity = GetEntityBy(selector);

                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                entity.Grade_Id = jambOlevelDetail.OLevelGrade.Id;
                if (jambOlevelDetail.OLevelSubject != null && jambOlevelDetail.OLevelSubject.Id > 0)
                {
                    entity.Subject_Id = jambOlevelDetail.OLevelSubject.Id;

                }

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    //throw new Exception(NoItemModified);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
