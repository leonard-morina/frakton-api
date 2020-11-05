using System;

namespace Frakton.Models
{
    public class FavoriteCryptoCoin
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CryptoCoinId { get; set; }
        public DateTime InsertedOn { get; set; }
    }
}