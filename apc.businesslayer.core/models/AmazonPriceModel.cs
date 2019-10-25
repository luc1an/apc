namespace apc.businesslayer.core.models
{
    public class AmazonPriceModel
    {
        public bool Valid { get; set; }
        public string ProductName { get; set; }
        public string ProductLink { get; set; }
        public string Name { get; set; }
        public decimal PriceInRequestedCurrency { get; set; }
        public decimal VatAmountInRequestedCurrency { get; set; }
        public decimal NetPriceInRequestedCurrency { get; set; }
        public decimal Vat { get; set; }
        public decimal NetPrice { get; set; }
        public decimal VatAmount { get; set; }
        public string MoneyAmt { get; set; }
        public decimal MoneyValue { get; set; }
        public string Currency { get; set; }
    }
}
