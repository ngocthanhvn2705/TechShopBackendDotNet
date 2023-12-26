namespace TechShopBackendDotnet.OtherModels
{
    public class PostDistrictResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<DataDistrict> data { get; set; }
    }

    public class DataDistrict
    {
        public int DistrictID { get; set; }
        public int ProvinceID { get; set; }
        public string DistrictName { get; set; }
        public string Code { get; set; }
        public int Type { get; set; }
        public int SupportType { get; set; }
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
