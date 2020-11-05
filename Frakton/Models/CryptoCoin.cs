namespace Frakton.Models
{
    public class CryptoCoin
    {
        public string Id { get; set; }
        public string Rank { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Supply { get; set; }
        public decimal? MaxSupply { get; set; }
    }
}
