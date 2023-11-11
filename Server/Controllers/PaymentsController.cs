using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;

namespace Server.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IHubContext<PayHub> _hubContext;

    public PaymentsController(IHubContext<PayHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> Pay()
    {
        Options options = new()
        {
            ApiKey = "sandbox-jJ9iwVPKmLVPhHy9quhLMsdqvDLQY0J9",
            SecretKey = "sandbox-q4dk0SrgBiNf9mr2zCCU5PuHQwMYGxKv",
            BaseUrl = "https://sandbox-api.iyzipay.com"
        };

        CreatePaymentRequest request = new CreatePaymentRequest();
        request.Locale = Locale.TR.ToString();
        request.ConversationId = Guid.NewGuid().ToString();
        request.Price = "1";
        request.PaidPrice = "1.2";
        request.Currency = Currency.TRY.ToString();
        request.Installment = 1;
        request.BasketId = "B67832";
        request.PaymentChannel = PaymentChannel.WEB.ToString();
        request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
        request.CallbackUrl = "https://localhost:7224/api/Payments/PayCallBack";

        PaymentCard paymentCard = new PaymentCard();
        paymentCard.CardHolderName = "John Doe";
        paymentCard.CardNumber = "5528790000000008";
        paymentCard.ExpireMonth = "12";
        paymentCard.ExpireYear = "2030";
        paymentCard.Cvc = "123";
        paymentCard.RegisterCard = 0;
        request.PaymentCard = paymentCard;

        Buyer buyer = new Buyer();
        buyer.Id = "BY789";
        buyer.Name = "John";
        buyer.Surname = "Doe";
        buyer.GsmNumber = "+905350000000";
        buyer.Email = "email@email.com";
        buyer.IdentityNumber = "74300864791";
        buyer.LastLoginDate = "2015-10-05 12:43:35";
        buyer.RegistrationDate = "2013-04-21 15:12:09";
        buyer.RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
        buyer.Ip = "85.34.78.112";
        buyer.City = "Istanbul";
        buyer.Country = "Turkey";
        buyer.ZipCode = "34732";
        request.Buyer = buyer;

        Address shippingAddress = new Address();
        shippingAddress.ContactName = "Jane Doe";
        shippingAddress.City = "Istanbul";
        shippingAddress.Country = "Turkey";
        shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
        shippingAddress.ZipCode = "34742";
        request.ShippingAddress = shippingAddress;

        Address billingAddress = new Address();
        billingAddress.ContactName = "Jane Doe";
        billingAddress.City = "Istanbul";
        billingAddress.Country = "Turkey";
        billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
        billingAddress.ZipCode = "34742";
        request.BillingAddress = billingAddress;

        List<BasketItem> basketItems = new List<BasketItem>();
        BasketItem firstBasketItem = new BasketItem();
        firstBasketItem.Id = "BI101";
        firstBasketItem.Name = "Binocular";
        firstBasketItem.Category1 = "Collectibles";
        firstBasketItem.Category2 = "Accessories";
        firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
        firstBasketItem.Price = "0.3";
        basketItems.Add(firstBasketItem);

        BasketItem secondBasketItem = new BasketItem();
        secondBasketItem.Id = "BI102";
        secondBasketItem.Name = "Game code";
        secondBasketItem.Category1 = "Game";
        secondBasketItem.Category2 = "Online Game Items";
        secondBasketItem.ItemType = BasketItemType.VIRTUAL.ToString();
        secondBasketItem.Price = "0.5";
        basketItems.Add(secondBasketItem);

        BasketItem thirdBasketItem = new BasketItem();
        thirdBasketItem.Id = "BI103";
        thirdBasketItem.Name = "Usb";
        thirdBasketItem.Category1 = "Electronics";
        thirdBasketItem.Category2 = "Usb / Cable";
        thirdBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
        thirdBasketItem.Price = "0.2";
        basketItems.Add(thirdBasketItem);
        request.BasketItems = basketItems;

        ThreedsInitialize threedsInitialize = ThreedsInitialize.Create(request, options);

        return Ok(new { Content = threedsInitialize.HtmlContent, ConversationId = request.ConversationId });
    }

    [HttpPost]
    public async Task<IActionResult> PayCallBack([FromForm] IFormCollection collections)
    {
        CallbackData data = new(
            Status: collections["status"],
            PaymentId: collections["paymentId"],
            ConversationData: collections["conversationData"],
            ConversationId: collections["conversationId"],
            MDStatus: collections["mdStatus"]);

        if(data.Status != "success")
        {
            return BadRequest("Ödeme başarısız oldu!");
        }

        await _hubContext.Clients.Client(PayHub.TransactionConnections[data.ConversationId]).SendAsync("Receive", data);

        return Ok();
    }
}

public sealed record CallbackData(
    string Status,
    string PaymentId,
    string ConversationData,
    string ConversationId,
    string MDStatus);