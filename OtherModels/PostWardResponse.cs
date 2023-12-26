namespace TechShopBackendDotnet.OtherModels
{
    public class PostWardResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<DataWard> data { get; set; }
    }

    public class DataWard
    {
        public string WardCode { get; set; }
        public int DistrictID { get; set; }
        public string WardName { get; set; }
        public List<string> NameExtension { get; set; }
        public bool CanUpdateCOD { get; set; }
        public int SupportType { get; set; }
        public int PickType { get; set; }
        public int DeliverType { get; set; }
        public WhiteListClient WhiteListClient { get; set; }
        public WhiteListWard WhiteListWard { get; set; }
        public int Status { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonMessage { get; set; }
        public object OnDates { get; set; } 
        public string CreatedIP { get; set; }
        public int CreatedEmployee { get; set; }
        public string CreatedSource { get; set; }
        public string CreatedDate { get; set; } 
        public int UpdatedEmployee { get; set; }
        public string UpdatedDate { get; set; } 
    }

    public class WhiteListClient
    {
        public List<int> From { get; set; }
        public List<int> To { get; set; }
        public List<int> Return { get; set; }
    }

    public class WhiteListWard
    {
        public object From { get; set; } 
        public object To { get; set; } 
    }

}
