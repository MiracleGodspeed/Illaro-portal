using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;

namespace Abundance_Nk.Business
{
    public class LevelLogic : BusinessBaseLogic<Level, LEVEL>
    {
        public LevelLogic()
        {
            translator = new LevelTranslator();
        }
        public List<Level> GetONDs()
        {
            try
            {
                System.Linq.Expressions.Expression<Func<LEVEL, bool>> selector = l => l.Level_Id <= 2;
                return base.GetModelsBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Level> GetHNDs()
        {
            try
            {
                System.Linq.Expressions.Expression<Func<LEVEL, bool>> selector = l => l.Level_Id > 2;
                return base.GetModelsBy(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }





    }
}



