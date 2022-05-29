using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class ClearanceUnitLogic : BusinessBaseLogic<ClearanceUnit, CLEARANCE_UNITS>
    {
        public ClearanceUnitLogic()
        {
            translator = new ClearanceUnitTranslator();
        }
    }
}
