using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Bookstore.Models.ViewModel;
using Bookstore.Utility;
using System;
using System.Security.Claims;
using SmartBreadcrumbs.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Security.Cryptography;
using Bookstore.Utility.PaymentServices;
using System.Diagnostics.Metrics;
using System.IO;
using Bookstore.DataAccess.Repository;

namespace MyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

		[Breadcrumb(Title = "Cart")]
		public async Task<IActionResult> Index()
        {
            var claimedIdentity = (ClaimsIdentity?)User.Identity;
            var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shoppingCartViewModel = new ShoppingCartViewModel()
            {
                ShoppingCarts = await _unitOfWork.ShoppingCartRepo.GetAllAsync(u => u.ApplicationUserId == userId,
                IncludeProperties: "Book,Book.Category"),
            };

            return View(shoppingCartViewModel);
        }

		[Breadcrumb(FromAction = "Index", Title = "Checkout")]
		public async Task<IActionResult> Checkout()
        {
			var claimedIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var user = await _unitOfWork.ApplicationUserRepo.GetAsync(u => u.Id == userId);
			string jsonData = Request.Cookies["jsonData"];

			var viewModel = new OrderViewModel()
            {
				ShipEmail = user.Email,
				ShipPhoneNumber = user.PhoneNumber,
				ShipName = user.Name,
            };

            try
            {
                JObject jsonObject = JObject.Parse(jsonData);

                foreach (var property in jsonObject)
                {
                    JArray valueArray = (JArray)property.Value;

                    int bookId = valueArray[0].Value<int>();
                    int count = valueArray[1].Value<int>();

                    ShoppingCart shoppingCart = new()
                    {
                        BookId = bookId,
                        Book = await _unitOfWork.BookRepo.GetAsync(b => b.BookId == bookId),
                        ApplicationUserId = userId,
                        Count = count
                    };
                    viewModel.ChosenBooks.Add(shoppingCart);
                }

                var payment = new TokenizationPayment();
                var linkedCardResult = await payment.GetLinkedCard(userId);
                viewModel.LinkedCard = linkedCardResult.Item2;

                return View(viewModel);
            } catch 
            {
                return View("Error");
            }

			
		}

		[HttpPost]
        public async Task<IActionResult> Checkout(OrderViewModel orderViewModel)
		{
            if (ModelState.IsValid)
            {
                var claimedIdentity = (ClaimsIdentity?)User.Identity;
                var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _unitOfWork.ApplicationUserRepo.GetAsync(u => u.Id == userId);

                JObject jsonObject = JObject.Parse(Request.Cookies["jsonData"]);

                foreach (var property in jsonObject)
                {
                    JArray valueArray = (JArray)property.Value;

                    int bookId = valueArray[0].Value<int>();
                    int count = valueArray[1].Value<int>();

                    ShoppingCart shoppingCart = new()
                    {
                        BookId = bookId,
                        Book = await _unitOfWork.BookRepo.GetAsync(b => b.BookId == bookId),
                        ApplicationUserId = userId,
                        Count = count
                    };
                    orderViewModel.ChosenBooks.Add(shoppingCart);
                }

                //Add Order Info to database
                var orderHeader = new OrderHeader()
                {
                    OrderDate = DateTime.Now,
                    ApplicationUserId = userId,
                    ShipName = orderViewModel.ShipName,
                    ShipAddress = orderViewModel.ShipAddress,
                    ShipCity = user.City,
                    ShipCountry = user.Country,
                    ShipEmail = orderViewModel.ShipEmail,
                    PostalCode = user.PostalCode,
                    ShipState = user.State,
                    OrderTotal = orderViewModel.GetOrderTotal(),
                    PaymentStatus = StaticDetails.PaymentStatus_Pending,
                    OrderStatus = StaticDetails.Status_Pending,
                    Description = orderViewModel.Description,
                    ShipPhoneNumber = orderViewModel.ShipPhoneNumber
                };

                await _unitOfWork.OrderHeaderRepo.AddAsync(orderHeader);
                await _unitOfWork.SaveAsync();
                foreach (var cart in orderViewModel.ChosenBooks)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        BookId = cart.BookId,
                        OrderHeaderId = orderHeader.Id,
                        Price = cart.Book.DiscountPrice,
                        Count = cart.Count
                    };
                    await _unitOfWork.OrderDetailRepo.AddAsync(orderDetail);
                    await _unitOfWork.SaveAsync();
                }

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                Tuple<bool, string> result;
                PaymentService payment = new()
                {
                    totalItem = orderViewModel.ChosenBooks.Count(),
                    amount = (int)orderViewModel.GetOrderTotal() * 24640,
                    buyerAddress = orderViewModel.ShipAddress,
                    buyerCity = user.City,
                    buyerCountry = user.Country,
                    buyerEmail = orderViewModel.ShipEmail,
                    buyerName = orderViewModel.ShipName,
                    buyerPhone = orderViewModel.ShipPhoneNumber,
                    orderCode = orderHeader.Id.ToString(),
                    orderDescription = orderViewModel.Description,
                    customMerchantId = userId,
                    returnUrl = domain + $"customer/cart/OrderConfirmation",
                    cancelUrl = domain + "customer/cart/index"
                };

                if (orderViewModel.PaymentMethod == 0)
                {
                    if (orderViewModel.IsCardLink)
                    {
                        var payment1 = new CardLinkPayment(payment);
                        payment1.buyerPostalCode = user.PostalCode;
                        payment1.buyerState = user.State;
                        result = await payment1.SendRequest();
                    }
                    else
                    {
                        var payment2 = new InternationalCardPayment(payment);
                        result = await payment2.SendRequest();
                    }
                }
                else if (orderViewModel.PaymentMethod == 1)
                {
                    var payment3 = new DomesticCardPayment(payment);
                    payment3.checkoutType = 4;
                    result = await payment3.SendRequest();
                }
                else if (orderViewModel.PaymentMethod == 2)
                {
                    var payment4 = new InstallmentPayment(payment);
                    payment4.checkoutType = 2;
                    result = await payment4.SendRequest();
                }
                else
                {
                    var payment5 = new TokenizationPayment(payment);
                    payment5.customerToken = orderViewModel.ChosenCardToken;
                    result = await payment5.SendRequest();
                }

                if (result.Item1 == true)
                {
                    if (orderViewModel.PaymentMethod == 0 && orderViewModel.IsCardLink)
                    {
                        var values = result.Item2.Split("\n");
                        TempData["transactionCode"] = values[1];
                        return Redirect(values[0]);
                    }
                    return Redirect(result.Item2);
                }
                else
                {
                    TempData["error"] = result.Item2;
                    return RedirectToAction("Checkout");
                }
            }
            return View(orderViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(string? transactionCode, string errorCode, string cancel)
        {
            //Nếu error code không hợp lệ
            try
            {
				transactionCode ??= TempData["transactionCode"].ToString();
				if (errorCode.Length != 3 || transactionCode.ToUpper().Length != 9 || (cancel != "true" && cancel != "false"))
                    return RedirectToAction("Index");

                if (!int.TryParse(errorCode, out int code) || (errorCode != "000" && errorCode != "999" && (code < 101 || code > 217)))
                    return RedirectToAction("Index");

				var transactionService = new TransactionService
				{
					TransactionCode = transactionCode
				};
				var getTransactionRequest = await transactionService.GetTransactionInfo();
                if (getTransactionRequest.Item1 == false)
                {
                    return RedirectToAction("Index");
                }

                var claimedIdentity = (ClaimsIdentity?)User.Identity;
                var userId = claimedIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                dynamic parsedObject = JsonConvert.DeserializeObject(getTransactionRequest.Item2);
                string orderCode = parsedObject.orderCode.ToString();
                int orderHeaderId = int.Parse(orderCode);
                var order = await _unitOfWork.OrderHeaderRepo.GetAsync(o => o.Id == orderHeaderId, IncludeProperties: "ApplicationUser");
                if (order.ApplicationUser.Id != userId)
                    return RedirectToAction("Index");
                if (cancel == "false")
                {
                    _unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeaderId, StaticDetails.Status_Approved, paymentStatus: StaticDetails.PaymentStatus_Approved);
                }
                else
                {
                    _unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeaderId, StaticDetails.PaymentStatus_Rejected, paymentStatus: StaticDetails.PaymentStatus_Rejected);
                }
                await _unitOfWork.SaveAsync();

                bool isMonthDigit = int.TryParse(parsedObject.month.ToString(), out int month);

                var transaction = new Transaction()
                {
                    TransactionCode = parsedObject.transactionCode,
                    ErrorCode = parsedObject.code,
                    ErrorMessage = parsedObject.message,
                    OrderCode = orderHeaderId,
                    Amount = parsedObject.amount != null ? Convert.ToDouble(parsedObject.amount) : null,
                    Currency = parsedObject.currency,
                    BuyerEmail = parsedObject.buyerEmail,
                    BuyerPhone = parsedObject.buyerPhone,
                    CardNumber = parsedObject.cardNumber,
                    BuyerName = parsedObject.buyerName,
                    Status = parsedObject.status,
                    Reason = parsedObject.reason,
                    Description = parsedObject.description,
                    Installment = parsedObject.installment,
                    Is3D = parsedObject.is3D,
                    Month = isMonthDigit == true ? month : null,
                    BankCode = parsedObject.bankCode,
                    BankName = parsedObject.bankName,
                    Method = parsedObject.method,
                    TransactionTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToDouble(parsedObject.transactionTime)),
                    SuccessTime = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToDouble(parsedObject.successTime)),
                    BankHotline = parsedObject.bankHotline,
                    MerchantFee = Convert.ToDouble(parsedObject.merchantFee),
                    PayerFee = parsedObject.payerFee != null ? Convert.ToDouble(parsedObject.payerFee) : null,
                    BankType = parsedObject.bankType,
                    AuthenCode = parsedObject.authenCode
                };
                await _unitOfWork.TransactionRepo.AddAsync(transaction);
                await _unitOfWork.SaveAsync();

                var orderDetail = await _unitOfWork.OrderDetailRepo.GetAllAsync(o => o.OrderHeaderId == orderHeaderId);
                var bookIds = orderDetail.Select(o => o.BookId).ToList();

                var shoppingCarts = await _unitOfWork.ShoppingCartRepo.GetAllAsync(
                    u => u.ApplicationUserId == userId && bookIds.Contains(u.BookId));
                _unitOfWork.ShoppingCartRepo.RemoveRange(shoppingCarts);
                await _unitOfWork.SaveAsync();

                HttpContext.Session.Clear();

                return Redirect("~/");
            }
            catch (Exception ex)
            {
                return Content(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
            }
        }

        [HttpPost]
		public async Task<IActionResult> CartCountDecrease(string applicationUserId, int bookId)
		{		
			var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(u => u.ApplicationUserId == applicationUserId && u.BookId == bookId, tracked: true);
            if (cartFromDb.Count > 1)
            {
                cartFromDb.Count--;
                _unitOfWork.ShoppingCartRepo.Update(cartFromDb);
				await _unitOfWork.SaveAsync();
                var price = (await _unitOfWork.BookRepo.GetAsync(b => b.BookId == bookId)).DiscountPrice;
				return Json(new { success = true, subTotal = cartFromDb.Count * price });
			}
            else
            {
				HttpContext.Session.SetInt32(StaticDetails.SessionCart,
		        (await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == cartFromDb.ApplicationUserId)).Count() - 1);
				_unitOfWork.ShoppingCartRepo.Remove(cartFromDb);
				await _unitOfWork.SaveAsync();
				return Json(new { success = true });
			}
			
		}

        [HttpPost]
        public async Task<IActionResult> CartCountIncrease(string applicationUserId, int bookId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(u => u.ApplicationUserId == applicationUserId && u.BookId == bookId);
            
            cartFromDb.Count++;
            _unitOfWork.ShoppingCartRepo.Update(cartFromDb);
            await _unitOfWork.SaveAsync();

			var price = (await _unitOfWork.BookRepo.GetAsync(b => b.BookId == bookId)).DiscountPrice;
			return Json(new { success = true, subTotal = cartFromDb.Count * price });
		}

        [HttpDelete]
        public async Task<IActionResult> RemoveCart(string applicationUserId, int bookId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCartRepo.GetAsync(u => u.ApplicationUserId == applicationUserId && u.BookId == bookId, tracked: true);
            
			HttpContext.Session.SetInt32(StaticDetails.SessionCart,
					(await _unitOfWork.ShoppingCartRepo.GetAllAsync(s => s.ApplicationUserId == cartFromDb.ApplicationUserId)).Count()-1);
			_unitOfWork.ShoppingCartRepo.Remove(cartFromDb);
			await _unitOfWork.SaveAsync();
			return Json(new { success = true });
		}
    }
}
