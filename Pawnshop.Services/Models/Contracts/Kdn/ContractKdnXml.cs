using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn.ContractKdnXml
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Root
    {

        private RootTitle titleField;

        private RootHeader headerField;

        private RootSubjectDetails subjectDetailsField;

        private RootSubjectsAddress subjectsAddressField;

        private RootIdentificationDocuments identificationDocumentsField;

        private RootClassificationOfBorrower classificationOfBorrowerField;

        private RootNegativeData negativeDataField;

        private RootSummaryInformation summaryInformationField;

        private RootExistingContracts existingContractsField;

        private RootInterconnectedSubjects interconnectedSubjectsField;

        private RootNumberOfQueries numberOfQueriesField;

        private RootRelatedCompanies relatedCompaniesField;

        private RootPublicSources publicSourcesField;

        private object semsField;

        private RootRWA170 rWA170Field;

        private RootFooter footerField;

        /// <remarks/>
        public RootTitle Title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        public RootHeader Header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }

        /// <remarks/>
        public RootSubjectDetails SubjectDetails
        {
            get
            {
                return this.subjectDetailsField;
            }
            set
            {
                this.subjectDetailsField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddress SubjectsAddress
        {
            get
            {
                return this.subjectsAddressField;
            }
            set
            {
                this.subjectsAddressField = value;
            }
        }

        /// <remarks/>
        public RootIdentificationDocuments IdentificationDocuments
        {
            get
            {
                return this.identificationDocumentsField;
            }
            set
            {
                this.identificationDocumentsField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrower ClassificationOfBorrower
        {
            get
            {
                return this.classificationOfBorrowerField;
            }
            set
            {
                this.classificationOfBorrowerField = value;
            }
        }

        /// <remarks/>
        public RootNegativeData NegativeData
        {
            get
            {
                return this.negativeDataField;
            }
            set
            {
                this.negativeDataField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformation SummaryInformation
        {
            get
            {
                return this.summaryInformationField;
            }
            set
            {
                this.summaryInformationField = value;
            }
        }

        /// <remarks/>
        public RootExistingContracts ExistingContracts
        {
            get
            {
                return this.existingContractsField;
            }
            set
            {
                this.existingContractsField = value;
            }
        }

        /// <remarks/>
        public RootInterconnectedSubjects InterconnectedSubjects
        {
            get
            {
                return this.interconnectedSubjectsField;
            }
            set
            {
                this.interconnectedSubjectsField = value;
            }
        }

        /// <remarks/>
        public RootNumberOfQueries NumberOfQueries
        {
            get
            {
                return this.numberOfQueriesField;
            }
            set
            {
                this.numberOfQueriesField = value;
            }
        }

        /// <remarks/>
        public RootRelatedCompanies RelatedCompanies
        {
            get
            {
                return this.relatedCompaniesField;
            }
            set
            {
                this.relatedCompaniesField = value;
            }
        }

        /// <remarks/>
        public RootPublicSources PublicSources
        {
            get
            {
                return this.publicSourcesField;
            }
            set
            {
                this.publicSourcesField = value;
            }
        }

        /// <remarks/>
        public object Sems
        {
            get
            {
                return this.semsField;
            }
            set
            {
                this.semsField = value;
            }
        }

        /// <remarks/>
        public RootRWA170 RWA170
        {
            get
            {
                return this.rWA170Field;
            }
            set
            {
                this.rWA170Field = value;
            }
        }

        /// <remarks/>
        public RootFooter Footer
        {
            get
            {
                return this.footerField;
            }
            set
            {
                this.footerField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootTitle
    {

        private string intitleField;

        private string nameField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string intitle
        {
            get
            {
                return this.intitleField;
            }
            set
            {
                this.intitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeader
    {

        private RootHeaderFIUniqueNumberID fIUniqueNumberIDField;

        private RootHeaderRegistrationID registrationIDField;

        private RootHeaderRNN rNNField;

        private RootHeaderSIC sICField;

        private RootHeaderIIN iINField;

        private RootHeaderDateOfBirth dateOfBirthField;

        private RootHeaderGender genderField;

        private RootHeaderSurname surnameField;

        private RootHeaderName nameField;

        private RootHeaderFathersName fathersNameField;

        private RootHeaderBirthName birthNameField;

        private RootHeaderCityOfBirth cityOfBirthField;

        private RootHeaderEducation educationField;

        private RootHeaderMatrialStatus matrialStatusField;

        private RootHeaderRegionOfBirth regionOfBirthField;

        private RootHeaderCountryOfBirth countryOfBirthField;

        private string entityTypeField;

        private string reportCodeField;

        private string intitleField;

        private string nameField1;

        private string stitleField;

        private string subjectField;

        private string titleField;

        /// <remarks/>
        public RootHeaderFIUniqueNumberID FIUniqueNumberID
        {
            get
            {
                return this.fIUniqueNumberIDField;
            }
            set
            {
                this.fIUniqueNumberIDField = value;
            }
        }

        /// <remarks/>
        public RootHeaderRegistrationID RegistrationID
        {
            get
            {
                return this.registrationIDField;
            }
            set
            {
                this.registrationIDField = value;
            }
        }

        /// <remarks/>
        public RootHeaderRNN RNN
        {
            get
            {
                return this.rNNField;
            }
            set
            {
                this.rNNField = value;
            }
        }

        /// <remarks/>
        public RootHeaderSIC SIC
        {
            get
            {
                return this.sICField;
            }
            set
            {
                this.sICField = value;
            }
        }

        /// <remarks/>
        public RootHeaderIIN IIN
        {
            get
            {
                return this.iINField;
            }
            set
            {
                this.iINField = value;
            }
        }

        /// <remarks/>
        public RootHeaderDateOfBirth DateOfBirth
        {
            get
            {
                return this.dateOfBirthField;
            }
            set
            {
                this.dateOfBirthField = value;
            }
        }

        /// <remarks/>
        public RootHeaderGender Gender
        {
            get
            {
                return this.genderField;
            }
            set
            {
                this.genderField = value;
            }
        }

        /// <remarks/>
        public RootHeaderSurname Surname
        {
            get
            {
                return this.surnameField;
            }
            set
            {
                this.surnameField = value;
            }
        }

        /// <remarks/>
        public RootHeaderName Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public RootHeaderFathersName FathersName
        {
            get
            {
                return this.fathersNameField;
            }
            set
            {
                this.fathersNameField = value;
            }
        }

        /// <remarks/>
        public RootHeaderBirthName BirthName
        {
            get
            {
                return this.birthNameField;
            }
            set
            {
                this.birthNameField = value;
            }
        }

        /// <remarks/>
        public RootHeaderCityOfBirth CityOfBirth
        {
            get
            {
                return this.cityOfBirthField;
            }
            set
            {
                this.cityOfBirthField = value;
            }
        }

        /// <remarks/>
        public RootHeaderEducation Education
        {
            get
            {
                return this.educationField;
            }
            set
            {
                this.educationField = value;
            }
        }

        /// <remarks/>
        public RootHeaderMatrialStatus MatrialStatus
        {
            get
            {
                return this.matrialStatusField;
            }
            set
            {
                this.matrialStatusField = value;
            }
        }

        /// <remarks/>
        public RootHeaderRegionOfBirth RegionOfBirth
        {
            get
            {
                return this.regionOfBirthField;
            }
            set
            {
                this.regionOfBirthField = value;
            }
        }

        /// <remarks/>
        public RootHeaderCountryOfBirth CountryOfBirth
        {
            get
            {
                return this.countryOfBirthField;
            }
            set
            {
                this.countryOfBirthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EntityType
        {
            get
            {
                return this.entityTypeField;
            }
            set
            {
                this.entityTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ReportCode
        {
            get
            {
                return this.reportCodeField;
            }
            set
            {
                this.reportCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string intitle
        {
            get
            {
                return this.intitleField;
            }
            set
            {
                this.intitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField1;
            }
            set
            {
                this.nameField1 = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subject
        {
            get
            {
                return this.subjectField;
            }
            set
            {
                this.subjectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderFIUniqueNumberID
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderRegistrationID
    {

        private string stitleField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderRNN
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderSIC
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderIIN
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderDateOfBirth
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderGender
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderSurname
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderName
    {

        private string stitleField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderFathersName
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderBirthName
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderCityOfBirth
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderEducation
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderMatrialStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderRegionOfBirth
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootHeaderCountryOfBirth
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetails
    {

        private object[] itemsField;

        private string nameField;

        private string stitleField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CellularPhone", typeof(RootSubjectDetailsCellularPhone))]
        [System.Xml.Serialization.XmlElementAttribute("City", typeof(RootSubjectDetailsCity))]
        [System.Xml.Serialization.XmlElementAttribute("Country", typeof(RootSubjectDetailsCountry))]
        [System.Xml.Serialization.XmlElementAttribute("Email", typeof(RootSubjectDetailsEmail))]
        [System.Xml.Serialization.XmlElementAttribute("EmployeesSalary", typeof(RootSubjectDetailsEmployeesSalary))]
        [System.Xml.Serialization.XmlElementAttribute("Fax", typeof(RootSubjectDetailsFax))]
        [System.Xml.Serialization.XmlElementAttribute("HomePhone", typeof(RootSubjectDetailsHomePhone))]
        [System.Xml.Serialization.XmlElementAttribute("Number", typeof(RootSubjectDetailsNumber))]
        [System.Xml.Serialization.XmlElementAttribute("NumberOfChildern", typeof(RootSubjectDetailsNumberOfChildern))]
        [System.Xml.Serialization.XmlElementAttribute("NumberOfDependents", typeof(RootSubjectDetailsNumberOfDependents))]
        [System.Xml.Serialization.XmlElementAttribute("OfficePhone", typeof(RootSubjectDetailsOfficePhone))]
        [System.Xml.Serialization.XmlElementAttribute("Region", typeof(RootSubjectDetailsRegion))]
        [System.Xml.Serialization.XmlElementAttribute("Street", typeof(RootSubjectDetailsStreet))]
        [System.Xml.Serialization.XmlElementAttribute("StreetAddress", typeof(RootSubjectDetailsStreetAddress))]
        [System.Xml.Serialization.XmlElementAttribute("ZipCode", typeof(RootSubjectDetailsZipCode))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsCellularPhone
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsCity
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsCountry
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsEmail
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsEmployeesSalary
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsFax
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsHomePhone
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsNumber
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsNumberOfChildern
    {

        private string stitleField;

        private string titleField;

        private string valueField;

        private bool valueFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valueSpecified
        {
            get
            {
                return this.valueFieldSpecified;
            }
            set
            {
                this.valueFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsNumberOfDependents
    {

        private string titleField;

        private string valueField;

        private bool valueFieldSpecified;

        private string stitle2Field;

        private string stitle3Field;

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valueSpecified
        {
            get
            {
                return this.valueFieldSpecified;
            }
            set
            {
                this.valueFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle2
        {
            get
            {
                return this.stitle2Field;
            }
            set
            {
                this.stitle2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle3
        {
            get
            {
                return this.stitle3Field;
            }
            set
            {
                this.stitle3Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsOfficePhone
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsRegion
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsStreet
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsStreetAddress
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectDetailsZipCode
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddress
    {

        private RootSubjectsAddressAddress[] addressField;

        private string stitleField;

        private string stitle2Field;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Address")]
        public RootSubjectsAddressAddress[] Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle2
        {
            get
            {
                return this.stitle2Field;
            }
            set
            {
                this.stitle2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddress
    {

        private RootSubjectsAddressAddressAddressType addressTypeField;

        private RootSubjectsAddressAddressStreet streetField;

        private RootSubjectsAddressAddressNumber numberField;

        private RootSubjectsAddressAddressZipCode zipCodeField;

        private RootSubjectsAddressAddressCity cityField;

        private RootSubjectsAddressAddressCountry countryField;

        private RootSubjectsAddressAddressRegion regionField;

        private RootSubjectsAddressAddressHomePhone homePhoneField;

        private RootSubjectsAddressAddressOfficePhone officePhoneField;

        private RootSubjectsAddressAddressFax faxField;

        private RootSubjectsAddressAddressCellularPhone cellularPhoneField;

        private RootSubjectsAddressAddressEmailAddress emailAddressField;

        private RootSubjectsAddressAddressWebPageAddress webPageAddressField;

        private RootSubjectsAddressAddressAddressInserted addressInsertedField;

        private RootSubjectsAddressAddressPostBox postBoxField;

        private RootSubjectsAddressAddressAdditionalInfo additionalInfoField;

        private string titleField;

        private string typeField;

        /// <remarks/>
        public RootSubjectsAddressAddressAddressType AddressType
        {
            get
            {
                return this.addressTypeField;
            }
            set
            {
                this.addressTypeField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressStreet Street
        {
            get
            {
                return this.streetField;
            }
            set
            {
                this.streetField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressNumber Number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressZipCode ZipCode
        {
            get
            {
                return this.zipCodeField;
            }
            set
            {
                this.zipCodeField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressCity City
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressCountry Country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressRegion Region
        {
            get
            {
                return this.regionField;
            }
            set
            {
                this.regionField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressHomePhone HomePhone
        {
            get
            {
                return this.homePhoneField;
            }
            set
            {
                this.homePhoneField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressOfficePhone OfficePhone
        {
            get
            {
                return this.officePhoneField;
            }
            set
            {
                this.officePhoneField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressFax Fax
        {
            get
            {
                return this.faxField;
            }
            set
            {
                this.faxField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressCellularPhone CellularPhone
        {
            get
            {
                return this.cellularPhoneField;
            }
            set
            {
                this.cellularPhoneField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressEmailAddress EmailAddress
        {
            get
            {
                return this.emailAddressField;
            }
            set
            {
                this.emailAddressField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressWebPageAddress WebPageAddress
        {
            get
            {
                return this.webPageAddressField;
            }
            set
            {
                this.webPageAddressField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressAddressInserted AddressInserted
        {
            get
            {
                return this.addressInsertedField;
            }
            set
            {
                this.addressInsertedField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressPostBox PostBox
        {
            get
            {
                return this.postBoxField;
            }
            set
            {
                this.postBoxField = value;
            }
        }

        /// <remarks/>
        public RootSubjectsAddressAddressAdditionalInfo AdditionalInfo
        {
            get
            {
                return this.additionalInfoField;
            }
            set
            {
                this.additionalInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressAddressType
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressStreet
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressNumber
    {

        private string stitleField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressZipCode
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressCity
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressCountry
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressRegion
    {

        private string stitleField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressHomePhone
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressOfficePhone
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressFax
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressCellularPhone
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressEmailAddress
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressWebPageAddress
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressAddressInserted
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressPostBox
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSubjectsAddressAddressAdditionalInfo
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocuments
    {

        private RootIdentificationDocumentsDocument[] documentField;

        private string stitleField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Document")]
        public RootIdentificationDocumentsDocument[] Document
        {
            get
            {
                return this.documentField;
            }
            set
            {
                this.documentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocument
    {

        private RootIdentificationDocumentsDocumentName nameField;

        private RootIdentificationDocumentsDocumentDateOfRegistration dateOfRegistrationField;

        private RootIdentificationDocumentsDocumentDateOfIssuance dateOfIssuanceField;

        private RootIdentificationDocumentsDocumentDateOfExpiration dateOfExpirationField;

        private RootIdentificationDocumentsDocumentNumber numberField;

        private RootIdentificationDocumentsDocumentIssuanceLocation issuanceLocationField;

        private RootIdentificationDocumentsDocumentDateOfInserted dateOfInsertedField;

        private string rankField;

        private string stitleField;

        private string typeField;

        /// <remarks/>
        public RootIdentificationDocumentsDocumentName Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public RootIdentificationDocumentsDocumentDateOfRegistration DateOfRegistration
        {
            get
            {
                return this.dateOfRegistrationField;
            }
            set
            {
                this.dateOfRegistrationField = value;
            }
        }

        /// <remarks/>
        public RootIdentificationDocumentsDocumentDateOfIssuance DateOfIssuance
        {
            get
            {
                return this.dateOfIssuanceField;
            }
            set
            {
                this.dateOfIssuanceField = value;
            }
        }

        /// <remarks/>
        public RootIdentificationDocumentsDocumentDateOfExpiration DateOfExpiration
        {
            get
            {
                return this.dateOfExpirationField;
            }
            set
            {
                this.dateOfExpirationField = value;
            }
        }

        /// <remarks/>
        public RootIdentificationDocumentsDocumentNumber Number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        public RootIdentificationDocumentsDocumentIssuanceLocation IssuanceLocation
        {
            get
            {
                return this.issuanceLocationField;
            }
            set
            {
                this.issuanceLocationField = value;
            }
        }

        /// <remarks/>
        public RootIdentificationDocumentsDocumentDateOfInserted DateOfInserted
        {
            get
            {
                return this.dateOfInsertedField;
            }
            set
            {
                this.dateOfInsertedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string rank
        {
            get
            {
                return this.rankField;
            }
            set
            {
                this.rankField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocumentName
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocumentDateOfRegistration
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocumentDateOfIssuance
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocumentDateOfExpiration
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocumentNumber
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocumentIssuanceLocation
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootIdentificationDocumentsDocumentDateOfInserted
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrower
    {

        private RootClassificationOfBorrowerBorrowerClassification borrowerClassificationField;

        private RootClassificationOfBorrowerPatent patentField;

        private RootClassificationOfBorrowerResident residentField;

        private RootClassificationOfBorrowerSubjectsPosition subjectsPositionField;

        private RootClassificationOfBorrowerCitizenship citizenshipField;

        private RootClassificationOfBorrowerSubjectsEmployment subjectsEmploymentField;

        private RootClassificationOfBorrowerForeignersCitizenship foreignersCitizenshipField;

        private RootClassificationOfBorrowerEconomicActivityGroup economicActivityGroupField;

        private string nameField;

        private string titleField;

        /// <remarks/>
        public RootClassificationOfBorrowerBorrowerClassification BorrowerClassification
        {
            get
            {
                return this.borrowerClassificationField;
            }
            set
            {
                this.borrowerClassificationField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrowerPatent Patent
        {
            get
            {
                return this.patentField;
            }
            set
            {
                this.patentField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrowerResident Resident
        {
            get
            {
                return this.residentField;
            }
            set
            {
                this.residentField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrowerSubjectsPosition SubjectsPosition
        {
            get
            {
                return this.subjectsPositionField;
            }
            set
            {
                this.subjectsPositionField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrowerCitizenship Citizenship
        {
            get
            {
                return this.citizenshipField;
            }
            set
            {
                this.citizenshipField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrowerSubjectsEmployment SubjectsEmployment
        {
            get
            {
                return this.subjectsEmploymentField;
            }
            set
            {
                this.subjectsEmploymentField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrowerForeignersCitizenship ForeignersCitizenship
        {
            get
            {
                return this.foreignersCitizenshipField;
            }
            set
            {
                this.foreignersCitizenshipField = value;
            }
        }

        /// <remarks/>
        public RootClassificationOfBorrowerEconomicActivityGroup EconomicActivityGroup
        {
            get
            {
                return this.economicActivityGroupField;
            }
            set
            {
                this.economicActivityGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerBorrowerClassification
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerPatent
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerResident
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerSubjectsPosition
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerCitizenship
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerSubjectsEmployment
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerForeignersCitizenship
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootClassificationOfBorrowerEconomicActivityGroup
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeData
    {

        private RootNegativeDataNegativeStatus[] negativeStatusField;

        private string nameField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NegativeStatus")]
        public RootNegativeDataNegativeStatus[] NegativeStatus
        {
            get
            {
                return this.negativeStatusField;
            }
            set
            {
                this.negativeStatusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatus
    {

        private RootNegativeDataNegativeStatusNegativeStatusOfClient negativeStatusOfClientField;

        private RootNegativeDataNegativeStatusNegativeStatusOfContract[] negativeStatusOfContractField;

        private string titleField;

        private string typeField;

        private string typeTitleField;

        /// <remarks/>
        public RootNegativeDataNegativeStatusNegativeStatusOfClient NegativeStatusOfClient
        {
            get
            {
                return this.negativeStatusOfClientField;
            }
            set
            {
                this.negativeStatusOfClientField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NegativeStatusOfContract")]
        public RootNegativeDataNegativeStatusNegativeStatusOfContract[] NegativeStatusOfContract
        {
            get
            {
                return this.negativeStatusOfContractField;
            }
            set
            {
                this.negativeStatusOfContractField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string typeTitle
        {
            get
            {
                return this.typeTitleField;
            }
            set
            {
                this.typeTitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfClient
    {

        private RootNegativeDataNegativeStatusNegativeStatusOfClientNegativeStatusOfClient negativeStatusOfClientField;

        private RootNegativeDataNegativeStatusNegativeStatusOfClientRegistrationDate registrationDateField;

        /// <remarks/>
        public RootNegativeDataNegativeStatusNegativeStatusOfClientNegativeStatusOfClient NegativeStatusOfClient
        {
            get
            {
                return this.negativeStatusOfClientField;
            }
            set
            {
                this.negativeStatusOfClientField = value;
            }
        }

        /// <remarks/>
        public RootNegativeDataNegativeStatusNegativeStatusOfClientRegistrationDate RegistrationDate
        {
            get
            {
                return this.registrationDateField;
            }
            set
            {
                this.registrationDateField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfClientNegativeStatusOfClient
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfClientRegistrationDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfContract
    {

        private RootNegativeDataNegativeStatusNegativeStatusOfContractNegativeStatusOfContract negativeStatusOfContractField;

        private RootNegativeDataNegativeStatusNegativeStatusOfContractRegistrationDate registrationDateField;

        private RootNegativeDataNegativeStatusNegativeStatusOfContractSubjectRole subjectRoleField;

        private RootNegativeDataNegativeStatusNegativeStatusOfContractSubjectRoleEn subjectRoleEnField;

        /// <remarks/>
        public RootNegativeDataNegativeStatusNegativeStatusOfContractNegativeStatusOfContract NegativeStatusOfContract
        {
            get
            {
                return this.negativeStatusOfContractField;
            }
            set
            {
                this.negativeStatusOfContractField = value;
            }
        }

        /// <remarks/>
        public RootNegativeDataNegativeStatusNegativeStatusOfContractRegistrationDate RegistrationDate
        {
            get
            {
                return this.registrationDateField;
            }
            set
            {
                this.registrationDateField = value;
            }
        }

        /// <remarks/>
        public RootNegativeDataNegativeStatusNegativeStatusOfContractSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        public RootNegativeDataNegativeStatusNegativeStatusOfContractSubjectRoleEn SubjectRoleEn
        {
            get
            {
                return this.subjectRoleEnField;
            }
            set
            {
                this.subjectRoleEnField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfContractNegativeStatusOfContract
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfContractRegistrationDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfContractSubjectRole
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNegativeDataNegativeStatusNegativeStatusOfContractSubjectRoleEn
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformation
    {

        private RootSummaryInformationExistingContracts existingContractsField;

        private RootSummaryInformationTerminatedContracts terminatedContractsField;

        private RootSummaryInformationTerminatedContractsDateLimit terminatedContractsDateLimitField;

        private RootSummaryInformationWithdrawnApplications withdrawnApplicationsField;

        private RootSummaryInformationNumberOfApplications numberOfApplicationsField;

        private RootSummaryInformationNumberOfInquiries numberOfInquiriesField;

        private string titleField;

        /// <remarks/>
        public RootSummaryInformationExistingContracts ExistingContracts
        {
            get
            {
                return this.existingContractsField;
            }
            set
            {
                this.existingContractsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContracts TerminatedContracts
        {
            get
            {
                return this.terminatedContractsField;
            }
            set
            {
                this.terminatedContractsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimit TerminatedContractsDateLimit
        {
            get
            {
                return this.terminatedContractsDateLimitField;
            }
            set
            {
                this.terminatedContractsDateLimitField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplications WithdrawnApplications
        {
            get
            {
                return this.withdrawnApplicationsField;
            }
            set
            {
                this.withdrawnApplicationsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfApplications NumberOfApplications
        {
            get
            {
                return this.numberOfApplicationsField;
            }
            set
            {
                this.numberOfApplicationsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfInquiries NumberOfInquiries
        {
            get
            {
                return this.numberOfInquiriesField;
            }
            set
            {
                this.numberOfInquiriesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContracts
    {

        private RootSummaryInformationExistingContractsSubjectRole subjectRoleField;

        private string titleField;

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRole
    {

        private RootSummaryInformationExistingContractsSubjectRoleNumberOfContracts numberOfContractsField;

        private RootSummaryInformationExistingContractsSubjectRoleNumberOfCreditLines numberOfCreditLinesField;

        private RootSummaryInformationExistingContractsSubjectRoleTotalOutstandingDebt totalOutstandingDebtField;

        private RootSummaryInformationExistingContractsSubjectRoleTotalDebtOverdue totalDebtOverdueField;

        private RootSummaryInformationExistingContractsSubjectRoleTotalFine totalFineField;

        private RootSummaryInformationExistingContractsSubjectRoleTotalPenalty totalPenaltyField;

        private RootSummaryInformationExistingContractsSubjectRoleContractStatuses contractStatusesField;

        private string titleField;

        private string typeField;

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleNumberOfContracts NumberOfContracts
        {
            get
            {
                return this.numberOfContractsField;
            }
            set
            {
                this.numberOfContractsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleNumberOfCreditLines NumberOfCreditLines
        {
            get
            {
                return this.numberOfCreditLinesField;
            }
            set
            {
                this.numberOfCreditLinesField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleTotalOutstandingDebt TotalOutstandingDebt
        {
            get
            {
                return this.totalOutstandingDebtField;
            }
            set
            {
                this.totalOutstandingDebtField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleTotalDebtOverdue TotalDebtOverdue
        {
            get
            {
                return this.totalDebtOverdueField;
            }
            set
            {
                this.totalDebtOverdueField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleTotalFine TotalFine
        {
            get
            {
                return this.totalFineField;
            }
            set
            {
                this.totalFineField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleTotalPenalty TotalPenalty
        {
            get
            {
                return this.totalPenaltyField;
            }
            set
            {
                this.totalPenaltyField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleContractStatuses ContractStatuses
        {
            get
            {
                return this.contractStatusesField;
            }
            set
            {
                this.contractStatusesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleNumberOfContracts
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleNumberOfCreditLines
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleTotalOutstandingDebt
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleTotalDebtOverdue
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleTotalFine
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleTotalPenalty
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleContractStatuses
    {

        private RootSummaryInformationExistingContractsSubjectRoleContractStatusesContractStatus contractStatusField;

        /// <remarks/>
        public RootSummaryInformationExistingContractsSubjectRoleContractStatusesContractStatus ContractStatus
        {
            get
            {
                return this.contractStatusField;
            }
            set
            {
                this.contractStatusField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationExistingContractsSubjectRoleContractStatusesContractStatus
    {

        private string countField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string count
        {
            get
            {
                return this.countField;
            }
            set
            {
                this.countField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContracts
    {

        private RootSummaryInformationTerminatedContractsSubjectRole subjectRoleField;

        private string titleField;

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRole
    {

        private RootSummaryInformationTerminatedContractsSubjectRoleNumberOfContracts numberOfContractsField;

        private RootSummaryInformationTerminatedContractsSubjectRoleNumberOfCreditLines numberOfCreditLinesField;

        private RootSummaryInformationTerminatedContractsSubjectRoleTotalOutstandingDebt totalOutstandingDebtField;

        private RootSummaryInformationTerminatedContractsSubjectRoleTotalDebtOverdue totalDebtOverdueField;

        private RootSummaryInformationTerminatedContractsSubjectRoleTotalFine totalFineField;

        private RootSummaryInformationTerminatedContractsSubjectRoleTotalPenalty totalPenaltyField;

        private RootSummaryInformationTerminatedContractsSubjectRoleContractStatus[] contractStatusesField;

        private string titleField;

        private string typeField;

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsSubjectRoleNumberOfContracts NumberOfContracts
        {
            get
            {
                return this.numberOfContractsField;
            }
            set
            {
                this.numberOfContractsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsSubjectRoleNumberOfCreditLines NumberOfCreditLines
        {
            get
            {
                return this.numberOfCreditLinesField;
            }
            set
            {
                this.numberOfCreditLinesField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsSubjectRoleTotalOutstandingDebt TotalOutstandingDebt
        {
            get
            {
                return this.totalOutstandingDebtField;
            }
            set
            {
                this.totalOutstandingDebtField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsSubjectRoleTotalDebtOverdue TotalDebtOverdue
        {
            get
            {
                return this.totalDebtOverdueField;
            }
            set
            {
                this.totalDebtOverdueField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsSubjectRoleTotalFine TotalFine
        {
            get
            {
                return this.totalFineField;
            }
            set
            {
                this.totalFineField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsSubjectRoleTotalPenalty TotalPenalty
        {
            get
            {
                return this.totalPenaltyField;
            }
            set
            {
                this.totalPenaltyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ContractStatus", IsNullable = false)]
        public RootSummaryInformationTerminatedContractsSubjectRoleContractStatus[] ContractStatuses
        {
            get
            {
                return this.contractStatusesField;
            }
            set
            {
                this.contractStatusesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRoleNumberOfContracts
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRoleNumberOfCreditLines
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRoleTotalOutstandingDebt
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRoleTotalDebtOverdue
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRoleTotalFine
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRoleTotalPenalty
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsSubjectRoleContractStatus
    {

        private string countField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string count
        {
            get
            {
                return this.countField;
            }
            set
            {
                this.countField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimit
    {

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRole subjectRoleField;

        private string titleField;

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRole
    {

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleNumberOfContracts numberOfContractsField;

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleNumberOfCreditLines numberOfCreditLinesField;

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalOutstandingDebt totalOutstandingDebtField;

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalDebtOverdue totalDebtOverdueField;

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalFine totalFineField;

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalPenalty totalPenaltyField;

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleContractStatuses contractStatusesField;

        private string titleField;

        private string typeField;

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleNumberOfContracts NumberOfContracts
        {
            get
            {
                return this.numberOfContractsField;
            }
            set
            {
                this.numberOfContractsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleNumberOfCreditLines NumberOfCreditLines
        {
            get
            {
                return this.numberOfCreditLinesField;
            }
            set
            {
                this.numberOfCreditLinesField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalOutstandingDebt TotalOutstandingDebt
        {
            get
            {
                return this.totalOutstandingDebtField;
            }
            set
            {
                this.totalOutstandingDebtField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalDebtOverdue TotalDebtOverdue
        {
            get
            {
                return this.totalDebtOverdueField;
            }
            set
            {
                this.totalDebtOverdueField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalFine TotalFine
        {
            get
            {
                return this.totalFineField;
            }
            set
            {
                this.totalFineField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalPenalty TotalPenalty
        {
            get
            {
                return this.totalPenaltyField;
            }
            set
            {
                this.totalPenaltyField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleContractStatuses ContractStatuses
        {
            get
            {
                return this.contractStatusesField;
            }
            set
            {
                this.contractStatusesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleNumberOfContracts
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleNumberOfCreditLines
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalOutstandingDebt
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalDebtOverdue
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalFine
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleTotalPenalty
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleContractStatuses
    {

        private RootSummaryInformationTerminatedContractsDateLimitSubjectRoleContractStatusesContractStatus contractStatusField;

        /// <remarks/>
        public RootSummaryInformationTerminatedContractsDateLimitSubjectRoleContractStatusesContractStatus ContractStatus
        {
            get
            {
                return this.contractStatusField;
            }
            set
            {
                this.contractStatusField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationTerminatedContractsDateLimitSubjectRoleContractStatusesContractStatus
    {

        private string countField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string count
        {
            get
            {
                return this.countField;
            }
            set
            {
                this.countField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplications
    {

        private RootSummaryInformationWithdrawnApplicationsSubjectRole subjectRoleField;

        private string titleField;

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRole
    {

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleNumberOfContracts numberOfContractsField;

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleNumberOfCreditLines numberOfCreditLinesField;

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalOutstandingDebt totalOutstandingDebtField;

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalDebtOverdue totalDebtOverdueField;

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalFine totalFineField;

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalPenalty totalPenaltyField;

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleContractStatuses contractStatusesField;

        private string titleField;

        private string typeField;

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleNumberOfContracts NumberOfContracts
        {
            get
            {
                return this.numberOfContractsField;
            }
            set
            {
                this.numberOfContractsField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleNumberOfCreditLines NumberOfCreditLines
        {
            get
            {
                return this.numberOfCreditLinesField;
            }
            set
            {
                this.numberOfCreditLinesField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalOutstandingDebt TotalOutstandingDebt
        {
            get
            {
                return this.totalOutstandingDebtField;
            }
            set
            {
                this.totalOutstandingDebtField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalDebtOverdue TotalDebtOverdue
        {
            get
            {
                return this.totalDebtOverdueField;
            }
            set
            {
                this.totalDebtOverdueField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalFine TotalFine
        {
            get
            {
                return this.totalFineField;
            }
            set
            {
                this.totalFineField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalPenalty TotalPenalty
        {
            get
            {
                return this.totalPenaltyField;
            }
            set
            {
                this.totalPenaltyField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleContractStatuses ContractStatuses
        {
            get
            {
                return this.contractStatusesField;
            }
            set
            {
                this.contractStatusesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleNumberOfContracts
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleNumberOfCreditLines
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalOutstandingDebt
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalDebtOverdue
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalFine
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleTotalPenalty
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleContractStatuses
    {

        private RootSummaryInformationWithdrawnApplicationsSubjectRoleContractStatusesContractStatus contractStatusField;

        /// <remarks/>
        public RootSummaryInformationWithdrawnApplicationsSubjectRoleContractStatusesContractStatus ContractStatus
        {
            get
            {
                return this.contractStatusField;
            }
            set
            {
                this.contractStatusField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationWithdrawnApplicationsSubjectRoleContractStatusesContractStatus
    {

        private string countField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string count
        {
            get
            {
                return this.countField;
            }
            set
            {
                this.countField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfApplications
    {

        private RootSummaryInformationNumberOfApplicationsApplicationType applicationTypeField;

        private string titleField;

        /// <remarks/>
        public RootSummaryInformationNumberOfApplicationsApplicationType ApplicationType
        {
            get
            {
                return this.applicationTypeField;
            }
            set
            {
                this.applicationTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfApplicationsApplicationType
    {

        private RootSummaryInformationNumberOfApplicationsApplicationTypeWeek weekField;

        private RootSummaryInformationNumberOfApplicationsApplicationTypeMonth monthField;

        private RootSummaryInformationNumberOfApplicationsApplicationTypeQuarter quarterField;

        private RootSummaryInformationNumberOfApplicationsApplicationTypeYear yearField;

        private RootSummaryInformationNumberOfApplicationsApplicationTypeLastThreeYear lastThreeYearField;

        private string titleField;

        private string typeField;

        private string valueField;

        /// <remarks/>
        public RootSummaryInformationNumberOfApplicationsApplicationTypeWeek Week
        {
            get
            {
                return this.weekField;
            }
            set
            {
                this.weekField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfApplicationsApplicationTypeMonth Month
        {
            get
            {
                return this.monthField;
            }
            set
            {
                this.monthField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfApplicationsApplicationTypeQuarter Quarter
        {
            get
            {
                return this.quarterField;
            }
            set
            {
                this.quarterField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfApplicationsApplicationTypeYear Year
        {
            get
            {
                return this.yearField;
            }
            set
            {
                this.yearField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfApplicationsApplicationTypeLastThreeYear LastThreeYear
        {
            get
            {
                return this.lastThreeYearField;
            }
            set
            {
                this.lastThreeYearField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfApplicationsApplicationTypeWeek
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfApplicationsApplicationTypeMonth
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfApplicationsApplicationTypeQuarter
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfApplicationsApplicationTypeYear
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfApplicationsApplicationTypeLastThreeYear
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfInquiries
    {

        private RootSummaryInformationNumberOfInquiriesFirstQuarter firstQuarterField;

        private RootSummaryInformationNumberOfInquiriesSecondQuarter secondQuarterField;

        private RootSummaryInformationNumberOfInquiriesThirdQuarter thirdQuarterField;

        private RootSummaryInformationNumberOfInquiriesFourthQuarter fourthQuarterField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        public RootSummaryInformationNumberOfInquiriesFirstQuarter FirstQuarter
        {
            get
            {
                return this.firstQuarterField;
            }
            set
            {
                this.firstQuarterField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfInquiriesSecondQuarter SecondQuarter
        {
            get
            {
                return this.secondQuarterField;
            }
            set
            {
                this.secondQuarterField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfInquiriesThirdQuarter ThirdQuarter
        {
            get
            {
                return this.thirdQuarterField;
            }
            set
            {
                this.thirdQuarterField = value;
            }
        }

        /// <remarks/>
        public RootSummaryInformationNumberOfInquiriesFourthQuarter FourthQuarter
        {
            get
            {
                return this.fourthQuarterField;
            }
            set
            {
                this.fourthQuarterField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfInquiriesFirstQuarter
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfInquiriesSecondQuarter
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfInquiriesThirdQuarter
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootSummaryInformationNumberOfInquiriesFourthQuarter
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContracts
    {

        private RootExistingContractsContract[] contractField;

        private string nameField;

        private string stitleField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Contract")]
        public RootExistingContractsContract[] Contract
        {
            get
            {
                return this.contractField;
            }
            set
            {
                this.contractField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContract
    {

        private RootExistingContractsContractCodeOfContract codeOfContractField;

        private RootExistingContractsContractAgreementNumber agreementNumberField;

        private RootExistingContractsContractAgreementNumberGuarantee agreementNumberGuaranteeField;

        private RootExistingContractsContractTypeOfFounding typeOfFoundingField;

        private RootExistingContractsContractPurposeOfCredit purposeOfCreditField;

        private RootExistingContractsContractCurrencyCode currencyCodeField;

        private RootExistingContractsContractContractStatus contractStatusField;

        private RootExistingContractsContractDateOfApplication dateOfApplicationField;

        private RootExistingContractsContractDateOfCreditStart dateOfCreditStartField;

        private RootExistingContractsContractDateOfCreditEnd dateOfCreditEndField;

        private RootExistingContractsContractDateOfRealRepayment dateOfRealRepaymentField;

        private RootExistingContractsContractDateAgreementGuarantee dateAgreementGuaranteeField;

        private RootExistingContractsContractGuaranteeEvent guaranteeEventField;

        private RootExistingContractsContractLastUpdate lastUpdateField;

        private RootExistingContractsContractCollateral collateralField;

        private RootExistingContractsContractClassificationOfContract classificationOfContractField;

        private RootExistingContractsContractTotalAmount totalAmountField;

        private RootExistingContractsContractAmount amountField;

        private RootExistingContractsContractCreditUsage creditUsageField;

        private RootExistingContractsContractResidualAmount residualAmountField;

        private RootExistingContractsContractCreditLimit creditLimitField;

        private RootExistingContractsContractNumberOfOutstandingInstalments numberOfOutstandingInstalmentsField;

        private RootExistingContractsContractNumberOfInstalments numberOfInstalmentsField;

        private RootExistingContractsContractOutstandingAmount outstandingAmountField;

        private RootExistingContractsContractPeriodicityOfPayments periodicityOfPaymentsField;

        private RootExistingContractsContractMethodOfPayments methodOfPaymentsField;

        private RootExistingContractsContractNumberOfOverdueInstalments numberOfOverdueInstalmentsField;

        private RootExistingContractsContractOverdueAmount overdueAmountField;

        private RootExistingContractsContractMonthlyInstalmentAmount monthlyInstalmentAmountField;

        private RootExistingContractsContractInterestRate interestRateField;

        private RootExistingContractsContractSubjectRole subjectRoleField;

        private RootExistingContractsContractFinancialInstitution financialInstitutionField;

        private RootExistingContractsContractSpecialRelationship specialRelationshipField;

        private RootExistingContractsContractAnnualEffectiveRate annualEffectiveRateField;

        private RootExistingContractsContractNominalRate nominalRateField;

        private RootExistingContractsContractAmountProvisions amountProvisionsField;

        private RootExistingContractsContractLoanAccount loanAccountField;

        private RootExistingContractsContractGracePrincipal gracePrincipalField;

        private RootExistingContractsContractGracePay gracePayField;

        private RootExistingContractsContractPlaceOfDisbursement placeOfDisbursementField;

        private RootExistingContractsContractYear[] paymentsCalendarField;

        private RootExistingContractsContractContractThirdParty contractThirdPartyField;

        private RootExistingContractsContractParentContractCode parentContractCodeField;

        private RootExistingContractsContractParentProvider parentProviderField;

        private RootExistingContractsContractParentContractStatus parentContractStatusField;

        private RootExistingContractsContractParentOperationDate parentOperationDateField;

        private RootExistingContractsContractFine fineField;

        private RootExistingContractsContractPenalty penaltyField;

        private RootExistingContractsContractBranchLocation branchLocationField;

        private RootExistingContractsContractProstringationCount prostringationCountField;

        private RootExistingContractsContractNumberOfTransactions numberOfTransactionsField;

        private RootExistingContractsContractInstalmentAmount instalmentAmountField;

        private RootExistingContractsContractActualDate actualDateField;

        private RootExistingContractsContractDateOfInserted dateOfInsertedField;

        private RootExistingContractsContractNumberOfOverdueInstalmentsMax numberOfOverdueInstalmentsMaxField;

        private RootExistingContractsContractNumberOfOverdueInstalmentsMaxDate numberOfOverdueInstalmentsMaxDateField;

        private RootExistingContractsContractNumberOfOverdueInstalmentsMaxAmount numberOfOverdueInstalmentsMaxAmountField;

        private RootExistingContractsContractOverdueAmountMax overdueAmountMaxField;

        private RootExistingContractsContractOverdueAmountMaxDate overdueAmountMaxDateField;

        private RootExistingContractsContractOverdueAmountMaxCount overdueAmountMaxCountField;

        private RootExistingContractsContractCreditObject creditObjectField;

        private RootExistingContractsContractInterconnectedSubjects interconnectedSubjectsField;

        private RootExistingContractsContractCreditLineParentCode creditLineParentCodeField;

        private RootExistingContractsContractFundingSource fundingSourceField;

        private RootExistingContractsContractCreditLineRelations creditLineRelationsField;

        private RootExistingContractsContractAvailableDate availableDateField;

        private RootExistingContractsContractAvailableLimit availableLimitField;

        private RootExistingContractsContractSchema schemaField;

        private string contractTypeField;

        private string contractTypeCodeField;

        private string currencyField;

        /// <remarks/>
        public RootExistingContractsContractCodeOfContract CodeOfContract
        {
            get
            {
                return this.codeOfContractField;
            }
            set
            {
                this.codeOfContractField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractAgreementNumber AgreementNumber
        {
            get
            {
                return this.agreementNumberField;
            }
            set
            {
                this.agreementNumberField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractAgreementNumberGuarantee AgreementNumberGuarantee
        {
            get
            {
                return this.agreementNumberGuaranteeField;
            }
            set
            {
                this.agreementNumberGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractTypeOfFounding TypeOfFounding
        {
            get
            {
                return this.typeOfFoundingField;
            }
            set
            {
                this.typeOfFoundingField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractPurposeOfCredit PurposeOfCredit
        {
            get
            {
                return this.purposeOfCreditField;
            }
            set
            {
                this.purposeOfCreditField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCurrencyCode CurrencyCode
        {
            get
            {
                return this.currencyCodeField;
            }
            set
            {
                this.currencyCodeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractContractStatus ContractStatus
        {
            get
            {
                return this.contractStatusField;
            }
            set
            {
                this.contractStatusField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractDateOfApplication DateOfApplication
        {
            get
            {
                return this.dateOfApplicationField;
            }
            set
            {
                this.dateOfApplicationField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractDateOfCreditStart DateOfCreditStart
        {
            get
            {
                return this.dateOfCreditStartField;
            }
            set
            {
                this.dateOfCreditStartField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractDateOfCreditEnd DateOfCreditEnd
        {
            get
            {
                return this.dateOfCreditEndField;
            }
            set
            {
                this.dateOfCreditEndField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractDateOfRealRepayment DateOfRealRepayment
        {
            get
            {
                return this.dateOfRealRepaymentField;
            }
            set
            {
                this.dateOfRealRepaymentField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractDateAgreementGuarantee DateAgreementGuarantee
        {
            get
            {
                return this.dateAgreementGuaranteeField;
            }
            set
            {
                this.dateAgreementGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractGuaranteeEvent GuaranteeEvent
        {
            get
            {
                return this.guaranteeEventField;
            }
            set
            {
                this.guaranteeEventField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractLastUpdate LastUpdate
        {
            get
            {
                return this.lastUpdateField;
            }
            set
            {
                this.lastUpdateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateral Collateral
        {
            get
            {
                return this.collateralField;
            }
            set
            {
                this.collateralField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractClassificationOfContract ClassificationOfContract
        {
            get
            {
                return this.classificationOfContractField;
            }
            set
            {
                this.classificationOfContractField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractTotalAmount TotalAmount
        {
            get
            {
                return this.totalAmountField;
            }
            set
            {
                this.totalAmountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractAmount Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditUsage CreditUsage
        {
            get
            {
                return this.creditUsageField;
            }
            set
            {
                this.creditUsageField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractResidualAmount ResidualAmount
        {
            get
            {
                return this.residualAmountField;
            }
            set
            {
                this.residualAmountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLimit CreditLimit
        {
            get
            {
                return this.creditLimitField;
            }
            set
            {
                this.creditLimitField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNumberOfOutstandingInstalments NumberOfOutstandingInstalments
        {
            get
            {
                return this.numberOfOutstandingInstalmentsField;
            }
            set
            {
                this.numberOfOutstandingInstalmentsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNumberOfInstalments NumberOfInstalments
        {
            get
            {
                return this.numberOfInstalmentsField;
            }
            set
            {
                this.numberOfInstalmentsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractOutstandingAmount OutstandingAmount
        {
            get
            {
                return this.outstandingAmountField;
            }
            set
            {
                this.outstandingAmountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractPeriodicityOfPayments PeriodicityOfPayments
        {
            get
            {
                return this.periodicityOfPaymentsField;
            }
            set
            {
                this.periodicityOfPaymentsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractMethodOfPayments MethodOfPayments
        {
            get
            {
                return this.methodOfPaymentsField;
            }
            set
            {
                this.methodOfPaymentsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNumberOfOverdueInstalments NumberOfOverdueInstalments
        {
            get
            {
                return this.numberOfOverdueInstalmentsField;
            }
            set
            {
                this.numberOfOverdueInstalmentsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractOverdueAmount OverdueAmount
        {
            get
            {
                return this.overdueAmountField;
            }
            set
            {
                this.overdueAmountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractMonthlyInstalmentAmount MonthlyInstalmentAmount
        {
            get
            {
                return this.monthlyInstalmentAmountField;
            }
            set
            {
                this.monthlyInstalmentAmountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractInterestRate InterestRate
        {
            get
            {
                return this.interestRateField;
            }
            set
            {
                this.interestRateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractFinancialInstitution FinancialInstitution
        {
            get
            {
                return this.financialInstitutionField;
            }
            set
            {
                this.financialInstitutionField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractSpecialRelationship SpecialRelationship
        {
            get
            {
                return this.specialRelationshipField;
            }
            set
            {
                this.specialRelationshipField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractAnnualEffectiveRate AnnualEffectiveRate
        {
            get
            {
                return this.annualEffectiveRateField;
            }
            set
            {
                this.annualEffectiveRateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNominalRate NominalRate
        {
            get
            {
                return this.nominalRateField;
            }
            set
            {
                this.nominalRateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractAmountProvisions AmountProvisions
        {
            get
            {
                return this.amountProvisionsField;
            }
            set
            {
                this.amountProvisionsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractLoanAccount LoanAccount
        {
            get
            {
                return this.loanAccountField;
            }
            set
            {
                this.loanAccountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractGracePrincipal GracePrincipal
        {
            get
            {
                return this.gracePrincipalField;
            }
            set
            {
                this.gracePrincipalField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractGracePay GracePay
        {
            get
            {
                return this.gracePayField;
            }
            set
            {
                this.gracePayField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractPlaceOfDisbursement PlaceOfDisbursement
        {
            get
            {
                return this.placeOfDisbursementField;
            }
            set
            {
                this.placeOfDisbursementField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Year", IsNullable = false)]
        public RootExistingContractsContractYear[] PaymentsCalendar
        {
            get
            {
                return this.paymentsCalendarField;
            }
            set
            {
                this.paymentsCalendarField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractContractThirdParty ContractThirdParty
        {
            get
            {
                return this.contractThirdPartyField;
            }
            set
            {
                this.contractThirdPartyField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractParentContractCode ParentContractCode
        {
            get
            {
                return this.parentContractCodeField;
            }
            set
            {
                this.parentContractCodeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractParentProvider ParentProvider
        {
            get
            {
                return this.parentProviderField;
            }
            set
            {
                this.parentProviderField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractParentContractStatus ParentContractStatus
        {
            get
            {
                return this.parentContractStatusField;
            }
            set
            {
                this.parentContractStatusField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractParentOperationDate ParentOperationDate
        {
            get
            {
                return this.parentOperationDateField;
            }
            set
            {
                this.parentOperationDateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractFine Fine
        {
            get
            {
                return this.fineField;
            }
            set
            {
                this.fineField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractPenalty Penalty
        {
            get
            {
                return this.penaltyField;
            }
            set
            {
                this.penaltyField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractBranchLocation BranchLocation
        {
            get
            {
                return this.branchLocationField;
            }
            set
            {
                this.branchLocationField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractProstringationCount ProstringationCount
        {
            get
            {
                return this.prostringationCountField;
            }
            set
            {
                this.prostringationCountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNumberOfTransactions NumberOfTransactions
        {
            get
            {
                return this.numberOfTransactionsField;
            }
            set
            {
                this.numberOfTransactionsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractInstalmentAmount InstalmentAmount
        {
            get
            {
                return this.instalmentAmountField;
            }
            set
            {
                this.instalmentAmountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractDateOfInserted DateOfInserted
        {
            get
            {
                return this.dateOfInsertedField;
            }
            set
            {
                this.dateOfInsertedField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNumberOfOverdueInstalmentsMax NumberOfOverdueInstalmentsMax
        {
            get
            {
                return this.numberOfOverdueInstalmentsMaxField;
            }
            set
            {
                this.numberOfOverdueInstalmentsMaxField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNumberOfOverdueInstalmentsMaxDate NumberOfOverdueInstalmentsMaxDate
        {
            get
            {
                return this.numberOfOverdueInstalmentsMaxDateField;
            }
            set
            {
                this.numberOfOverdueInstalmentsMaxDateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractNumberOfOverdueInstalmentsMaxAmount NumberOfOverdueInstalmentsMaxAmount
        {
            get
            {
                return this.numberOfOverdueInstalmentsMaxAmountField;
            }
            set
            {
                this.numberOfOverdueInstalmentsMaxAmountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractOverdueAmountMax OverdueAmountMax
        {
            get
            {
                return this.overdueAmountMaxField;
            }
            set
            {
                this.overdueAmountMaxField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractOverdueAmountMaxDate OverdueAmountMaxDate
        {
            get
            {
                return this.overdueAmountMaxDateField;
            }
            set
            {
                this.overdueAmountMaxDateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractOverdueAmountMaxCount OverdueAmountMaxCount
        {
            get
            {
                return this.overdueAmountMaxCountField;
            }
            set
            {
                this.overdueAmountMaxCountField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditObject CreditObject
        {
            get
            {
                return this.creditObjectField;
            }
            set
            {
                this.creditObjectField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractInterconnectedSubjects InterconnectedSubjects
        {
            get
            {
                return this.interconnectedSubjectsField;
            }
            set
            {
                this.interconnectedSubjectsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLineParentCode CreditLineParentCode
        {
            get
            {
                return this.creditLineParentCodeField;
            }
            set
            {
                this.creditLineParentCodeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractFundingSource FundingSource
        {
            get
            {
                return this.fundingSourceField;
            }
            set
            {
                this.fundingSourceField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelations CreditLineRelations
        {
            get
            {
                return this.creditLineRelationsField;
            }
            set
            {
                this.creditLineRelationsField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractAvailableDate AvailableDate
        {
            get
            {
                return this.availableDateField;
            }
            set
            {
                this.availableDateField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractAvailableLimit AvailableLimit
        {
            get
            {
                return this.availableLimitField;
            }
            set
            {
                this.availableLimitField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractSchema Schema
        {
            get
            {
                return this.schemaField;
            }
            set
            {
                this.schemaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ContractType
        {
            get
            {
                return this.contractTypeField;
            }
            set
            {
                this.contractTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ContractTypeCode
        {
            get
            {
                return this.contractTypeCodeField;
            }
            set
            {
                this.contractTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCodeOfContract
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractAgreementNumber
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractAgreementNumberGuarantee
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractTypeOfFounding
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractPurposeOfCredit
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCurrencyCode
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractContractStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractDateOfApplication
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractDateOfCreditStart
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractDateOfCreditEnd
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractDateOfRealRepayment
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractDateAgreementGuarantee
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractGuaranteeEvent
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractLastUpdate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateral
    {

        private RootExistingContractsContractCollateralTypeOfGuarantee typeOfGuaranteeField;

        private RootExistingContractsContractCollateralValueOfGuarantee valueOfGuaranteeField;

        private RootExistingContractsContractCollateralTypeOfValueOfGuarantee typeOfValueOfGuaranteeField;

        private RootExistingContractsContractCollateralAdditionalInformation additionalInformationField;

        private RootExistingContractsContractCollateralPlaceOfGuarantee placeOfGuaranteeField;

        private RootExistingContractsContractCollateralTypeOfCollateral typeOfCollateralField;

        private RootExistingContractsContractCollateralCollateralStatus collateralStatusField;

        private RootExistingContractsContractCollateralCollateralName collateralNameField;

        /// <remarks/>
        public RootExistingContractsContractCollateralTypeOfGuarantee TypeOfGuarantee
        {
            get
            {
                return this.typeOfGuaranteeField;
            }
            set
            {
                this.typeOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateralValueOfGuarantee ValueOfGuarantee
        {
            get
            {
                return this.valueOfGuaranteeField;
            }
            set
            {
                this.valueOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateralTypeOfValueOfGuarantee TypeOfValueOfGuarantee
        {
            get
            {
                return this.typeOfValueOfGuaranteeField;
            }
            set
            {
                this.typeOfValueOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateralAdditionalInformation AdditionalInformation
        {
            get
            {
                return this.additionalInformationField;
            }
            set
            {
                this.additionalInformationField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateralPlaceOfGuarantee PlaceOfGuarantee
        {
            get
            {
                return this.placeOfGuaranteeField;
            }
            set
            {
                this.placeOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateralTypeOfCollateral TypeOfCollateral
        {
            get
            {
                return this.typeOfCollateralField;
            }
            set
            {
                this.typeOfCollateralField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateralCollateralStatus CollateralStatus
        {
            get
            {
                return this.collateralStatusField;
            }
            set
            {
                this.collateralStatusField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCollateralCollateralName CollateralName
        {
            get
            {
                return this.collateralNameField;
            }
            set
            {
                this.collateralNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralTypeOfGuarantee
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralValueOfGuarantee
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralTypeOfValueOfGuarantee
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralAdditionalInformation
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralPlaceOfGuarantee
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralTypeOfCollateral
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralCollateralStatus
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCollateralCollateralName
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractClassificationOfContract
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractTotalAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditUsage
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractResidualAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLimit
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNumberOfOutstandingInstalments
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNumberOfInstalments
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractOutstandingAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractPeriodicityOfPayments
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractMethodOfPayments
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNumberOfOverdueInstalments
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractOverdueAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractMonthlyInstalmentAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractInterestRate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractSubjectRole
    {

        private string numberField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractFinancialInstitution
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractSpecialRelationship
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractAnnualEffectiveRate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNominalRate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractAmountProvisions
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractLoanAccount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractGracePrincipal
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractGracePay
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractPlaceOfDisbursement
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractYear
    {

        private RootExistingContractsContractYearPayment[] paymentField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Payment")]
        public RootExistingContractsContractYearPayment[] Payment
        {
            get
            {
                return this.paymentField;
            }
            set
            {
                this.paymentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractYearPayment
    {

        private string fineField;

        private string numberField;

        private string overdueField;

        private string penaltyField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string fine
        {
            get
            {
                return this.fineField;
            }
            set
            {
                this.fineField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string overdue
        {
            get
            {
                return this.overdueField;
            }
            set
            {
                this.overdueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string penalty
        {
            get
            {
                return this.penaltyField;
            }
            set
            {
                this.penaltyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractContractThirdParty
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractParentContractCode
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractParentProvider
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractParentContractStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractParentOperationDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractFine
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractPenalty
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractBranchLocation
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractProstringationCount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNumberOfTransactions
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractInstalmentAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractActualDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractDateOfInserted
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNumberOfOverdueInstalmentsMax
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNumberOfOverdueInstalmentsMaxDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractNumberOfOverdueInstalmentsMaxAmount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractOverdueAmountMax
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractOverdueAmountMaxDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractOverdueAmountMaxCount
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditObject
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractInterconnectedSubjects
    {

        private string nameField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineParentCode
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractFundingSource
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelations
    {

        private RootExistingContractsContractCreditLineRelationsExistingTranches existingTranchesField;

        private RootExistingContractsContractCreditLineRelationsTerminatedTranches terminatedTranchesField;

        private RootExistingContractsContractCreditLineRelationsWithdrawnTranches withdrawnTranchesField;

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsExistingTranches ExistingTranches
        {
            get
            {
                return this.existingTranchesField;
            }
            set
            {
                this.existingTranchesField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsTerminatedTranches TerminatedTranches
        {
            get
            {
                return this.terminatedTranchesField;
            }
            set
            {
                this.terminatedTranchesField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsWithdrawnTranches WithdrawnTranches
        {
            get
            {
                return this.withdrawnTranchesField;
            }
            set
            {
                this.withdrawnTranchesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsExistingTranches
    {

        private RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroup primaryGroupField;

        private RootExistingContractsContractCreditLineRelationsExistingTranchesSecondaryGroup secondaryGroupField;

        private string titleField;

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroup PrimaryGroup
        {
            get
            {
                return this.primaryGroupField;
            }
            set
            {
                this.primaryGroupField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsExistingTranchesSecondaryGroup SecondaryGroup
        {
            get
            {
                return this.secondaryGroupField;
            }
            set
            {
                this.secondaryGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroup
    {

        private RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroupContract[] contractField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Contract")]
        public RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroupContract[] Contract
        {
            get
            {
                return this.contractField;
            }
            set
            {
                this.contractField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroupContract
    {

        private RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroupContractContractCode contractCodeField;

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroupContractContractCode ContractCode
        {
            get
            {
                return this.contractCodeField;
            }
            set
            {
                this.contractCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsExistingTranchesPrimaryGroupContractContractCode
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsExistingTranchesSecondaryGroup
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsTerminatedTranches
    {

        private RootExistingContractsContractCreditLineRelationsTerminatedTranchesPrimaryGroup primaryGroupField;

        private RootExistingContractsContractCreditLineRelationsTerminatedTranchesSecondaryGroup secondaryGroupField;

        private string titleField;

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsTerminatedTranchesPrimaryGroup PrimaryGroup
        {
            get
            {
                return this.primaryGroupField;
            }
            set
            {
                this.primaryGroupField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsTerminatedTranchesSecondaryGroup SecondaryGroup
        {
            get
            {
                return this.secondaryGroupField;
            }
            set
            {
                this.secondaryGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsTerminatedTranchesPrimaryGroup
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsTerminatedTranchesSecondaryGroup
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsWithdrawnTranches
    {

        private RootExistingContractsContractCreditLineRelationsWithdrawnTranchesPrimaryGroup primaryGroupField;

        private RootExistingContractsContractCreditLineRelationsWithdrawnTranchesSecondaryGroup secondaryGroupField;

        private string titleField;

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsWithdrawnTranchesPrimaryGroup PrimaryGroup
        {
            get
            {
                return this.primaryGroupField;
            }
            set
            {
                this.primaryGroupField = value;
            }
        }

        /// <remarks/>
        public RootExistingContractsContractCreditLineRelationsWithdrawnTranchesSecondaryGroup SecondaryGroup
        {
            get
            {
                return this.secondaryGroupField;
            }
            set
            {
                this.secondaryGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsWithdrawnTranchesPrimaryGroup
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractCreditLineRelationsWithdrawnTranchesSecondaryGroup
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractAvailableDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractAvailableLimit
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootExistingContractsContractSchema
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootInterconnectedSubjects
    {

        private RootInterconnectedSubjectsInterconnectedSubject[] interconnectedSubjectField;

        private string nameField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InterconnectedSubject")]
        public RootInterconnectedSubjectsInterconnectedSubject[] InterconnectedSubject
        {
            get
            {
                return this.interconnectedSubjectField;
            }
            set
            {
                this.interconnectedSubjectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootInterconnectedSubjectsInterconnectedSubject
    {

        private RootInterconnectedSubjectsInterconnectedSubjectTypeOfLink typeOfLinkField;

        private RootInterconnectedSubjectsInterconnectedSubjectSubjectCode subjectCodeField;

        /// <remarks/>
        public RootInterconnectedSubjectsInterconnectedSubjectTypeOfLink TypeOfLink
        {
            get
            {
                return this.typeOfLinkField;
            }
            set
            {
                this.typeOfLinkField = value;
            }
        }

        /// <remarks/>
        public RootInterconnectedSubjectsInterconnectedSubjectSubjectCode SubjectCode
        {
            get
            {
                return this.subjectCodeField;
            }
            set
            {
                this.subjectCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootInterconnectedSubjectsInterconnectedSubjectTypeOfLink
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootInterconnectedSubjectsInterconnectedSubjectSubjectCode
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNumberOfQueries
    {

        private RootNumberOfQueriesDetailsQueries7days detailsQueries7daysField;

        private RootNumberOfQueriesDays30 days30Field;

        private RootNumberOfQueriesDays90 days90Field;

        private RootNumberOfQueriesDays120 days120Field;

        private RootNumberOfQueriesDays180 days180Field;

        private RootNumberOfQueriesDays360 days360Field;

        private string titleField;

        private string valueField;

        /// <remarks/>
        public RootNumberOfQueriesDetailsQueries7days DetailsQueries7days
        {
            get
            {
                return this.detailsQueries7daysField;
            }
            set
            {
                this.detailsQueries7daysField = value;
            }
        }

        /// <remarks/>
        public RootNumberOfQueriesDays30 Days30
        {
            get
            {
                return this.days30Field;
            }
            set
            {
                this.days30Field = value;
            }
        }

        /// <remarks/>
        public RootNumberOfQueriesDays90 Days90
        {
            get
            {
                return this.days90Field;
            }
            set
            {
                this.days90Field = value;
            }
        }

        /// <remarks/>
        public RootNumberOfQueriesDays120 Days120
        {
            get
            {
                return this.days120Field;
            }
            set
            {
                this.days120Field = value;
            }
        }

        /// <remarks/>
        public RootNumberOfQueriesDays180 Days180
        {
            get
            {
                return this.days180Field;
            }
            set
            {
                this.days180Field = value;
            }
        }

        /// <remarks/>
        public RootNumberOfQueriesDays360 Days360
        {
            get
            {
                return this.days360Field;
            }
            set
            {
                this.days360Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNumberOfQueriesDetailsQueries7days
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNumberOfQueriesDays30
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNumberOfQueriesDays90
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNumberOfQueriesDays120
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNumberOfQueriesDays180
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootNumberOfQueriesDays360
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootRelatedCompanies
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSources
    {

        private RootPublicSourcesQamqorList qamqorListField;

        private RootPublicSourcesKgdWanted kgdWantedField;

        private RootPublicSourcesQamqorAlimony qamqorAlimonyField;

        private RootPublicSourcesRNUGosZakup rNUGosZakupField;

        private RootPublicSourcesFalseBusi falseBusiField;

        private RootPublicSourcesTerrorList terrorListField;

        private RootPublicSourcesAreears areearsField;

        private RootPublicSourcesBankruptcy bankruptcyField;

        private RootPublicSourcesL150o10 l150o10Field;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesQamqorList QamqorList
        {
            get
            {
                return this.qamqorListField;
            }
            set
            {
                this.qamqorListField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesKgdWanted KgdWanted
        {
            get
            {
                return this.kgdWantedField;
            }
            set
            {
                this.kgdWantedField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesQamqorAlimony QamqorAlimony
        {
            get
            {
                return this.qamqorAlimonyField;
            }
            set
            {
                this.qamqorAlimonyField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesRNUGosZakup RNUGosZakup
        {
            get
            {
                return this.rNUGosZakupField;
            }
            set
            {
                this.rNUGosZakupField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesFalseBusi FalseBusi
        {
            get
            {
                return this.falseBusiField;
            }
            set
            {
                this.falseBusiField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesTerrorList TerrorList
        {
            get
            {
                return this.terrorListField;
            }
            set
            {
                this.terrorListField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesAreears Areears
        {
            get
            {
                return this.areearsField;
            }
            set
            {
                this.areearsField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesBankruptcy Bankruptcy
        {
            get
            {
                return this.bankruptcyField;
            }
            set
            {
                this.bankruptcyField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesL150o10 L150o10
        {
            get
            {
                return this.l150o10Field;
            }
            set
            {
                this.l150o10Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorList
    {

        private RootPublicSourcesQamqorListSource sourceField;

        private RootPublicSourcesQamqorListActualDate actualDateField;

        private RootPublicSourcesQamqorListRefreshDate refreshDateField;

        private RootPublicSourcesQamqorListStatus statusField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesQamqorListSource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesQamqorListActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesQamqorListRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesQamqorListStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorListSource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorListActualDate
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorListRefreshDate
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorListStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesKgdWanted
    {

        private RootPublicSourcesKgdWantedSource sourceField;

        private RootPublicSourcesKgdWantedActualDate actualDateField;

        private RootPublicSourcesKgdWantedRefreshDate refreshDateField;

        private RootPublicSourcesKgdWantedStatus statusField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesKgdWantedSource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesKgdWantedActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesKgdWantedRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesKgdWantedStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesKgdWantedSource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesKgdWantedActualDate
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesKgdWantedRefreshDate
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesKgdWantedStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorAlimony
    {

        private RootPublicSourcesQamqorAlimonySource sourceField;

        private RootPublicSourcesQamqorAlimonyActualDate actualDateField;

        private RootPublicSourcesQamqorAlimonyRefreshDate refreshDateField;

        private RootPublicSourcesQamqorAlimonyStatus statusField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesQamqorAlimonySource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesQamqorAlimonyActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesQamqorAlimonyRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesQamqorAlimonyStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorAlimonySource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorAlimonyActualDate
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorAlimonyRefreshDate
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesQamqorAlimonyStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesRNUGosZakup
    {

        private RootPublicSourcesRNUGosZakupSource sourceField;

        private RootPublicSourcesRNUGosZakupRefreshDate refreshDateField;

        private RootPublicSourcesRNUGosZakupActualDate actualDateField;

        private RootPublicSourcesRNUGosZakupStatus statusField;

        private RootPublicSourcesRNUGosZakupCompanies companiesField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesRNUGosZakupSource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesRNUGosZakupRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesRNUGosZakupActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesRNUGosZakupStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesRNUGosZakupCompanies Companies
        {
            get
            {
                return this.companiesField;
            }
            set
            {
                this.companiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesRNUGosZakupSource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesRNUGosZakupRefreshDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesRNUGosZakupActualDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesRNUGosZakupStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesRNUGosZakupCompanies
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesFalseBusi
    {

        private RootPublicSourcesFalseBusiSource sourceField;

        private RootPublicSourcesFalseBusiRefreshDate refreshDateField;

        private RootPublicSourcesFalseBusiActualDate actualDateField;

        private RootPublicSourcesFalseBusiStatus statusField;

        private RootPublicSourcesFalseBusiCompanies companiesField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesFalseBusiSource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesFalseBusiRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesFalseBusiActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesFalseBusiStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesFalseBusiCompanies Companies
        {
            get
            {
                return this.companiesField;
            }
            set
            {
                this.companiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesFalseBusiSource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesFalseBusiRefreshDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesFalseBusiActualDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesFalseBusiStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesFalseBusiCompanies
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesTerrorList
    {

        private RootPublicSourcesTerrorListSource sourceField;

        private RootPublicSourcesTerrorListRefreshDate refreshDateField;

        private RootPublicSourcesTerrorListActualDate actualDateField;

        private RootPublicSourcesTerrorListStatus statusField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesTerrorListSource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesTerrorListRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesTerrorListActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesTerrorListStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesTerrorListSource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesTerrorListRefreshDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesTerrorListActualDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesTerrorListStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesAreears
    {

        private RootPublicSourcesAreearsSource sourceField;

        private RootPublicSourcesAreearsRefreshDate refreshDateField;

        private RootPublicSourcesAreearsActualDate actualDateField;

        private RootPublicSourcesAreearsStatus statusField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesAreearsSource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesAreearsRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesAreearsActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesAreearsStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesAreearsSource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesAreearsRefreshDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesAreearsActualDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesAreearsStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesBankruptcy
    {

        private RootPublicSourcesBankruptcySource sourceField;

        private RootPublicSourcesBankruptcyRefreshDate refreshDateField;

        private RootPublicSourcesBankruptcyActualDate actualDateField;

        private RootPublicSourcesBankruptcyStatus statusField;

        private RootPublicSourcesBankruptcyCompanies companiesField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesBankruptcySource Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesBankruptcyRefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesBankruptcyActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesBankruptcyStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesBankruptcyCompanies Companies
        {
            get
            {
                return this.companiesField;
            }
            set
            {
                this.companiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesBankruptcySource
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesBankruptcyRefreshDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesBankruptcyActualDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesBankruptcyStatus
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesBankruptcyCompanies
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesL150o10
    {

        private RootPublicSourcesL150o10Source sourceField;

        private RootPublicSourcesL150o10ActualDate actualDateField;

        private RootPublicSourcesL150o10RefreshDate refreshDateField;

        private RootPublicSourcesL150o10Status statusField;

        private RootPublicSourcesL150o10Companies companiesField;

        private string titleField;

        /// <remarks/>
        public RootPublicSourcesL150o10Source Source
        {
            get
            {
                return this.sourceField;
            }
            set
            {
                this.sourceField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesL150o10ActualDate ActualDate
        {
            get
            {
                return this.actualDateField;
            }
            set
            {
                this.actualDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesL150o10RefreshDate RefreshDate
        {
            get
            {
                return this.refreshDateField;
            }
            set
            {
                this.refreshDateField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesL150o10Status Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public RootPublicSourcesL150o10Companies Companies
        {
            get
            {
                return this.companiesField;
            }
            set
            {
                this.companiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesL150o10Source
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesL150o10ActualDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesL150o10RefreshDate
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesL150o10Status
    {

        private string idField;

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootPublicSourcesL150o10Companies
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootRWA170
    {

        private RootRWA170Contracts contractsField;

        private string notFoundTextField;

        private string titleField;

        /// <remarks/>
        public RootRWA170Contracts Contracts
        {
            get
            {
                return this.contractsField;
            }
            set
            {
                this.contractsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NotFoundText
        {
            get
            {
                return this.notFoundTextField;
            }
            set
            {
                this.notFoundTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootRWA170Contracts
    {

        private RootRWA170ContractsContract[] contractField;

        private string contractsSummField;

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Contract")]
        public RootRWA170ContractsContract[] Contract
        {
            get
            {
                return this.contractField;
            }
            set
            {
                this.contractField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ContractsSumm
        {
            get
            {
                return this.contractsSummField;
            }
            set
            {
                this.contractsSummField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootRWA170ContractsContract
    {

        private RootRWA170ContractsContractContractCode contractCodeField;

        private RootRWA170ContractsContractContractSumm contractSummField;

        /// <remarks/>
        public RootRWA170ContractsContractContractCode ContractCode
        {
            get
            {
                return this.contractCodeField;
            }
            set
            {
                this.contractCodeField = value;
            }
        }

        /// <remarks/>
        public RootRWA170ContractsContractContractSumm ContractSumm
        {
            get
            {
                return this.contractSummField;
            }
            set
            {
                this.contractSummField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootRWA170ContractsContractContractCode
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootRWA170ContractsContractContractSumm
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooter
    {

        private RootFooterDateOfIssue dateOfIssueField;

        private RootFooterContactInfo contactInfoField;

        private RootFooterContracts contractsField;

        private RootFooterFiveYearsWarning fiveYearsWarningField;

        private RootFooterAuthenticityWarning authenticityWarningField;

        private RootFooterFooter footerField;

        private RootFooterExistingContracts existingContractsField;

        private RootFooterTerminatedContracts terminatedContractsField;

        private RootFooterWithdrawnApplications withdrawnApplicationsField;

        private RootFooterNoAddresses noAddressesField;

        private RootFooterNoContacts noContactsField;

        private RootFooterGuarant guarantField;

        private RootFooterBorrower borrowerField;

        private RootFooterSubjectRole subjectRoleField;

        private RootFooterNoDocuments noDocumentsField;

        private RootFooterNoExistingContracts noExistingContractsField;

        private RootFooterNoNegativeStatus noNegativeStatusField;

        private RootFooterNoTerminatedContracts noTerminatedContractsField;

        private RootFooterNoTerminatedContractsDateLimit noTerminatedContractsDateLimitField;

        private RootFooterNoWithdrawnApplications noWithdrawnApplicationsField;

        private RootFooterTitleDetailedInformation titleDetailedInformationField;

        private RootFooterTitleAdditionalInformation titleAdditionalInformationField;

        private RootFooterTitleLiabilities titleLiabilitiesField;

        private RootFooterInterconnectedSubjects interconnectedSubjectsField;

        private RootFooterTitleSummaryInformation titleSummaryInformationField;

        private RootFooterContract contractField;

        private RootFooterSubjectDetails2 subjectDetails2Field;

        private RootFooterAmountOverdue amountOverdueField;

        private RootFooterMaxOverdueAmount maxOverdueAmountField;

        private RootFooterMaxOverdueCount maxOverdueCountField;

        private RootFooterPaymentsCalendar paymentsCalendarField;

        private RootFooterNoSubjects noSubjectsField;

        private RootFooterNoViolations noViolationsField;

        private RootFooterInformationFromDB informationFromDBField;

        private RootFooterInformationFromPS informationFromPSField;

        private RootFooterInformationFromPSS informationFromPSSField;

        private RootFooterInformationFromPSS3 informationFromPSS3Field;

        private RootFooterInformationFromPCR informationFromPCRField;

        private RootFooterInformationFromGDB informationFromGDBField;

        private RootFooterTranches tranchesField;

        private RootFooterTestWarning testWarningField;

        private RootFooterAdvCC advCCField;

        private RootFooterIncludingRehabilitation includingRehabilitationField;

        private string endOfReportField;

        private string nameField;

        /// <remarks/>
        public RootFooterDateOfIssue DateOfIssue
        {
            get
            {
                return this.dateOfIssueField;
            }
            set
            {
                this.dateOfIssueField = value;
            }
        }

        /// <remarks/>
        public RootFooterContactInfo ContactInfo
        {
            get
            {
                return this.contactInfoField;
            }
            set
            {
                this.contactInfoField = value;
            }
        }

        /// <remarks/>
        public RootFooterContracts Contracts
        {
            get
            {
                return this.contractsField;
            }
            set
            {
                this.contractsField = value;
            }
        }

        /// <remarks/>
        public RootFooterFiveYearsWarning FiveYearsWarning
        {
            get
            {
                return this.fiveYearsWarningField;
            }
            set
            {
                this.fiveYearsWarningField = value;
            }
        }

        /// <remarks/>
        public RootFooterAuthenticityWarning AuthenticityWarning
        {
            get
            {
                return this.authenticityWarningField;
            }
            set
            {
                this.authenticityWarningField = value;
            }
        }

        /// <remarks/>
        public RootFooterFooter Footer
        {
            get
            {
                return this.footerField;
            }
            set
            {
                this.footerField = value;
            }
        }

        /// <remarks/>
        public RootFooterExistingContracts ExistingContracts
        {
            get
            {
                return this.existingContractsField;
            }
            set
            {
                this.existingContractsField = value;
            }
        }

        /// <remarks/>
        public RootFooterTerminatedContracts TerminatedContracts
        {
            get
            {
                return this.terminatedContractsField;
            }
            set
            {
                this.terminatedContractsField = value;
            }
        }

        /// <remarks/>
        public RootFooterWithdrawnApplications WithdrawnApplications
        {
            get
            {
                return this.withdrawnApplicationsField;
            }
            set
            {
                this.withdrawnApplicationsField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoAddresses NoAddresses
        {
            get
            {
                return this.noAddressesField;
            }
            set
            {
                this.noAddressesField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoContacts NoContacts
        {
            get
            {
                return this.noContactsField;
            }
            set
            {
                this.noContactsField = value;
            }
        }

        /// <remarks/>
        public RootFooterGuarant Guarant
        {
            get
            {
                return this.guarantField;
            }
            set
            {
                this.guarantField = value;
            }
        }

        /// <remarks/>
        public RootFooterBorrower Borrower
        {
            get
            {
                return this.borrowerField;
            }
            set
            {
                this.borrowerField = value;
            }
        }

        /// <remarks/>
        public RootFooterSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoDocuments NoDocuments
        {
            get
            {
                return this.noDocumentsField;
            }
            set
            {
                this.noDocumentsField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoExistingContracts NoExistingContracts
        {
            get
            {
                return this.noExistingContractsField;
            }
            set
            {
                this.noExistingContractsField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoNegativeStatus NoNegativeStatus
        {
            get
            {
                return this.noNegativeStatusField;
            }
            set
            {
                this.noNegativeStatusField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoTerminatedContracts NoTerminatedContracts
        {
            get
            {
                return this.noTerminatedContractsField;
            }
            set
            {
                this.noTerminatedContractsField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoTerminatedContractsDateLimit NoTerminatedContractsDateLimit
        {
            get
            {
                return this.noTerminatedContractsDateLimitField;
            }
            set
            {
                this.noTerminatedContractsDateLimitField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoWithdrawnApplications NoWithdrawnApplications
        {
            get
            {
                return this.noWithdrawnApplicationsField;
            }
            set
            {
                this.noWithdrawnApplicationsField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleDetailedInformation TitleDetailedInformation
        {
            get
            {
                return this.titleDetailedInformationField;
            }
            set
            {
                this.titleDetailedInformationField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleAdditionalInformation TitleAdditionalInformation
        {
            get
            {
                return this.titleAdditionalInformationField;
            }
            set
            {
                this.titleAdditionalInformationField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilities TitleLiabilities
        {
            get
            {
                return this.titleLiabilitiesField;
            }
            set
            {
                this.titleLiabilitiesField = value;
            }
        }

        /// <remarks/>
        public RootFooterInterconnectedSubjects InterconnectedSubjects
        {
            get
            {
                return this.interconnectedSubjectsField;
            }
            set
            {
                this.interconnectedSubjectsField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleSummaryInformation TitleSummaryInformation
        {
            get
            {
                return this.titleSummaryInformationField;
            }
            set
            {
                this.titleSummaryInformationField = value;
            }
        }

        /// <remarks/>
        public RootFooterContract Contract
        {
            get
            {
                return this.contractField;
            }
            set
            {
                this.contractField = value;
            }
        }

        /// <remarks/>
        public RootFooterSubjectDetails2 SubjectDetails2
        {
            get
            {
                return this.subjectDetails2Field;
            }
            set
            {
                this.subjectDetails2Field = value;
            }
        }

        /// <remarks/>
        public RootFooterAmountOverdue AmountOverdue
        {
            get
            {
                return this.amountOverdueField;
            }
            set
            {
                this.amountOverdueField = value;
            }
        }

        /// <remarks/>
        public RootFooterMaxOverdueAmount MaxOverdueAmount
        {
            get
            {
                return this.maxOverdueAmountField;
            }
            set
            {
                this.maxOverdueAmountField = value;
            }
        }

        /// <remarks/>
        public RootFooterMaxOverdueCount MaxOverdueCount
        {
            get
            {
                return this.maxOverdueCountField;
            }
            set
            {
                this.maxOverdueCountField = value;
            }
        }

        /// <remarks/>
        public RootFooterPaymentsCalendar PaymentsCalendar
        {
            get
            {
                return this.paymentsCalendarField;
            }
            set
            {
                this.paymentsCalendarField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoSubjects NoSubjects
        {
            get
            {
                return this.noSubjectsField;
            }
            set
            {
                this.noSubjectsField = value;
            }
        }

        /// <remarks/>
        public RootFooterNoViolations NoViolations
        {
            get
            {
                return this.noViolationsField;
            }
            set
            {
                this.noViolationsField = value;
            }
        }

        /// <remarks/>
        public RootFooterInformationFromDB InformationFromDB
        {
            get
            {
                return this.informationFromDBField;
            }
            set
            {
                this.informationFromDBField = value;
            }
        }

        /// <remarks/>
        public RootFooterInformationFromPS InformationFromPS
        {
            get
            {
                return this.informationFromPSField;
            }
            set
            {
                this.informationFromPSField = value;
            }
        }

        /// <remarks/>
        public RootFooterInformationFromPSS InformationFromPSS
        {
            get
            {
                return this.informationFromPSSField;
            }
            set
            {
                this.informationFromPSSField = value;
            }
        }

        /// <remarks/>
        public RootFooterInformationFromPSS3 InformationFromPSS3
        {
            get
            {
                return this.informationFromPSS3Field;
            }
            set
            {
                this.informationFromPSS3Field = value;
            }
        }

        /// <remarks/>
        public RootFooterInformationFromPCR InformationFromPCR
        {
            get
            {
                return this.informationFromPCRField;
            }
            set
            {
                this.informationFromPCRField = value;
            }
        }

        /// <remarks/>
        public RootFooterInformationFromGDB InformationFromGDB
        {
            get
            {
                return this.informationFromGDBField;
            }
            set
            {
                this.informationFromGDBField = value;
            }
        }

        /// <remarks/>
        public RootFooterTranches Tranches
        {
            get
            {
                return this.tranchesField;
            }
            set
            {
                this.tranchesField = value;
            }
        }

        /// <remarks/>
        public RootFooterTestWarning TestWarning
        {
            get
            {
                return this.testWarningField;
            }
            set
            {
                this.testWarningField = value;
            }
        }

        /// <remarks/>
        public RootFooterAdvCC AdvCC
        {
            get
            {
                return this.advCCField;
            }
            set
            {
                this.advCCField = value;
            }
        }

        /// <remarks/>
        public RootFooterIncludingRehabilitation IncludingRehabilitation
        {
            get
            {
                return this.includingRehabilitationField;
            }
            set
            {
                this.includingRehabilitationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EndOfReport
        {
            get
            {
                return this.endOfReportField;
            }
            set
            {
                this.endOfReportField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterDateOfIssue
    {

        private string titleField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContactInfo
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContracts
    {

        private RootFooterContractsAdditionalInformation additionalInformationField;

        private RootFooterContractsBalance balanceField;

        private RootFooterContractsCollaterals collateralsField;

        private RootFooterContractsContract contractField;

        private RootFooterContractsDaysOverdue daysOverdueField;

        private RootFooterContractsPayment paymentField;

        private RootFooterContractsRelatedSubjects relatedSubjectsField;

        private RootFooterContractsStatus statusField;

        private RootFooterContractsTypeOfGuarantee typeOfGuaranteeField;

        private RootFooterContractsValueOfGuarantee valueOfGuaranteeField;

        private RootFooterContractsProstringationStartDate prostringationStartDateField;

        private RootFooterContractsProstringationEndDate prostringationEndDateField;

        private RootFooterContractsCollateral collateralField;

        /// <remarks/>
        public RootFooterContractsAdditionalInformation AdditionalInformation
        {
            get
            {
                return this.additionalInformationField;
            }
            set
            {
                this.additionalInformationField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsBalance Balance
        {
            get
            {
                return this.balanceField;
            }
            set
            {
                this.balanceField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollaterals Collaterals
        {
            get
            {
                return this.collateralsField;
            }
            set
            {
                this.collateralsField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsContract Contract
        {
            get
            {
                return this.contractField;
            }
            set
            {
                this.contractField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsDaysOverdue DaysOverdue
        {
            get
            {
                return this.daysOverdueField;
            }
            set
            {
                this.daysOverdueField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsPayment Payment
        {
            get
            {
                return this.paymentField;
            }
            set
            {
                this.paymentField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsRelatedSubjects RelatedSubjects
        {
            get
            {
                return this.relatedSubjectsField;
            }
            set
            {
                this.relatedSubjectsField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsTypeOfGuarantee TypeOfGuarantee
        {
            get
            {
                return this.typeOfGuaranteeField;
            }
            set
            {
                this.typeOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsValueOfGuarantee ValueOfGuarantee
        {
            get
            {
                return this.valueOfGuaranteeField;
            }
            set
            {
                this.valueOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsProstringationStartDate ProstringationStartDate
        {
            get
            {
                return this.prostringationStartDateField;
            }
            set
            {
                this.prostringationStartDateField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsProstringationEndDate ProstringationEndDate
        {
            get
            {
                return this.prostringationEndDateField;
            }
            set
            {
                this.prostringationEndDateField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollateral Collateral
        {
            get
            {
                return this.collateralField;
            }
            set
            {
                this.collateralField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsAdditionalInformation
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsBalance
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollaterals
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsContract
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsDaysOverdue
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsPayment
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsRelatedSubjects
    {

        private RootFooterContractsRelatedSubjectsDocuments documentsField;

        private RootFooterContractsRelatedSubjectsFIO fIOField;

        private RootFooterContractsRelatedSubjectsSubjectRole subjectRoleField;

        private string stitleField;

        /// <remarks/>
        public RootFooterContractsRelatedSubjectsDocuments Documents
        {
            get
            {
                return this.documentsField;
            }
            set
            {
                this.documentsField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsRelatedSubjectsFIO FIO
        {
            get
            {
                return this.fIOField;
            }
            set
            {
                this.fIOField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsRelatedSubjectsSubjectRole SubjectRole
        {
            get
            {
                return this.subjectRoleField;
            }
            set
            {
                this.subjectRoleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsRelatedSubjectsDocuments
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsRelatedSubjectsFIO
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsRelatedSubjectsSubjectRole
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsStatus
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsTypeOfGuarantee
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsValueOfGuarantee
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsProstringationStartDate
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsProstringationEndDate
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateral
    {

        private RootFooterContractsCollateralTypeOfGuarantee typeOfGuaranteeField;

        private RootFooterContractsCollateralValueOfGuarantee valueOfGuaranteeField;

        private RootFooterContractsCollateralTypeOfValueOfGuarantee typeOfValueOfGuaranteeField;

        private RootFooterContractsCollateralPlaceOfGuarantee placeOfGuaranteeField;

        private RootFooterContractsCollateralCollateralStatus collateralStatusField;

        private RootFooterContractsCollateralCollateralName collateralNameField;

        private RootFooterContractsCollateralTypeOfCollateral typeOfCollateralField;

        /// <remarks/>
        public RootFooterContractsCollateralTypeOfGuarantee TypeOfGuarantee
        {
            get
            {
                return this.typeOfGuaranteeField;
            }
            set
            {
                this.typeOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollateralValueOfGuarantee ValueOfGuarantee
        {
            get
            {
                return this.valueOfGuaranteeField;
            }
            set
            {
                this.valueOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollateralTypeOfValueOfGuarantee TypeOfValueOfGuarantee
        {
            get
            {
                return this.typeOfValueOfGuaranteeField;
            }
            set
            {
                this.typeOfValueOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollateralPlaceOfGuarantee PlaceOfGuarantee
        {
            get
            {
                return this.placeOfGuaranteeField;
            }
            set
            {
                this.placeOfGuaranteeField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollateralCollateralStatus CollateralStatus
        {
            get
            {
                return this.collateralStatusField;
            }
            set
            {
                this.collateralStatusField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollateralCollateralName CollateralName
        {
            get
            {
                return this.collateralNameField;
            }
            set
            {
                this.collateralNameField = value;
            }
        }

        /// <remarks/>
        public RootFooterContractsCollateralTypeOfCollateral TypeOfCollateral
        {
            get
            {
                return this.typeOfCollateralField;
            }
            set
            {
                this.typeOfCollateralField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateralTypeOfGuarantee
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateralValueOfGuarantee
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateralTypeOfValueOfGuarantee
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateralPlaceOfGuarantee
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateralCollateralStatus
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateralCollateralName
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContractsCollateralTypeOfCollateral
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterFiveYearsWarning
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterAuthenticityWarning
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterFooter
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterExistingContracts
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTerminatedContracts
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterWithdrawnApplications
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoAddresses
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoContacts
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterGuarant
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterBorrower
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterSubjectRole
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoDocuments
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoExistingContracts
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoNegativeStatus
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoTerminatedContracts
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoTerminatedContractsDateLimit
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoWithdrawnApplications
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleDetailedInformation
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleAdditionalInformation
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilities
    {

        private RootFooterTitleLiabilitiesContractsNumber contractsNumberField;

        private RootFooterTitleLiabilitiesContractsPhase contractsPhaseField;

        private RootFooterTitleLiabilitiesContractStatus contractStatusField;

        private RootFooterTitleLiabilitiesOutstandingAmount outstandingAmountField;

        private RootFooterTitleLiabilitiesOverdueAmount overdueAmountField;

        private RootFooterTitleLiabilitiesNumberOfCreditLines numberOfCreditLinesField;

        private RootFooterTitleLiabilitiesTotalFine totalFineField;

        private RootFooterTitleLiabilitiesTotalPenalty totalPenaltyField;

        private string stitleField;

        /// <remarks/>
        public RootFooterTitleLiabilitiesContractsNumber ContractsNumber
        {
            get
            {
                return this.contractsNumberField;
            }
            set
            {
                this.contractsNumberField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilitiesContractsPhase ContractsPhase
        {
            get
            {
                return this.contractsPhaseField;
            }
            set
            {
                this.contractsPhaseField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilitiesContractStatus ContractStatus
        {
            get
            {
                return this.contractStatusField;
            }
            set
            {
                this.contractStatusField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilitiesOutstandingAmount OutstandingAmount
        {
            get
            {
                return this.outstandingAmountField;
            }
            set
            {
                this.outstandingAmountField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilitiesOverdueAmount OverdueAmount
        {
            get
            {
                return this.overdueAmountField;
            }
            set
            {
                this.overdueAmountField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilitiesNumberOfCreditLines NumberOfCreditLines
        {
            get
            {
                return this.numberOfCreditLinesField;
            }
            set
            {
                this.numberOfCreditLinesField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilitiesTotalFine TotalFine
        {
            get
            {
                return this.totalFineField;
            }
            set
            {
                this.totalFineField = value;
            }
        }

        /// <remarks/>
        public RootFooterTitleLiabilitiesTotalPenalty TotalPenalty
        {
            get
            {
                return this.totalPenaltyField;
            }
            set
            {
                this.totalPenaltyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesContractsNumber
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesContractsPhase
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesContractStatus
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesOutstandingAmount
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesOverdueAmount
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesNumberOfCreditLines
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesTotalFine
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleLiabilitiesTotalPenalty
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInterconnectedSubjects
    {

        private RootFooterInterconnectedSubjectsTypeOfLink typeOfLinkField;

        private RootFooterInterconnectedSubjectsSubjectCode subjectCodeField;

        private RootFooterInterconnectedSubjectsSubjectName subjectNameField;

        private RootFooterInterconnectedSubjectsSubjectRNN subjectRNNField;

        private RootFooterInterconnectedSubjectsSubjectIIN subjectIINField;

        /// <remarks/>
        public RootFooterInterconnectedSubjectsTypeOfLink TypeOfLink
        {
            get
            {
                return this.typeOfLinkField;
            }
            set
            {
                this.typeOfLinkField = value;
            }
        }

        /// <remarks/>
        public RootFooterInterconnectedSubjectsSubjectCode SubjectCode
        {
            get
            {
                return this.subjectCodeField;
            }
            set
            {
                this.subjectCodeField = value;
            }
        }

        /// <remarks/>
        public RootFooterInterconnectedSubjectsSubjectName SubjectName
        {
            get
            {
                return this.subjectNameField;
            }
            set
            {
                this.subjectNameField = value;
            }
        }

        /// <remarks/>
        public RootFooterInterconnectedSubjectsSubjectRNN SubjectRNN
        {
            get
            {
                return this.subjectRNNField;
            }
            set
            {
                this.subjectRNNField = value;
            }
        }

        /// <remarks/>
        public RootFooterInterconnectedSubjectsSubjectIIN SubjectIIN
        {
            get
            {
                return this.subjectIINField;
            }
            set
            {
                this.subjectIINField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInterconnectedSubjectsTypeOfLink
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInterconnectedSubjectsSubjectCode
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInterconnectedSubjectsSubjectName
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInterconnectedSubjectsSubjectRNN
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInterconnectedSubjectsSubjectIIN
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTitleSummaryInformation
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterContract
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterSubjectDetails2
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterAmountOverdue
    {

        private string stitleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string stitle
        {
            get
            {
                return this.stitleField;
            }
            set
            {
                this.stitleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterMaxOverdueAmount
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterMaxOverdueCount
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterPaymentsCalendar
    {

        private RootFooterPaymentsCalendarDescription1 description1Field;

        private RootFooterPaymentsCalendarDescription2 description2Field;

        private string titleField;

        /// <remarks/>
        public RootFooterPaymentsCalendarDescription1 Description1
        {
            get
            {
                return this.description1Field;
            }
            set
            {
                this.description1Field = value;
            }
        }

        /// <remarks/>
        public RootFooterPaymentsCalendarDescription2 Description2
        {
            get
            {
                return this.description2Field;
            }
            set
            {
                this.description2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterPaymentsCalendarDescription1
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterPaymentsCalendarDescription2
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoSubjects
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterNoViolations
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInformationFromDB
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInformationFromPS
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInformationFromPSS
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInformationFromPSS3
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInformationFromPCR
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterInformationFromGDB
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTranches
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterTestWarning
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterAdvCC
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootFooterIncludingRehabilitation
    {

        private string titleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
    }
}
