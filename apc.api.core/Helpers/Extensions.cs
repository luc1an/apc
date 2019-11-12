using apc.bussinesslayer.core;
using apc.bussinesslayer.core.models;

namespace apc.api.core.Helpers
{
    public static class MyExtensions
    {
       public static AmazonPriceModel ToPriceModel(this Product product)
        {
            var price = new AmazonPriceModel
            {
                Currency = product.Currency,
                SiteName = $"AMAZON {product.AmazonSite.Name}",
                ProductLink = product.ProductLink,
                ProductName = product.ProductName,
                
                Valid = product.Valid,
                NetPrice = product.Netto,
                MoneyValue = product.MoneyValue,
                MoneyAmt = product.MoneyAmt,
                
                Vat = product.AmazonSite.Vat,
                VatAmount = product.VatAmount
            };
            return price;
        }
    }
}