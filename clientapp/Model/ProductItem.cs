using System;

namespace clientapp.Model
{
    public class ProductItem
    {
        public int Id { get; set; }

        public string Name { get; set; }        

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }        
    }
}
