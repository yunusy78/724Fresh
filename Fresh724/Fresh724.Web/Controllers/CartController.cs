using System.Security.Claims;
using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Fresh724.Web.Controllers;
[Authorize]
public class CartController : Controller
{
    // GET
    private readonly ILogger<CartController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _um;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _emailSender;
    [BindProperty]
    public CartList CartList { get; set; }
    public CartController(ILogger<CartController> logger,IEmailSender emailSender,ApplicationDbContext db, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, UserManager<ApplicationUser> um)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _um = um;
        _roleManager = roleManager;
        _db = db;
        _emailSender = emailSender;
        
    }
    
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _um.GetUserAsync(User).Result;
           
            CartList = new CartList()
            {
                CartItems = _unitOfWork.Carts.GetAll(u => u.UserId == claim.Value,
                includeProperties: "Product"),
                OrderShopping = new()
            };
            foreach(var cart in CartList.CartItems)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Quantity, cart.Product.PurchasePrice);
                CartList.OrderShopping.TotalPrice += (cart.Price * cart.Quantity);
            }
            return View(CartList);
        }





        public IActionResult Checkout()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _um.GetUserAsync(User).Result;
            var address= _unitOfWork.AddressUsers.GetAll(u => u.UserId == claim.Value).FirstOrDefault();

            if (User.IsInRole(RoleService.Role_User_Indi) && address == null)
            {
                return RedirectToAction("Add", "AddressUser");
            }
            if (user.FirstName=="" || user.LastName=="" || user.PhoneNumber=="")
            {
                
                ViewBag.information = _unitOfWork.ApplicationUsers.GetAll(s => s.Id == user.Id);
                ViewBag.Id = user.Id;
                return RedirectToAction("Index", "MyAccount");
            }
            
            else if (User.IsInRole(RoleService.Role_User_Comp) && address== null)
            {
                return RedirectToAction("Add", "AddressUser");
            }
            else
            {
                CartList = new CartList()
                {
                    CartItems = _unitOfWork.Carts.GetAll(u => u.UserId == claim.Value,
                        includeProperties: "Product"),
                    OrderShopping= new()
                };
                CartList.OrderShopping.User = _unitOfWork.ApplicationUsers.GetFirstOrDefault(
                    u => u.Id == user.Id);
                if (User.IsInRole(RoleService.Role_User_Comp))

                {
                    CartList.OrderShopping.FirstName =  CartList.OrderShopping.User.UserName; 
                    CartList.OrderShopping.LastName =  CartList.OrderShopping.User.UserName; 
                }
                    
                else{ 
                    CartList.OrderShopping.FirstName =  CartList.OrderShopping.User.FirstName;
                CartList.OrderShopping.LastName =  CartList.OrderShopping.User.LastName;
                }
                
                CartList.OrderShopping.PhoneNumber = CartList.OrderShopping.User.PhoneNumber;
                CartList.OrderShopping.Email = CartList.OrderShopping.User.Email;
                var addressUser = _unitOfWork.AddressUsers.GetFirstOrDefault(
                    u => u.UserId == user.Id);
            
            
                CartList.OrderShopping.Street1 = addressUser.Street1;
                CartList.OrderShopping.Street2 = addressUser.Street2;
                CartList.OrderShopping.City = addressUser.City;
                CartList.OrderShopping.State = addressUser.State;
                CartList.OrderShopping.ZipCode = addressUser.ZipCode;
                CartList.OrderShopping.Country = addressUser.Country;



                foreach(var cart in CartList.CartItems)
                {
                    cart.Price = GetPriceBasedOnQuantity(cart.Quantity, cart.Product.PurchasePrice);
                    CartList.OrderShopping.TotalPrice += (cart.Price * cart.Quantity);
                }
                if(CartList.OrderShopping.TotalPrice == 0)
                {
                    return RedirectToAction("Index");
                }
                return View(CartList);
            }

        }
        
        
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckoutPOST()
        {
            var user = _um.GetUserAsync(User).Result;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartList.CartItems = _unitOfWork.Carts.GetAll(u => u.UserId == claim.Value,
                includeProperties: "Product");

           
            CartList.OrderShopping.OrderDate = DateTime.Now;
            CartList.OrderShopping.UserId = user.Id;


            foreach(var cart in CartList.CartItems)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Quantity, cart.Product.PurchasePrice);
                CartList.OrderShopping.TotalPrice += (cart.Price * cart.Quantity);
            }
            
            ApplicationUser applicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == claim.Value);
            
                CartList.OrderShopping.PaymentStatus = PaymentService.PaymentStatusPending;
                CartList.OrderShopping.OrderStatus = StatusService.Pending;
          

            _unitOfWork.Orders.Add(CartList.OrderShopping);
            _unitOfWork.SaveChanges();
            foreach (var cart in CartList.CartItems)
            {
                var product= _unitOfWork.Products.GetFirstOrDefault(u => u.Id == cart.ProductId);
                
                OrderShoppingDetails orderDetail = new()
                {
                    
                    ProductId = cart.ProductId,
                    OrderId = CartList.OrderShopping.Id,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };
                _unitOfWork.OrderDetails.Add(orderDetail);
                _unitOfWork.SaveChanges();
            }

            
                //stripe settings 
                var domain = "https://localhost:7085/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"cart/OrderConfirmation?id={CartList.OrderShopping.Id}",
                    CancelUrl = domain + $"cart/index",
                };

                foreach (var cart in CartList.CartItems)
                {

                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(cart.Price * 100),
                            Currency = "nok",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = cart.Product.Title
                            },

                        },
                        Quantity = cart.Quantity,
                    };
                    options.LineItems.Add(sessionLineItem);

                }
                
               
                

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.Orders.UpdateStripeSessionId(CartList.OrderShopping.Id, session.Id);
                _unitOfWork.SaveChanges();
                _unitOfWork.Orders.UpdateStripePaymentID(CartList.OrderShopping.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.SaveChanges();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            

            
        }

        public IActionResult OrderConfirmation(Guid id)
        {
            var orderShopping = _unitOfWork.Orders.GetFirstOrDefault(u => u.Id == id, includeProperties: "User");
            if (orderShopping.PaymentStatus != PaymentService.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderShopping.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Orders.UpdateStripePaymentID(id, orderShopping.SessionId, session.PaymentIntentId);
                    _unitOfWork.Orders.UpdateStatus(id, StatusService.Approved, PaymentService.PaymentStatusApproved);
                    _unitOfWork.SaveChanges();
                }
            }

            _emailSender.SendEmailAsync(orderShopping.User.Email, "New Order - Fresh724", "<p>New Order Created </p>");
            List<Cart> Carts = _unitOfWork.Carts.GetAll(u => u.UserId ==
                  
                                                             orderShopping.UserId).ToList();
            var cartList = new CartList()
            {
                OrderShopping = orderShopping,
                CartItems = Carts
            };
            
            foreach (Cart cart in cartList.CartItems)
            {
                var product= _unitOfWork.Products.GetFirstOrDefault(u => u.Id == cart.ProductId);
                product.Quantity -= cart.Quantity;
                if(product.Quantity == 0)
                {
                    product.Status = StatusService.Unavailable;
                }
                _unitOfWork.Products.Update(product);
                _unitOfWork.SaveChanges();
            }

            HttpContext.Session.Clear();
            _unitOfWork.Carts.RemoveRange(Carts);
            _unitOfWork.SaveChanges();
            return View(id);
        }

        public async Task<IActionResult> OrderConformPaypal()
        {
            //TODo will make changes here for paypal and will be ....

            return View();
        }

        public IActionResult Plus(Guid cartId)
        {
            var cart = _unitOfWork.Carts.GetFirstOrDefault(u => u.Id == cartId);
            var product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == cart.ProductId);
            if (cart.Quantity < product.Quantity)
            {
                _unitOfWork.Carts.IncrementCount(cart, 1);
                _unitOfWork.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(Guid cartId)
        {
            var cart = _unitOfWork.Carts.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Quantity <= 1)
            {
                _unitOfWork.Carts.Remove(cart);
                var count = _unitOfWork.Carts.GetAll(u => u.UserId == cart.UserId).ToList().Count-1;
                HttpContext.Session.SetInt32(PaymentService.SessionCart, count);
            }
            else
            {
                _unitOfWork.Carts.DecrementCount(cart, 1);
            }
            _unitOfWork.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(Guid cartId)
        {
            var cart = _unitOfWork.Carts.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.Carts.Remove(cart);
            _unitOfWork.SaveChanges();
            var count = _unitOfWork.Carts.GetAll(u => u.UserId == cart.UserId).ToList().Count;
            HttpContext.Session.SetInt32(PaymentService.SessionCart, count);
            return RedirectToAction(nameof(Index));
        }
        private double GetPriceBasedOnQuantity(double quantity, double price)
        {
            
                return price;
           
        }
        
    }


