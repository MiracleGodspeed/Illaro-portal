using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
   public class ComplianLogTranslator: TranslatorBase<ComplaintLog,COMPLAIN_LOG>
    {
       ProgrammeTranslator programmeTranslator;
       DepartmentTranslator departmentTranslator;
       public ComplianLogTranslator()
       {
           programmeTranslator = new ProgrammeTranslator();
           departmentTranslator = new DepartmentTranslator();
       }

        public override ComplaintLog TranslateToModel(COMPLAIN_LOG entity)
        {
            try
            {
                ComplaintLog model = null;
                if (entity != null)
                {
                    model = new ComplaintLog();
                    model.Id = entity.Id;
                    model.Name = entity.Name;
                    model.Complain = entity.Complain;
                    model.ApplicationNumber = entity.Application_Number;
                    model.ConfirmationNumber = entity.Confirmation_Order_Number;
                    model.ExamNumber = entity.Exam_Number;
                    model.MobileNumber = entity.Mobile_Number;
                    model.RRR = entity.RRR;
                    model.Status = entity.Status;
                    model.TicketID = entity.Ticket_Id;
                    model.Department = departmentTranslator.Translate(entity.DEPARTMENT);
                    model.Programme = programmeTranslator.Translate(entity.PROGRAMME);
                    model.DateSubmitted = entity.Date_Submitted;
                    model.Comment = entity.Comment;
                }

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override COMPLAIN_LOG TranslateToEntity(ComplaintLog model)
        {
            try
            {
                COMPLAIN_LOG entity = null;
                if (model != null)
                {
                    entity = new COMPLAIN_LOG();
                    entity.Id = model.Id;
                    entity.Name = model.Name;
                    entity.Application_Number = model.ApplicationNumber;
                    entity.Complain = model.Complain;
                    entity.Confirmation_Order_Number = model.ConfirmationNumber;
                    entity.Exam_Number = model.ExamNumber;
                    entity.Mobile_Number = model.MobileNumber;
                    entity.RRR = model.RRR;
                    entity.Status = model.Status;
                    entity.Ticket_Id = model.TicketID;
                    entity.Department_Id = model.Department.Id;
                    entity.Programme_Id = model.Programme.Id;
                    entity.Date_Submitted = model.DateSubmitted;
                    entity.Comment = model.Comment;
                }

                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
