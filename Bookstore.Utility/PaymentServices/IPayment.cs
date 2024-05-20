using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Utility.PaymentServices
{
    public class PaymentService
    {
        protected string ApiURL { get; }
        public virtual string GetSignature() { return ""; }
        public virtual string GetContentBody() { return ""; }
        protected readonly string tokenKey;
        protected readonly string checksumKey;
        public double amount { get; set; }
        public string orderCode { get; set; }
        public string orderDescription { get; set; }
        public int totalItem { get; set; }
        public int checkoutType { get; set; }
        public int installment {  get; set; }
        public string returnUrl { get; set; }
        public string cancelUrl { get; set; }
        public string buyerName { get; set; }
        public string buyerEmail { get; set; }
        public string buyerPhone { get; set; }
        public string buyerAddress { get; set; }
        public string buyerCity { get; set; }
        public string buyerCountry { get; set; }
        protected readonly string currency = "VND";
        public string customMerchantId { get; set; }
        protected bool allowDomestic;
        public PaymentService()
        {
            var keys = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AlePayKeys");
            tokenKey = keys["TokenKey"];
            checksumKey = keys["ChecksumKey"];
        }

        public PaymentService(int totalItem,
        double amount,
        string buyerAddress,
        string buyerCity,
        string buyerCountry,
        string buyerEmail,
        string buyerName,
        string buyerPhone,
        string orderCode,
        string orderDescription,
        string customMerchantId,
        string returnUrl,
        string cancelUrl)
        {
            this.totalItem = totalItem;
            this.amount = amount;
            this.buyerAddress = buyerAddress;
            this.buyerCity = buyerCity;
            this.buyerCountry = buyerCountry;
            this.buyerEmail = buyerEmail;
            this.buyerName = buyerName;
            this.buyerPhone = buyerPhone;
            this.orderDescription = orderDescription;
            this.customMerchantId = customMerchantId;
            this.returnUrl = returnUrl;
            this.cancelUrl = cancelUrl;
            this.orderCode = orderCode;
            var keys = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AlePayKeys");
            tokenKey = keys["TokenKey"];
            checksumKey = keys["ChecksumKey"];
        }

        public static string CalculateHmacSHA256(string message, string secretKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }

    public class InternationalCardPayment : PaymentService
    {
        private string ApiURL => "https://alepay-v3-sandbox.nganluong.vn/api/v3/checkout/request-payment";
        public InternationalCardPayment(PaymentService payment)
    : base(payment.totalItem, payment.amount, payment.buyerAddress,
           payment.buyerCity, payment.buyerCountry, payment.buyerEmail,
           payment.buyerName, payment.buyerPhone, payment.orderCode, payment.orderDescription,
           payment.customMerchantId, payment.returnUrl, payment.cancelUrl)
        {

        }

        public override string GetSignature()
        {
            string data = $"amount={amount}&buyerAddress={buyerAddress}&" +
                $"buyerCity={buyerCity}&buyerCountry={buyerCountry}&buyerEmail={buyerEmail}&buyerName={buyerName}&" +
                $"buyerPhone={buyerPhone}&cancelUrl={cancelUrl}&checkoutType={checkoutType}&" +
                $"currency={currency}&customMerchantId={customMerchantId}&language=en&orderCode={orderCode}&" +
                $"orderDescription={orderDescription}&paymentHours=5&returnUrl={returnUrl}&tokenKey={tokenKey}&totalItem={totalItem}";

            return CalculateHmacSHA256(data, checksumKey);
        }

        public override string GetContentBody()
        {
            var jsonToSend = @"{
                ""amount"": " + amount + @",
                ""buyerAddress"": """ + buyerAddress + @""",
                ""buyerCity"": """ + buyerCity + @""",
                ""buyerCountry"": """ + buyerCountry + @""",
                ""buyerEmail"": """ + buyerEmail + @""",
                ""buyerName"": """ + buyerName + @""",
                ""buyerPhone"": """ + buyerPhone + @""",
                ""cancelUrl"": """ + cancelUrl + @""",
                ""checkoutType"": "+ checkoutType + @",
                ""currency"": """ + currency + @""",
                ""customMerchantId"": """ + customMerchantId + @""",
                ""language"": ""en"",
                ""orderCode"": """ + orderCode + @""",
                ""orderDescription"": """ + orderDescription + @""",
                ""paymentHours"":""5"",
                ""returnUrl"": """ + returnUrl + @""",
                ""tokenKey"": """ + tokenKey + @""",
                ""totalItem"": " + totalItem + @",
                ""signature"": """ + GetSignature() + @"""
            }";

            return jsonToSend;
        }

        public async Task<Tuple<bool, string>> SendRequest()
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(GetContentBody(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(ApiURL, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    if (jsonResponse.checkoutUrl != null)
                    {
                        return new Tuple<bool, string>(true, jsonResponse.checkoutUrl.ToString());
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, jsonResponse.message.ToString());
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "Can't access payment page. Please try later.");
                }
            }
        }
    }

    public class DomesticCardPayment : PaymentService
    {
        private string ApiURL => "https://alepay-v3-sandbox.nganluong.vn/api/v3/checkout/request-payment";
        public DomesticCardPayment(PaymentService payment)
    : base(payment.totalItem, payment.amount, payment.buyerAddress,
           payment.buyerCity, payment.buyerCountry, payment.buyerEmail,
           payment.buyerName, payment.buyerPhone, payment.orderCode, payment.orderDescription,
           payment.customMerchantId, payment.returnUrl, payment.cancelUrl)
        {

        }

        public override string GetSignature()
        {
            string data = $"allowDomestic=true&amount={amount}&buyerAddress={buyerAddress}&" +
                $"buyerCity={buyerCity}&buyerCountry={buyerCountry}&buyerEmail={buyerEmail}&buyerName={buyerName}&" +
                $"buyerPhone={buyerPhone}&cancelUrl={cancelUrl}&checkoutType={checkoutType}&" +
                $"currency={currency}&customMerchantId={customMerchantId}&language=en&orderCode={orderCode}&" +
                $"orderDescription={orderDescription}&paymentHours=5&returnUrl={returnUrl}&tokenKey={tokenKey}&totalItem={totalItem}";

            return CalculateHmacSHA256(data, checksumKey);
        }

        public override string GetContentBody()
        {
            var jsonToSend = @"{
                ""allowDomestic"": true,
                ""amount"": " + amount + @",
                ""buyerAddress"": """ + buyerAddress + @""",
                ""buyerCity"": """ + buyerCity + @""",
                ""buyerCountry"": """ + buyerCountry + @""",
                ""buyerEmail"": """ + buyerEmail + @""",
                ""buyerName"": """ + buyerName + @""",
                ""buyerPhone"": """ + buyerPhone + @""",
                ""cancelUrl"": """ + cancelUrl + @""",
                ""checkoutType"": " + checkoutType + @",
                ""currency"": """ + currency + @""",
                ""customMerchantId"": """ + customMerchantId + @""",
                ""language"": ""en"",
                ""orderCode"": """ + orderCode + @""",
                ""orderDescription"": """ + orderDescription + @""",
                ""paymentHours"":""5"",
                ""returnUrl"": """ + returnUrl + @""",
                ""tokenKey"": """ + tokenKey + @""",
                ""totalItem"": " + totalItem + @",
                ""signature"": """ + GetSignature() + @"""
            }";

            return jsonToSend;
        }

        public async Task<Tuple<bool, string>> SendRequest()
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(GetContentBody(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(ApiURL, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    if (jsonResponse.checkoutUrl != null)
                    {
                        return new Tuple<bool, string>(true, jsonResponse.checkoutUrl.ToString());
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, jsonResponse.message.ToString());
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "Can't access payment page. Please try later.");
                }
            }
        }
    }

    public class InstallmentPayment : PaymentService
    {
        private string ApiURL => "https://alepay-v3-sandbox.nganluong.vn/api/v3/checkout/request-payment";
        public InstallmentPayment(PaymentService payment)
    : base(payment.totalItem, payment.amount, payment.buyerAddress,
           payment.buyerCity, payment.buyerCountry, payment.buyerEmail,
           payment.buyerName, payment.buyerPhone, payment.orderCode, payment.orderDescription,
           payment.customMerchantId, payment.returnUrl, payment.cancelUrl)
        {

        }

        public override string GetSignature()
        {
            string data = $"amount={amount}&buyerAddress={buyerAddress}&" +
                $"buyerCity={buyerCity}&buyerCountry={buyerCountry}&buyerEmail={buyerEmail}&buyerName={buyerName}&" +
                $"buyerPhone={buyerPhone}&cancelUrl={cancelUrl}&checkoutType={checkoutType}&" +
                $"currency={currency}&customMerchantId={customMerchantId}&language=en&orderCode={orderCode}&" +
                $"orderDescription={orderDescription}&paymentHours=5&returnUrl={returnUrl}&tokenKey={tokenKey}&totalItem={totalItem}";

            return CalculateHmacSHA256(data, checksumKey);
        }

        public override string GetContentBody()
        {
            var jsonToSend = @"{
                ""amount"": " + amount + @",
                ""buyerAddress"": """ + buyerAddress + @""",
                ""buyerCity"": """ + buyerCity + @""",
                ""buyerCountry"": """ + buyerCountry + @""",
                ""buyerEmail"": """ + buyerEmail + @""",
                ""buyerName"": """ + buyerName + @""",
                ""buyerPhone"": """ + buyerPhone + @""",
                ""cancelUrl"": """ + cancelUrl + @""",
                ""checkoutType"": " + checkoutType + @",
                ""currency"": """ + currency + @""",
                ""customMerchantId"": """ + customMerchantId + @""",
                ""language"": ""en"",
                ""orderCode"": """ + orderCode + @""",
                ""orderDescription"": """ + orderDescription + @""",
                ""paymentHours"":""5"",
                ""returnUrl"": """ + returnUrl + @""",
                ""tokenKey"": """ + tokenKey + @""",
                ""totalItem"": " + totalItem + @",
                ""signature"": """ + GetSignature() + @"""
            }";

            return jsonToSend;
        }

        public async Task<Tuple<bool, string>> SendRequest()
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(GetContentBody(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(ApiURL, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    if (jsonResponse.checkoutUrl != null)
                    {
                        return new Tuple<bool, string>(true, jsonResponse.checkoutUrl.ToString());
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, jsonResponse.message.ToString());
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "Can't access payment page. Please try later.");
                }
            }
        }
    }

    //Tokenization

    public class CardLinkPayment : PaymentService
    {
        private string ApiURL => "https://alepay-v3-sandbox.nganluong.vn/api/v3/checkout/request-payment";
        public string buyerPostalCode { get; set; }
        public string buyerState { get; set; }
        public CardLinkPayment(PaymentService payment)
    : base(payment.totalItem, payment.amount, payment.buyerAddress,
           payment.buyerCity, payment.buyerCountry, payment.buyerEmail,
           payment.buyerName, payment.buyerPhone, payment.orderCode, payment.orderDescription,
           payment.customMerchantId, payment.returnUrl, payment.cancelUrl)
        {

        }

        public override string GetSignature()
        {
            string data = $"amount={amount}&buyerAddress={buyerAddress}&" +
                $"buyerCity={buyerCity}&buyerCountry={buyerCountry}&buyerEmail={buyerEmail}&buyerName={buyerName}&" +
                $"buyerPhone={buyerPhone}&buyerPostalCode={buyerPostalCode}&buyerState={buyerState}&cancelUrl={cancelUrl}&" +
                $"currency={currency}&isCardLink=true&language=en&merchantSideUserId={customMerchantId}&orderCode={orderCode}&" +
                $"orderDescription={orderDescription}&paymentHours=5&returnUrl={returnUrl}&tokenKey={tokenKey}&totalItem={totalItem}";

            return CalculateHmacSHA256(data, checksumKey);
        }

        public override string GetContentBody()
        {
            var jsonToSend = @"{
                ""amount"": " + amount + @",
                ""buyerAddress"": """ + buyerAddress + @""",
                ""buyerCity"": """ + buyerCity + @""",
                ""buyerCountry"": """ + buyerCountry + @""",
                ""buyerEmail"": """ + buyerEmail + @""",
                ""buyerName"": """ + buyerName + @""",
                ""buyerPhone"": """ + buyerPhone + @""",
                ""buyerPostalCode"": """ + buyerPostalCode + @""",
                ""buyerState"": """ + buyerState + @""",
                ""cancelUrl"": """ + cancelUrl + @""",
                ""currency"": """ + currency + @""",
                ""isCardLink"": true,
                ""language"": ""en"",
                ""merchantSideUserId"": """ + customMerchantId + @""",
                ""orderCode"": """ + orderCode + @""",
                ""orderDescription"": """ + orderDescription + @""",
                ""paymentHours"":""5"",
                ""returnUrl"": """ + returnUrl + @""",
                ""tokenKey"": """ + tokenKey + @""",
                ""totalItem"": " + totalItem + @",
                ""signature"": """ + GetSignature() + @"""
            }";

            return jsonToSend;
        }

        public async Task<Tuple<bool, string>> SendRequest()
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(GetContentBody(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(ApiURL, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    if (jsonResponse.checkoutUrl != null)
                    {
                        return new Tuple<bool, string>(true, jsonResponse.checkoutUrl.ToString() + "\n" + jsonResponse.transactionCode.ToString());
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, jsonResponse.message.ToString());
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "Can't access payment page. Please try later.");
                }
            }
        }
    }

    public class TokenizationPayment : PaymentService
    {
        private string ApiURL => "https://alepay-v3-sandbox.nganluong.vn/api/v3/checkout/request-tokenization-payment";
        public string customerToken { get; set; }

        public TokenizationPayment() : base()
        {

        }
        public TokenizationPayment(PaymentService payment)
    : base(payment.totalItem, payment.amount, payment.buyerAddress,
           payment.buyerCity, payment.buyerCountry, payment.buyerEmail,
           payment.buyerName, payment.buyerPhone, payment.orderCode, payment.orderDescription,
           payment.customMerchantId, payment.returnUrl, payment.cancelUrl)
        {

        }

        public override string GetSignature()
        {
            string data = $"amount={amount}&cancelUrl={cancelUrl}&" +
                $"currency={currency}&customerToken={customerToken}&language=en&orderCode={orderCode}&" +
                $"orderDescription={orderDescription}&paymentHours=5&returnUrl={returnUrl}&tokenKey={tokenKey}";

            return CalculateHmacSHA256(data, checksumKey);
        }

        public override string GetContentBody()
        {
            var jsonToSend = @"{
                ""amount"": " + amount + @",
                ""cancelUrl"": """ + cancelUrl + @""",
                ""currency"": """ + currency + @""",
                ""customerToken"": """ + customerToken + @""",
                ""language"": ""en"",
                ""orderCode"": """ + orderCode + @""",
                ""orderDescription"": """ + orderDescription + @""",
                ""paymentHours"":""5"",
                ""returnUrl"": """ + returnUrl + @""",
                ""tokenKey"": """ + tokenKey + @""",
                ""signature"": """ + GetSignature() + @"""
            }";

            return jsonToSend;
        }

        public async Task<Tuple<bool, string>> SendRequest()
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(GetContentBody(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(ApiURL, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    if (jsonResponse.checkoutUrl != null)
                    {
                        return new Tuple<bool, string>(true, jsonResponse.checkoutUrl.ToString());
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, jsonResponse.message.ToString());
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "Can't access payment page. Please try later.");
                }
            }
        }
        public async Task<Tuple<bool, List<TokenizationInfo>>> GetLinkedCard(string customerId)
        {
            var httpClient = new HttpClient();
            string data = $"customerId={customerId}&language=en&tokenKey={tokenKey}";
            var tokenizationInfos = new List<TokenizationInfo>();

            var jsonToSend = @"{
                ""customerId"": """ + customerId + @""",
                ""language"": ""en"",
                ""tokenKey"": """ + tokenKey + @""",
                ""signature"": """ + CalculateHmacSHA256(data, checksumKey) + @"""
            }";
            var content = new StringContent(jsonToSend, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://alepay-v3-sandbox.nganluong.vn/api/v3/checkout/get-customer-info", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                if (responseObject.code == "000")
                {
                    
                    var listCardTokens = responseObject["listCardTokens"];
                    foreach (var token in listCardTokens)
                    {
                        var tokenizationInfo = new TokenizationInfo
                        {
                            tokenKey = token["token"].ToString(),
                            cardNumber = token["cardNumber"].ToString(),
                            cardHolderName = token["cardHolderName"].ToString(),
                            paymentmethod = token["paymentMethod"].ToString(),
                            bankCode = token["bankCode"].ToString()
                        };
                        tokenizationInfos.Add(tokenizationInfo);
                    }
                    return new Tuple<bool, List<TokenizationInfo>>(true, tokenizationInfos);
                }
                else
                {
                    return new Tuple<bool, List<TokenizationInfo>>(false, tokenizationInfos);
                }
            }
            else
            {
                return new Tuple<bool, List<TokenizationInfo>>(false, tokenizationInfos);
            }
        }
    }

    public class TokenizationInfo
    {
        public string tokenKey {  get; set; }
        public string cardNumber { get; set; }
        public string cardHolderName { get; set; }
        public string paymentmethod { get; set; }
        public string bankCode { get; set; }
    }

    public class TransactionService
    {
        private string ApiURL = "https://alepay-v3-sandbox.nganluong.vn/api/v3/checkout/get-transaction-info";
        private string TokenKey;
        private string ChecksumKey;
        public string TransactionCode { get; set; }

        public TransactionService()
        {
            var keys = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AlePayKeys");
            TokenKey = keys["TokenKey"];
            ChecksumKey = keys["ChecksumKey"];
        }

        private string GetSignature()
        {
            string data = $"tokenKey={TokenKey}&transactionCode={TransactionCode}";

            return PaymentService.CalculateHmacSHA256(data, ChecksumKey);
        }

        private string GetContentBody()
        {
            var jsonToSend = @"{
                ""tokenKey"": """ + TokenKey + @""",
                ""transactionCode"": """ + TransactionCode + @""",
                ""signature"": """ + GetSignature() + @"""
            }";

            return jsonToSend;
        }

        public async Task<Tuple<bool, string>> GetTransactionInfo()
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(GetContentBody(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(ApiURL, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                    if (jsonResponse.code.ToString() == "000")
                    {
                        return new Tuple<bool, string>(true, jsonResponse.ToString());
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, "");
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "");
                }
            }
        }
    }
}
