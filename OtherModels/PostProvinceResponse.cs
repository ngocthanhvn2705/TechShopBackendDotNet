namespace TechShopBackendDotnet.OtherModels
{
    public class PostProvinceResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<DataProvince> data { get; set; }

    }

    public class DataProvince
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public int CountryID { get; set; }
        public string Code { get; set; }
        public List<string> NameExtension { get; set; }
        public int IsEnable { get; set; }
        public int RegionID { get; set; }
        public int RegionCPN { get; set; }
        public int UpdatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public bool CanUpdateCOD { get; set; }
        public int Status { get; set; }
    }
}
