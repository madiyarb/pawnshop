using System;
using System.Xml.Serialization;

namespace Pawnshop.Web.Models.CreditBureau.UploadResultSCB
{

    /// <summary>
    /// обработка батча по BatchPackageId для ГКБ
    /// </summary>

    [XmlRoot(ElementName = "filesImportInfo")]
    public class UploadResultSCB
    {
        [XmlElement(ElementName = "batchFileDtoList")]
        public BatchFileDtoList BatchFileDtoList { get; set; }

        [XmlElement(ElementName = "count")]
        public int Count { get; set; }
    }

    public class BatchFileDtoList
    {
        [XmlElement(ElementName = "fileId")]
        public int FileId { get; set; }

        [XmlElement(ElementName = "fileNumber")]
        public int FileNumber { get; set; }

        [XmlElement(ElementName = "fileScheme")]
        public int FileScheme { get; set; }

        [XmlElement(ElementName = "fileName")]
        public string FileName { get; set; }

        [XmlElement(ElementName = "batchFile")]
        public string BatchFile { get; set; }

        [XmlElement(ElementName = "uploadedTime")]
        public DateTime UploadedTime { get; set; }

        [XmlElement(ElementName = "batchUploadStatus")]
        public string BatchUploadStatus { get; set; }

        [XmlElement(ElementName = "numberOfContracts")]
        public int NumberOfContracts { get; set; }

        [XmlElement(ElementName = "numberOfSubjects")]
        public int NumberOfSubjects { get; set; }

        [XmlElement(ElementName = "updatedContracts")]
        public int UpdatedContracts { get; set; }

        [XmlElement(ElementName = "updatedSubjects")]
        public int UpdatedSubjects { get; set; }

        [XmlElement(ElementName = "newContracts")]
        public int NewContracts { get; set; }

        [XmlElement(ElementName = "newSubjects")]
        public int NewSubjects { get; set; }

        [XmlElement(ElementName = "mergedContracts")]
        public int MergedContracts { get; set; }

        [XmlElement(ElementName = "skippedContracts")]
        public int SkippedContracts { get; set; }

        [XmlElement(ElementName = "skippedSubjects")]
        public int SkippedSubjects { get; set; }

        [XmlElement(ElementName = "mergedSubjects")]
        public int MergedSubjects { get; set; }

        [XmlElement(ElementName = "numberOfErrors")]
        public int NumberOfErrors { get; set; }

        [XmlElement(ElementName = "batchPackage")]
        public BatchPackage BatchPackage { get; set; }
    }

    public class BatchPackage
    {
        [XmlElement(ElementName = "packageId")]
        public int PackageId { get; set; }

        [XmlElement(ElementName = "packageName")]
        public string PackageName { get; set; }

        [XmlElement(ElementName = "addTime")]
        public DateTime AddTime { get; set; }

        [XmlElement(ElementName = "creditor")]
        public string Creditor { get; set; }

        [XmlElement(ElementName = "employee")]
        public string Employee { get; set; }
    }
}


