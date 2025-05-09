using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbReportResponse
    {
        public Guid CorrelationId { get; set; }
        public CigResultError AvailableReportsErrorResponse { get; set; }
        public string PdfLink { get; set; }
        public string XmlLink { get; set; }
        public string FolderName { get; set; }
    }

    public class CigResultError
    {
        public string Culture { get; set; }

        public string Username { get; set; }

        public CigResultErrorReporttype Reporttype { get; set; }

        public CigResultErrorDocument Document { get; set; }

        public CigResultErrorErrmessage Errmessage { get; set; }

        public CigResultErrorSubjects Subjects { get; set; }

        public string version { get; set; }
    }

    public class CigResultErrorReporttype
    {
        public string codetype { get; set; }
    }

    public class CigResultErrorDocument
    {
        public string codetype { get; set; }

        public string type { get; set; }

        public string Value { get; set; }
    }

    public class CigResultErrorErrmessage
    {
        public int code { get; set; }

        public string Value { get; set; }
    }

    public class CigResultErrorSubjects
    {
        public CigResultErrorSubjectsCompany[] Company { get; set; }

        public CigResultErrorSubjectsIndividual[] Individual { get; set; }
    }

    public class CigResultErrorSubjectsCompany
    {
        public CigResultErrorSubjectsCompanyCreditinfoid Creditinfoid { get; set; }

        public CigResultErrorSubjectsCompanyNameNative NameNative { get; set; }

        public CigResultErrorSubjectsCompanyRegistered Registered { get; set; }

        public CigResultErrorSubjectsCompanyIdentificationDocuments IdentificationDocuments { get; set; }
    }
    public class CigResultErrorSubjectsCompanyCreditinfoid
    {
        public string title { get; set; }

        public int value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyNameNative
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyRegistered
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocuments
    {
        public CigResultErrorSubjectsCompanyIdentificationDocumentsDocument[] Document { get; set; }

        public string title { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocumentsDocument
    {
        public CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentName Name { get; set; }

        public CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentDateOfRegistration DateOfRegistration { get; set; }

        public CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentDateOfIssuance DateOfIssuance { get; set; }

        public CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentDateOfExpiration DateOfExpiration { get; set; }

        public CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentNumber Number { get; set; }

        public CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentIssuanceLocation IssuanceLocation { get; set; }

        public string rank { get; set; }

        public string type { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentName
    {
        public string id { get; set; }

        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentDateOfRegistration
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentDateOfIssuance
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentDateOfExpiration
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentNumber
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsCompanyIdentificationDocumentsDocumentIssuanceLocation
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividual
    {
        public CigResultErrorSubjectsIndividualCreditinfoid Creditinfoid { get; set; }

        public CigResultErrorSubjectsIndividualName Name { get; set; }

        public CigResultErrorSubjectsIndividualSurname Surname { get; set; }

        public CigResultErrorSubjectsIndividualFathersName FathersName { get; set; }

        public CigResultErrorSubjectsIndividualDateOfBirth DateOfBirth { get; set; }

        public CigResultErrorSubjectsIndividualIdentificationDocuments IdentificationDocuments { get; set; }
    }

    public class CigResultErrorSubjectsIndividualCreditinfoid
    {
        public string title { get; set; }

        public int value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualName
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualSurname
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualFathersName
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualDateOfBirth
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocuments
    {
        public CigResultErrorSubjectsIndividualIdentificationDocumentsDocument[] Document { get; set; }

        public string title { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocumentsDocument
    {
        public CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentName Name { get; set; }

        public CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentDateOfRegistration DateOfRegistration { get; set; }

        public CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentDateOfIssuance DateOfIssuance { get; set; }

        public CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentDateOfExpiration DateOfExpiration { get; set; }

        public CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentNumber Number { get; set; }

        public CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentIssuanceLocation IssuanceLocation { get; set; }

        public string rank { get; set; }

        public string type { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentName
    {
        public string id { get; set; }

        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentDateOfRegistration
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentDateOfIssuance
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentDateOfExpiration
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentNumber
    {
        public string title { get; set; }

        public string value { get; set; }
    }

    public class CigResultErrorSubjectsIndividualIdentificationDocumentsDocumentIssuanceLocation
    {
        public string title { get; set; }

        public string value { get; set; }
    }
}
