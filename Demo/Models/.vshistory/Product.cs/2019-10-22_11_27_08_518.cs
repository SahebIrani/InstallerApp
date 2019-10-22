namespace Demo.Models
{
    public class Product
    {
        public string FileNamePath => $"/imgs/product/{FileName}";

        public int Id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
    }
}
