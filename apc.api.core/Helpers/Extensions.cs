using apc.businesslayer.core;
using apc.businesslayer.core.models;

namespace apc.api.core.Helpers
{
    public static class MyExtensions
    {
       

        public static AmazonPriceModel ToPriceModel(this AmazonSite prc)
        {
            var price = new AmazonPriceModel
            {
                Currency = prc.Currency,
                Name = prc.Name,
                ProductLink = prc.ProductLink,
                ProductName = prc.ProductName,
                
                Valid = prc.Valid,
                NetPrice = prc.Netto,
                MoneyValue = prc.MoneyValue,
                MoneyAmt = prc.MoneyAmt,
                
                Vat = prc.VAT,
                VatAmount = prc.VATAmount
            };
            return price;
        }
    }
}