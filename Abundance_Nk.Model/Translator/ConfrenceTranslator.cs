using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class ConfrenceTranslator : TranslatorBase<Confrence, CONFRENCE>
    {
        private PaymentModeTranslator paymentModeTranslator;
        private PersonTypeTranslator personTypeTranslator;
        private PaymentTypeTranslator paymentTypeTranslator;
        private FeeTypeTranslator feeTypeTranslator;
        private PersonTranslator personTranslator;
        private SessionTranslator sessionTranslator;

        public ConfrenceTranslator()
        {
            feeTypeTranslator = new FeeTypeTranslator();
            personTypeTranslator = new PersonTypeTranslator();
            paymentTypeTranslator = new PaymentTypeTranslator();
            paymentModeTranslator = new PaymentModeTranslator();
            personTranslator = new PersonTranslator();
            sessionTranslator = new SessionTranslator();
        }

        public override Confrence TranslateToModel(CONFRENCE confrenceEntity)
        {
            try
            {
                Confrence confrence = null;
                if (confrenceEntity != null)
                {
                    confrence = new Confrence();
                    confrence.Confrence_Id = confrenceEntity.Confrence_Id;
                    confrence.Person = personTranslator.Translate(confrenceEntity.PERSON);
                    confrence.Postal_Code = confrenceEntity.Postal_Code;
                    confrence.State = confrenceEntity.State;
                    confrence.Status = confrenceEntity.Status;
                    confrence.Institution = confrenceEntity.Institution;
                    confrence.Department = confrenceEntity.Department;
                    confrence.Country = confrenceEntity.Country;
                    confrence.Date_Applied = confrenceEntity.Date_Applied;
                    confrence.City = confrenceEntity.City;
                    confrence.Amount_Package = confrenceEntity.Amount_Package;
                    confrence.Research_DocumentUrl = confrenceEntity.Research_DocumentURl;
}

                return confrence;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override CONFRENCE TranslateToEntity(Confrence confrence)
        {
            try
            {
                CONFRENCE confrenceEntity = null;

                 if (confrence != null)
                {
                    confrenceEntity = new CONFRENCE();
                    confrenceEntity.Confrence_Id = confrence.Confrence_Id;
                    confrenceEntity.Person_Id = confrence.Person_Id;
                    confrenceEntity.Postal_Code = confrence.Postal_Code;
                    confrenceEntity.State = confrence.State;
                    confrenceEntity.Status = confrence.Status;
                    confrenceEntity.Institution = confrence.Institution;
                    confrenceEntity.Date_Applied = confrence.Date_Applied;
                    confrenceEntity.Country = confrence.Country;
                    confrenceEntity.City = confrence.City;
                    confrenceEntity.Amount_Package = confrence.Amount_Package;
                    confrenceEntity.Department = confrence.Department;
                    confrenceEntity.Research_DocumentURl = confrence.Research_DocumentUrl;


                  

                }

                return confrenceEntity;

            }
            catch (Exception)
            {
                throw;
            }
        }




    }
}
