﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public class ApplicantResult
    {
        public string JambRegNumber { get; set; }
        public short? JambScore { get; set; }
        public string JambSubjects { get; set; }
        public string OLevelType { get; set; }
        public string Sitting { get; set; }
        public int NumberOfSittings { get; set; }
        public string OLevelResults { get; set; }
        public string SubjectName { get; set; }
        public string Grade { get; set; }
        public string OLevelYear { get; set; }
        public string Programme { get; set; }
        public string Department { get; set; }
        public string Session { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string ApplicationFormNumber { get; set; }
        public string InstitutionChoice { get; set; }

        public string Sex { get; set; }
        public string State { get; set; }
        public string LocalGovernment { get; set; }
        public string ExamNumber { get; set; }
        public string RejectReason { get; set; }
        public bool Rejected { get; set; }
        public string RejectedStr { get; set; }
        public string PreviousCourse { get; set; }
        public string PreviousEducationPeriod { get; set; }
        public string PreviousSchoolName { get; set; }
        public string EducationalQualificationName { get; set; }
        public string PreviousEducationResultGrade { get; set; }
        public string ITDurationName { get; set; }
        public double? ScreeningScore { get; set; }
        public long PersonId { get; set; }
        public string DepartmentOption { get; set; }
        public string ScannedCopyUrl { get; set; }          
    }
}
