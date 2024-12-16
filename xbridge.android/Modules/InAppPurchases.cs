using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Newtonsoft.Json.Linq;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using xbridge.Util;
using Plugin.CurrentActivity;

namespace xbridge.android.Modules
{
    /*public class PurchaseActivity: Activity
    {
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }*/

    internal class PurchaseData
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "data")]
        public string Data;
        [Newtonsoft.Json.JsonProperty(PropertyName = "signature")]
        public string Signature;
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string ID;
        [Newtonsoft.Json.JsonProperty(PropertyName = "transaction")]
        public string Transaction;
        [Newtonsoft.Json.JsonProperty(PropertyName = "token")]
        public string Token;
    }

    public class Product
    {

        public Product(InAppBillingProduct p)
        {
            Name = p.Name;
            Description = p.Description;
            ID = p.ProductId;
            Price = p.LocalizedPrice;
            Currency = p.CurrencyCode;
            MicrosPrice = p.MicrosPrice;
        }
        /// <summary>
        /// Name of the product
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the product
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Product ID or sku
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// Localized Price (not including tax)
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "price")]
        public string Price { get; set; }

        /// <summary>
        /// ISO 4217 currency code for price. For example, if price is specified in British pounds sterling is "GBP".
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Price in micro-units, where 1,000,000 micro-units equal one unit of the 
        /// currency. For example, if price is "€7.99", price_amount_micros is "7990000". 
        /// This value represents the localized, rounded price for a particular currency.
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "microsPrice")]
        public Int64 MicrosPrice { get; set; }
    }

    internal class Verifier : IInAppBillingVerifyPurchase
    {
        internal List<PurchaseData> data = new List<PurchaseData>();

        public async Task<bool> VerifyPurchase(string signedData, string signature, string productId = null, string transactionId = null)
        {
            data.Add(new PurchaseData
            {
                Data = signedData,
                Signature = signature,
                ID = productId,
                Transaction = transactionId
            });
            return true;
        }
    }

    public class InAppPurchases
    {
        Random rand = new Random();
        public InAppPurchases(XBridge xbridge)
        {
            this.Bridge = xbridge;
            //this.Activity = new PurchaseActivity();
        }

        internal readonly XBridge Bridge;
        //private readonly PurchaseActivity Activity;

        public Task<object> Purchase(string product, string payload)
        {
            return DoPurchase(product, payload, false);
        }

        private Task<object> DoPurchase(string product, string payload, bool isSubscription)
        {
            return RunTransAction(new Func<Task<object>>(async () =>
            {
                var itemType = isSubscription ? Plugin.InAppBilling.Abstractions.ItemType.Subscription : Plugin.InAppBilling.Abstractions.ItemType.InAppPurchase;
                if(payload == null)
                    payload = System.Guid.NewGuid().ToString();
                var verifier = new Verifier();
                try
                {
                    var p = await CrossInAppBilling.Current.PurchaseAsync(product, itemType, payload, verifier);
                    if (p == null)
                        return false;
                    if (p.State == Plugin.InAppBilling.Abstractions.PurchaseState.Purchased)
                    {
                        var data = verifier.data[0];
                        data.ID = p.PurchaseToken.Split(":")[2];
                        data.Token = p.PurchaseToken;
                        return verifier.data;
                    }
                }
                catch (InAppBillingPurchaseException ex)
                {
                    var error = ex.PurchaseError;
                    throw new Exception(error.ToString());
                }
                return false;
            }));
        }

        public Task<object> Subscribe(string productId, string payload)
        {
            return DoPurchase(productId, payload, true);
        }

        public Task<object> Purchases()
        {
            return DoGetPurchases(false);

        }

        public async Task<object> Product(string product)
        {
            return (await Products(new string[] { product }) as IEnumerable<object>).First();
        }

        public async Task<object> Subscribable(string subscription)
        {
            return (await Subscribables(new string [] { subscription }) as IEnumerable<object>).First();
        }

        public Task<object> Subscribables(string[] ids)
        {
            return DoGetInfos(ids, true);
        }
        public Task<object> Products(string[] ids)
        {
            return DoGetInfos(ids, false);
        }

        private Task<object> DoGetInfos(string[] ids, bool subscriptions)
        {
            return RunTransAction(new Func<Task<object>>(async () =>
            {
                var payload = System.Guid.NewGuid().ToString() + rand.Next();
                var verifier = new Verifier();
                try
                {
                    var infos = await CrossInAppBilling.Current.GetProductInfoAsync(subscriptions ? ItemType.Subscription : ItemType.InAppPurchase, ids);
                    if (infos == null)
                    {
                        return new object[0];
                    }
                    return ids.Select((id) => {
                        Product result = null;
                        infos.Each((found, index) => {
                            Console.WriteLine(found.ProductId);
                            if(found.ProductId == id)
                            {
                                found.Name = Regex.Replace(found.Name, "\\s+\\([^)]+\\)$", "");
                                result = new Product(found);
                                //return false;
                            }
                            //return true;
                        });

                        return result;
                    });
                }
                catch (InAppBillingPurchaseException ex)
                {
                    var error = ex.PurchaseError;
                    throw new Exception(error.ToString());
                }
            }));
        }

        private Task<object> DoGetPurchases(bool subscriptions)
        {
            return RunTransAction(new Func<Task<object>>(async () =>
            {
                var payload = System.Guid.NewGuid().ToString() + rand.Next();
                var verifier = new Verifier();
                try
                {
                    var purchases = await CrossInAppBilling.Current.GetPurchasesAsync(subscriptions ? ItemType.Subscription : ItemType.InAppPurchase, verifier);
                    if (purchases == null)
                        return new object[0];
                    purchases.Each((p, i) =>
                    {
                        var data = verifier.data[i];
                        data.ID = p.PurchaseToken.Split(":")[2];
                        data.Token = p.PurchaseToken;
                    });
                    return verifier.data;
                }
                catch (InAppBillingPurchaseException ex)
                {
                    var error = ex.PurchaseError;
                    throw new Exception(error.ToString());
                }
            }));
        }

        public Task<object> Subscriptions()
        {
            return DoGetPurchases(true);
        }

        public Task<object> Consume(string productId, string token)
        {
            return RunTransAction(new Func<Task<object>>(async () =>
            {
                var payload = System.Guid.NewGuid().ToString() + rand.Next();
                var verifier = new Verifier();
                try
                {
                    var purchase = await CrossInAppBilling.Current.ConsumePurchaseAsync(productId, token);
                    if (purchase == null)
                        return false;
                    return true;
                }
                catch (InAppBillingPurchaseException ex)
                {
                    var error = ex.PurchaseError;
                    throw new Exception(error.ToString());
                }
            }));
        }

        private async Task<object> RunTransAction(Func<Task<object>> action)
        {
            try
            {
                var connected = await CrossInAppBilling.Current.ConnectAsync(Plugin.InAppBilling.Abstractions.ItemType.InAppPurchase);
                if (!connected)
                    throw new Exception("could not connect to google play");
                return await action();
            }
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }
        }





    }
}
