using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using apc.bussinesslayer.core;
using apc.api.core.Helpers;
using apc.bussinesslayer.core.models;
using Microsoft.AspNetCore.Hosting;
using RestSharp;

namespace apc.api.core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApcController : ControllerBase
    {
        [HttpGet]
        [Route("[action]")]
        public string Asin(string link) => Helper.Detect(link);

        private IWebHostEnvironment _env;
        
        public  ApcController (IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        [Route("{asin}/{currency}/[action]")]
        public IEnumerable<AmazonPriceModel> Price(string asin, string currency)
        {
            try
            {
                var webRoot = _env.ContentRootPath;
                var amazonPrices = Site.GetPrices(asin, webRoot).Select(c=>c.ToPriceModel()).ToList();
                // get the currencies, then the exchange.
                var distinctCurrencies = amazonPrices.Select(c => c.Currency).Distinct();
                // ok, i have the currency list. what's the exchange rate for each one ?
                var xchList = distinctCurrencies.Select(xx => GetCurrencyExchange(currency, xx)).ToList();
                // now, for every currency in the list, set the proper value.
                foreach (var price in amazonPrices)
                {
                    var xch = xchList.FirstOrDefault(c => c.Currency == price.Currency);
                    if (xch != null)
                    {
                        price.PriceInRequestedCurrency = Math.Round(price.MoneyValue * (decimal)xch.Value, 2);
                        price.VatAmountInRequestedCurrency = Math.Round(price.VatAmount * (decimal) xch.Value,2);
                        price.NetPriceInRequestedCurrency = Math.Round(price.NetPrice * (decimal) xch.Value, 2);
                    }
                }
                return amazonPrices.OrderBy(c=>c.NetPriceInRequestedCurrency);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return null;
        }


        public double GetExchangeRest(string currency)
        {
            if (currency == "RON") return 1;
            var client = new RestClient("http://infovalutar.ro/bnr/azi/");
            var request = new RestRequest(currency, Method.GET);
            var response = client.Execute(request);
            var content = response.Content;
            try
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<double>(content);
                return data;
            }
            catch
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("exchange")]
        public CurrencyExchangeModel GetCurrencyExchange(string baseCurrency, string currency)
        {
            if (baseCurrency == "RON")
            {

                var xe = new CurrencyExchangeModel
                {
                    Currency = currency,
                    BaseCurrency = baseCurrency,
                    Value = currency switch
                    {
                        "RON" => 1,
                        "AUD" => GetExchangeRest(currency),
                        "EUR" => GetExchangeRest(currency),
                        "USD" => GetExchangeRest(currency),
                        "JPY" => GetExchangeRest(currency),
                        "GBP" => GetExchangeRest(currency),
                        "CDN" => GetExchangeRest(currency),
                        "CAD" => GetExchangeRest(currency),
                        _ => -1
                    }
                };

                return xe;
                //retD.Add(xe.Currency, xe.ExchangeRate);
            }
            else
            {
                // get all from BCE except RON currency
                var xe = new CurrencyExchangeModel { Currency = currency, BaseCurrency = baseCurrency };
                var currencyValue = GetExchangeRest(baseCurrency);
                var acc2 = GetExchangeRest(currency);
                xe.Value = acc2 / currencyValue;
                return xe;
            }
        }

        [HttpPost]
        [Route("exchangelist")]
        public List<CurrencyExchangeModel> Exchange([FromBody]CurrencyModel model)
        {
            
            // RON // USD 
            if (model == null) return null;
            var ret = new List<CurrencyExchangeModel>();
            var currencies = model.Currencies.Split(',');


            foreach (var s in currencies)
            {
                var c = GetCurrencyExchange(model.Currency, s);
                ret.Add(c);
            }

            return ret;
        }

        //https://euvatrates.com/rates.json
    }
}