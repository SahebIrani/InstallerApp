namespace Demo.Models
{
    public class Product
    {
        public string FileNamePath => $"/imgs/products/{FileName}";

        public int Id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
    }
}
