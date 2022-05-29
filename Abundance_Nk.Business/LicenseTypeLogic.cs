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
    public class LicenseTypeLogic : BusinessBaseLogic<LicenseType, LICENSE_TYPE>
    {
        public LicenseTypeLogic()
        {
            translator = new LicenseTypeTranslator();
        }
    }
}
