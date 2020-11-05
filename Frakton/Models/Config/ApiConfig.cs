namespace Frakton.Models.Config
{
    public class ApiConfig
    {
        public string Url { get; set; }
        public string Key { get; set; }
        public bool RequiresKey => Key.Length > 0; //not the ideal solution just for testing since the api has no key
    }
}