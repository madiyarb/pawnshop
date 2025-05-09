namespace Pawnshop.Services.Estimation.Request
{
    public sealed class LicenseGallery
    {
        public string Collection_name { get; set; }
        public string Path { get; set; }
        public string Action { get; set; }
        public MetaData Meta_data { get; set; }

        public LicenseGallery(string path, string title, int number)
        {
            Action = "add";
            Collection_name = "gallery";
            Path = path;
            Meta_data = new MetaData
            {
                Name = title,
                Number = number
            };

        }
    }
}
