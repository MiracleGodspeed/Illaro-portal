
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Model
{
    public enum FeeTypes
    {
        NDFullTimeApplicationForm = 1,
        AcceptanceFee = 2,
        SchoolFees = 3,
        HNDFullTimeApplicationForm = 4,
        NDPartTimeApplicationForm = 5,
        HNDAcceptance = 9,
        CarryOverSchoolFees = 10,
        ChangeOfCourseFees = 11,
        ShortFall = 12,
        Transcript = 13,
        HostelFee = 17,
        ConvocationFee = 18,
        CerificateCollection = 14,
        HNDWeekendApplicationForm = 19,
        Conference = 20,
        ConferenceWithTourism = 21,
        CertificateVerification=24,
        DrivingAcceptance = 26,
        DriversLicense = 29, //Production
        //DriversLicense = 1028, //Development
    }

    public enum CourseModes
    {
        FirstAttempt = 1,
        CarryOver = 2,
        ExtraYear = 3
    }
    public enum PersonTypes
    {
        Staff = 1,
        Parent = 2,
        Student = 3,
        Applicant = 4,
        ConferenceAttendee = 5
    }

    public enum PaymentModes
    {
        Full = 1,
        Part,
        FirstInstallment,
        SecondInstallment,
        ThirdInstallment
    }

    public enum Paymenttypes
    {
        CardPayment = 1,
        OnlinePayment = 2,
    }
    public enum Fees
    {
        CertificateFee_5K = 131,
        GraduateDrivingTrainingSchoolFees = 35000,
        AllDrivingAcceptance = 5000,
        ProfessionalDiplomaDrivingSchooFees = 60000,
        TechnicalCertificateDrivingSchooFees = 50000,
        CISCOCERTFIEDNETWORKASSOCIATED = 1185,
        RoboticS_UAE = 184,
        CISCOFeeAmount = 6000,
        RoboticsFeeAmount = 10000
        //CISCOCERTFIEDNETWORKASSOCIATED = 191,
        //RoboticS_UAE = 183,

    }

    public enum TranscriptFees
    {
        ///////Development/////
        //SouthWestNIG = 2180,
        //OthersNIG = 2182,
        //International_SP_IT_CAN_US = 2183,
        //InternationalOthers = 2184,
        //StudentCopyTranscript = 3185,

        //////Production//////
        SouthWestNIG = 1186,
        OthersNIG,
        International_SP_IT_CAN_US,
        InternationalOthers,
        StudentCopyTranscript = 2198


    }


    public enum GeoZones
    {
        SouthWest = 6

    }

    public enum DepartmentOptions
    {
        GraduateDRVCertificate = 16,
        TechnicalCertDrv,
        ProfessionalDiplomaDrv

    }

}
