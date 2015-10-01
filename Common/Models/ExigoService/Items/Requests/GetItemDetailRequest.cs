namespace ExigoService
{
    public class GetItemDetailRequest
    {
        public IOrderConfiguration Configuration { get; set; }
        public string ItemCode { get; set; }
    }
}