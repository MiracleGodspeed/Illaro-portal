using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Web.ModelBinding;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Translator;
using Abundance_Nk.Model.Entity.Model;

namespace Abundance_Nk.Business
{
    public class TranscriptRequestLogic : BusinessBaseLogic<TranscriptRequest,TRANSCRIPT_REQUEST>
    {
        public TranscriptRequestLogic()
        {
            translator = new TranscriptRequestTranslator();
        }

        public TranscriptRequest GetBy(long Id)
        {
            TranscriptRequest request = null;
            try
            {
                request = GetModelBy(a => a.Student_id == Id && a.Transcript_Status_Id < 5);
            }
            catch (Exception)
            {
                
                throw;
            }
            return request;
        }

        public List<TranscriptRequest> GetBy(Student student)
        {
            List<TranscriptRequest> request = null;
            try
            {
                request = GetModelsBy(a => a.Student_id == student.Id);
            }
            catch (Exception)
            {

                throw;
            }
            return request;
        }

        public bool Modify (TranscriptRequest model)
        {
            try
            {
                Expression<Func<TRANSCRIPT_REQUEST, bool>> selector = af => af.Transcript_Request_Id == model.Id;
                TRANSCRIPT_REQUEST entity = GetEntityBy(selector);
                if (entity == null)
                {
                    throw new Exception(NoItemFound);
                }

                if (model.payment != null)
                {
                    entity.Payment_Id = model.payment.Id;
                }

                entity.Destination_Address = model.DestinationAddress;

                if (model.DestinationState != null && !string.IsNullOrEmpty(model.DestinationState.Id))
                {
                    entity.Destination_State_Id = model.DestinationState.Id;
                }

                if (model.DestinationCountry != null && !string.IsNullOrEmpty(model.DestinationCountry.Id))
                {
                    entity.Destination_Country_Id = model.DestinationCountry.Id;
                }
                
                entity.Transcript_clearance_Status_Id = model.transcriptClearanceStatus.TranscriptClearanceStatusId;
                entity.Transcript_Status_Id = model.transcriptStatus.TranscriptStatusId;

                if (model.DeliveryServiceZone != null)
                {
                    entity.Delivery_Service_Zone_Id = model.DeliveryServiceZone.Id;
                }

                int modifiedRecordCount = Save();
                if (modifiedRecordCount <= 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
        public List<TranscriptRequest> GetTranscriptRequests(DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_TRANSCRIPT_REQUEST_REMITA>(x => x.Date_Requested <= DateTo && x.Date_Requested >= DateFrom)
                                                            select new TranscriptRequest()
                                                          {
                                                              Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                              MatricNumber = sr.Matric_Number,
                                                              DateRequested = sr.Date_Requested,
                                                              DestinationAddress = sr.Destination_Address,
                                                              Status = sr.Transcript_Status_Name,
                                                              DeliveryService = sr.Delivery_Service,
                                                              GeoZone = sr.Geo_Zone,
                                                              StateName = sr.State_Name,
                                                              CountryName = sr.Country_Name,
                                                              InvoiceNumber = sr.Invoice_Number,
                                                              RRR = sr.RRR,
                                                              TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                              TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                              ProgrammeName = sr.Programme_Name,
                                                              DepartmentName = sr.Department_Name,
                                                          }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<TranscriptRequest> GetTranscriptRequestsByVerification(DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_TRANSCRIPT_VERIFICATION_REMITA>(x => x.Date_Requested <= DateTo && x.Date_Requested >= DateFrom)
                                                            select new TranscriptRequest()
                                                            {
                                                                Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                                MatricNumber = sr.Matric_Number,
                                                                DateRequested = sr.Date_Requested,
                                                                DestinationAddress = sr.Destination_Address,
                                                                Status = sr.Transcript_Status_Name,
                                                                DeliveryService = sr.Delivery_Service,
                                                                GeoZone = sr.Geo_Zone,
                                                                StateName = sr.State_Name,
                                                                CountryName = sr.Country_Name,
                                                                InvoiceNumber = sr.Invoice_Number,
                                                                RRR = sr.RRR,
                                                                TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                                TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                                ProgrammeName = sr.Programme_Name,
                                                                DepartmentName = sr.Department_Name,
                                                                Id = sr.Transcript_Request_Id,
                                                               
                                                            }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<TranscriptRequest> GetTranscriptVerification()
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_TRANSCRIPT_VERIFICATION_REMITA2>()
                                                            select new TranscriptRequest()
                                                            {
                                                                Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                                MatricNumber = sr.Matric_Number,
                                                                DateRequested = sr.Date_Requested,
                                                                DestinationAddress = sr.Destination_Address,
                                                                Status = sr.Transcript_Status_Name,
                                                                DeliveryService = sr.Delivery_Service,
                                                                GeoZone = sr.Geo_Zone,
                                                                StateName = sr.State_Name,
                                                                CountryName = sr.Country_Name,
                                                                InvoiceNumber = sr.Invoice_Number,
                                                                RRR = sr.RRR,
                                                                TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                                TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                                ProgrammeName = sr.Programme_Name,
                                                                DepartmentName = sr.Department_Name,
                                                                Id = sr.Transcript_Request_Id,
                                                                PaymentId=(long)sr.Payment_Id,
                                                                TranscriptName = sr.Transcript_Status_Name

                                                            }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //bursar
        public List<TranscriptRequest> GetTranscriptRequests()
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_TRANSCRIPT_REQUEST_REMITA>()
                                                            select new TranscriptRequest()
                                                            {
                                                                Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                                MatricNumber = sr.Matric_Number,
                                                                DateRequested = sr.Date_Requested,
                                                                DestinationAddress = sr.Destination_Address,
                                                                Status = sr.Transcript_Status_Name,
                                                                DeliveryService = sr.Delivery_Service,
                                                                GeoZone = sr.Geo_Zone,
                                                                StateName = sr.State_Name,
                                                                CountryName = sr.Country_Name,
                                                                InvoiceNumber = sr.Invoice_Number,
                                                                RRR = sr.RRR,
                                                                TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                                TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                                ProgrammeName = sr.Programme_Name,
                                                                DepartmentName = sr.Department_Name,
                                                                PaymentId=(long)sr.Payment_Id,
                                                                transcriptStatusId = sr.Transcript_Status_Id,
                                                                TranscriptName=sr.Transcript_Status_Name
                                                            }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        

        public List<TranscriptRequest> GetCertificateVerification()
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_CERTIFICATE_VERFICATION_REMITA>()
                                                            select new TranscriptRequest()
                                                            {
                                                                Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                                MatricNumber = sr.Matric_Number,
                                                                DateRequested = sr.Date_Requested,
                                                                DestinationAddress = sr.Destination_Address,
                                                                Status = sr.Transcript_Status_Name,
                                                                DeliveryService = sr.Delivery_Service,
                                                                GeoZone = sr.Geo_Zone,
                                                                StateName = sr.State_Name,
                                                                CountryName = sr.Country_Name,
                                                                InvoiceNumber = sr.Invoice_Number,
                                                                RRR = sr.RRR,
                                                                TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                                TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                                ProgrammeName = sr.Programme_Name,
                                                                DepartmentName = sr.Department_Name,
                                                                PaymentId = (long)sr.Payment_Id,
                                                                transcriptStatusId = sr.Transcript_Status_Id,
                                                                TranscriptName = sr.Transcript_Status_Name
                                                            }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<TranscriptRequest> GetVerifiedTranscriptRequestPayment()
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_VERIFIED_TRANSCRIPT_PAYMENT>()
                                                            select new TranscriptRequest()
                                                            {
                                                                Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                                MatricNumber = sr.Matric_Number,
                                                                DateRequested = sr.Date_Requested,
                                                                DestinationAddress = sr.Destination_Address,
                                                                Status = sr.Transcript_Status_Name,
                                                                DeliveryService = sr.Delivery_Service,
                                                                GeoZone = sr.Geo_Zone,
                                                                StateName = sr.State_Name,
                                                                CountryName = sr.Country_Name,
                                                                InvoiceNumber = sr.Invoice_Number,
                                                                RRR = sr.RRR,
                                                                TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                                TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                                ProgrammeName = sr.Programme_Name,
                                                                DepartmentName = sr.Department_Name,
                                                                PaymentId = (long)sr.Payment_Id,
                                                                transcriptStatusId = sr.Transcript_Status_Id,
                                                                TranscriptName = sr.Transcript_Status_Name
                                                            }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<TranscriptRequest> GetVerifiedCertificateVerificationPayment()
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_VERIFIED_CERTIFICATE_VERIFICATION_PAYMENT>()
                                                            select new TranscriptRequest()
                                                            {
                                                                Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                                MatricNumber = sr.Matric_Number,
                                                                DateRequested = sr.Date_Requested,
                                                                DestinationAddress = sr.Destination_Address,
                                                                Status = sr.Transcript_Status_Name,
                                                                DeliveryService = sr.Delivery_Service,
                                                                GeoZone = sr.Geo_Zone,
                                                                StateName = sr.State_Name,
                                                                CountryName = sr.Country_Name,
                                                                InvoiceNumber = sr.Invoice_Number,
                                                                RRR = sr.RRR,
                                                                TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                                TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                                ProgrammeName = sr.Programme_Name,
                                                                DepartmentName = sr.Department_Name,
                                                                PaymentId = (long)sr.Payment_Id,
                                                                transcriptStatusId = sr.Transcript_Status_Id,
                                                                TranscriptName = sr.Transcript_Status_Name
                                                            }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<TranscriptRequest> GetVerifiedTranscriptVerificationPayment()
        {
            try
            {
                List<TranscriptRequest> applicantResults = (from sr in repository.GetBy<VW_VERIFIED_TRANSCRIPT_VERIFICATION_PAYMENT>()
                                                            select new TranscriptRequest()
                                                            {
                                                                Name = sr.Last_Name + " " + sr.First_Name + " " + sr.Other_Name,
                                                                MatricNumber = sr.Matric_Number,
                                                                DateRequested = sr.Date_Requested,
                                                                DestinationAddress = sr.Destination_Address,
                                                                Status = sr.Transcript_Status_Name,
                                                                DeliveryService = sr.Delivery_Service,
                                                                GeoZone = sr.Geo_Zone,
                                                                StateName = sr.State_Name,
                                                                CountryName = sr.Country_Name,
                                                                InvoiceNumber = sr.Invoice_Number,
                                                                RRR = sr.RRR,
                                                                TransactionDate = sr.Transaction_Date.ToLongDateString(),
                                                                TransactionAmount = Convert.ToString(sr.Transaction_Amount),
                                                                ProgrammeName = sr.Programme_Name,
                                                                DepartmentName = sr.Department_Name,
                                                                Id = sr.Transcript_Request_Id,
                                                                PaymentId = (long)sr.Payment_Id,
                                                                TranscriptName = sr.Transcript_Status_Name

                                                            }).ToList();

                return applicantResults.OrderBy(m => m.Name).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<AlumniRecord> GetAlumniRecord()
        {
            Expression<Func<TRANSCRIPT_REQUEST, bool>> selector = p => p.Payment_Id != null && p.Transcript_Status_Id>=3 && p.PlaceOfWork!=null && p.YearOfGraduation!=null;
            List<TRANSCRIPT_REQUEST> entities = GetEntitiesBy(selector);
            StudentLogic studentLogic = new StudentLogic();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            List<AlumniRecord> alumniRecordList = new List<AlumniRecord>();
            if (entities?.Count > 0)
            {
                var disticntStudent=entities.Distinct().GroupBy(g => g.Student_id).ToList();
                foreach(var item in disticntStudent)
                {
                    var student=studentLogic.GetModelsBy(f => f.Person_Id == item.Key).FirstOrDefault();
                    if (student?.Id > 0)
                    {

                        var studentLevel=studentLevelLogic.GetModelsBy(f => f.Person_Id == item.Key).FirstOrDefault();
                        var request=entities.Where(f => f.Student_id == item.Key).LastOrDefault();
                        if (studentLevel?.Id > 0)
                        {
                            AlumniRecord alumniRecord = new AlumniRecord();
                            alumniRecord.AlumniNumber = "ALUMNUS-" + student.Id;
                            alumniRecord.Contact = student.ContactAddress;
                            alumniRecord.DepartmentName = studentLevel.Department.Name;
                            alumniRecord.ProgrammeName = studentLevel.Programme.Name;
                            alumniRecord.FirstName = student.FirstName;
                            alumniRecord.OtherName = student.OtherName;
                            alumniRecord.MatricNumber = student.MatricNumber;
                            alumniRecord.PhoneNo = student.MobilePhone;
                            alumniRecord.WorkPlace = request.PlaceOfWork;
                            alumniRecord.GraduationYear = request.YearOfGraduation!=null? request.YearOfGraduation.ToString():"";
                            alumniRecord.Email = student.Email;
                            alumniRecord.LastName = student.LastName;
                            alumniRecordList.Add(alumniRecord);
                        }
                        
                    }
                }
            }
            return alumniRecordList;
        }
        public List<AlumniRecord> GetAlumniRecordByName(string Name)
        {
            Expression<Func<TRANSCRIPT_REQUEST, bool>> selector = p => p.Payment_Id != null && p.Transcript_Status_Id >= 3 && p.PlaceOfWork != null && p.YearOfGraduation != null && (p.STUDENT.PERSON.First_Name.Contains(Name) || p.STUDENT.PERSON.Other_Name.Contains(Name)  || p.STUDENT.PERSON.Last_Name.Contains(Name));
            List<TRANSCRIPT_REQUEST> entities = GetEntitiesBy(selector);
            StudentLogic studentLogic = new StudentLogic();
            StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
            List<AlumniRecord> alumniRecordList = new List<AlumniRecord>();
            if (entities?.Count > 0)
            {
                var disticntStudent = entities.Distinct().GroupBy(g => g.Student_id).ToList();
                foreach (var item in disticntStudent)
                {
                    var student = studentLogic.GetModelsBy(f => f.Person_Id == item.Key).FirstOrDefault();
                    if (student?.Id > 0)
                    {

                        var studentLevel = studentLevelLogic.GetModelsBy(f => f.Person_Id == item.Key).FirstOrDefault();
                        var request = entities.Where(f => f.Student_id == item.Key).LastOrDefault();
                        if (studentLevel?.Id > 0)
                        {
                            AlumniRecord alumniRecord = new AlumniRecord();
                            alumniRecord.AlumniNumber = "ALUMNUS-" + student.Id;
                            alumniRecord.Contact = student.ContactAddress;
                            alumniRecord.DepartmentName = studentLevel.Department.Name;
                            alumniRecord.ProgrammeName = studentLevel.Programme.Name;
                            alumniRecord.FirstName = student.FirstName;
                            alumniRecord.OtherName = student.OtherName;
                            alumniRecord.MatricNumber = student.MatricNumber;
                            alumniRecord.PhoneNo = student.MobilePhone;
                            alumniRecord.WorkPlace = request.PlaceOfWork;
                            alumniRecord.GraduationYear = request.YearOfGraduation != null ? request.YearOfGraduation.ToString() : "";
                            alumniRecord.Email = student.Email;
                            alumniRecord.LastName = student.LastName;
                            alumniRecordList.Add(alumniRecord);
                        }

                    }
                }
            }
            return alumniRecordList;
        }
        public void DeleteBy(long PaymentID)
        {
            try
            {
                Expression<Func<TRANSCRIPT_REQUEST, bool>> selector = a => a.Payment_Id == PaymentID;
                Delete(selector);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
