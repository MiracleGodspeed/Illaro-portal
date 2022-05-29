using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;

namespace Abundance_Nk.Model.Translator
{
    public class StudentPaymentTranslator : TranslatorBase<StudentPayment, STUDENT_PAYMENT>
    {
        private PersonTypeTranslator personTypeTranslator;
        private PaymentTypeTranslator paymentTypeTranslator;
        private PaymentTranslator paymentTranslator;
        private StudentTranslator studentTranslator;
        private SessionSemesterTranslator sessionSemesterTranslator;
        private LevelTranslator levelTranslator;
        private PaymentModeTranslator paymentModeTranslator;
        private FeeTranslator feeTranslator;

        public StudentPaymentTranslator()
        {
            personTypeTranslator = new PersonTypeTranslator();
            paymentTypeTranslator = new PaymentTypeTranslator();
            paymentTranslator = new PaymentTranslator();
            studentTranslator = new StudentTranslator();
            sessionSemesterTranslator = new SessionSemesterTranslator();
            levelTranslator = new LevelTranslator();
            paymentModeTranslator = new PaymentModeTranslator();
            feeTranslator = new FeeTranslator();
        }

        public override StudentPayment TranslateToModel(STUDENT_PAYMENT studentPaymentEntity)
        {
            try
            {
                StudentPayment studentPayment = null;
                if (studentPaymentEntity != null)
                {
                    studentPayment = new StudentPayment();
                    studentPayment.Id = studentPaymentEntity.Payment_Id;

                    //studentPayment.PaymentMode = paymentModeTranslator.TranslateToModel(studentPaymentEntity.PAYMENT.PAYMENT_MODE);
                    //studentPayment.Fee = feeTranslator.TranslateToModel(studentPaymentEntity.PAYMENT.FEE);
                    //studentPayment.DatePaid = studentPaymentEntity.PAYMENT.Date_Paid;
                    //studentPayment.PaymentType = paymentTypeTranslator.Translate(studentPaymentEntity.PAYMENT.PAYMENT_TYPE);
                    //studentPayment.PersonType = personTypeTranslator.Translate(studentPaymentEntity.PAYMENT.PERSON_TYPE);

                    studentPayment.Student = studentTranslator.TranslateToModel(studentPaymentEntity.STUDENT);
                    studentPayment.SessionSemester = sessionSemesterTranslator.TranslateToModel(studentPaymentEntity.SESSION_SEMESTER);
                    studentPayment.Level = levelTranslator.TranslateToModel(studentPaymentEntity.LEVEL);
                }

                return studentPayment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override STUDENT_PAYMENT TranslateToEntity(StudentPayment studentPayment)
        {
            try
            {
                STUDENT_PAYMENT studentPaymentEntity = null;
                if (studentPayment != null)
                {
                    studentPaymentEntity = new STUDENT_PAYMENT();
                    studentPaymentEntity.Payment_Id = studentPayment.Id;
                    studentPaymentEntity.Person_Id = studentPayment.Student.Id;
                    studentPaymentEntity.Session_Semester_Id = studentPayment.SessionSemester.Id;
                    studentPaymentEntity.Level_Id = studentPayment.Level.Id;
                }

                return studentPaymentEntity;
            }
            catch (Exception)
            {
                throw;
            }
        }





    }
}
