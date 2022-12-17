using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace Fresh724.Web.Controllers;
[Area(RoleService.Role_User_Comp)]
[Authorize(Roles = RoleService.Role_User_Comp + "," + RoleService.Role_Admin + "," + RoleService.Role_User_Empl)]
public class OrderShoppingController : Controller
{
    
        private readonly ILogger<AddressUserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _um;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        
        [BindProperty]
        public OrderShoppingList OrderShoppingList { get; set; }
        public OrderShoppingController(ILogger<AddressUserController> logger,IEmailSender emailSender,ApplicationDbContext db, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, UserManager<ApplicationUser> um)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _um = um;
            _roleManager = roleManager;
            _db = db;
            _emailSender = emailSender;
        
        }
        
    // GET
    public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
    {
        var user = _um.GetUserAsync(User).Result;
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "email_desc" : "";
        ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

        if (searchString != null)
        {
            page = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        ViewBag.CurrentFilter = searchString;

        var orders = from s in _unitOfWork.Orders.GetAll()
            select s;
        if (!string.IsNullOrEmpty(searchString))
        {
            orders = orders.Where(s => s.FirstName.Contains(searchString)
                                       || s.Email.Contains(searchString) || s.OrderStatus.Contains(searchString));
        }
 
        switch (sortOrder)
        {
            case "name_desc":
                orders = orders.OrderByDescending(s => s.FirstName);
                break;
            case "email_desc":
                orders = orders.OrderByDescending(s => s.Email);
                break;
            case "Date":
                orders = orders.OrderBy(s => s.OrderDate);
                break;
            case "date_desc":
                orders = orders.OrderByDescending(s => s.OrderDate);
                break;
            default: // Name ascending 
                orders = orders.OrderBy(s => s.FirstName);
                break;

        }

        int pageSize;
        int pageNumber;
        
       
           
        if ( User.IsInRole(RoleService.Role_Admin))
        {
            pageSize = 5;
            pageNumber = (page ?? 1);
            return View(orders.ToPagedList(pageNumber, pageSize));
        }
        else
        {
            
            var objList1 = _unitOfWork.Orders.OrderByDescending().ToList();
            
            foreach(var i in objList1.ToArray())
            {
                var obj = _unitOfWork.OrderDetails.GetAll().Where(x => x.OrderId == i.Id).ToList();
            
                foreach(var j in obj.ToArray())
                {
                    var obj2 = _unitOfWork.Products.GetAll().Where(x => x.Id == j.ProductId).ToList();
                    
                    foreach(var k in obj2.ToArray())
                    {
                        if (k.CompanyId != user.CompanyId) objList1.Remove(i);

                    }
                    
                }
            }
            switch (sortOrder)
            {
                case "name_desc":
                    orders = objList1.OrderByDescending(s => s.FirstName);
                    break;
                case "email_desc":
                    orders = objList1.OrderByDescending(s => s.Email);
                    break;
                case "Date":
                    orders = objList1.OrderBy(s => s.OrderDate);
                    break;
                case "date_desc":
                    orders = objList1.OrderByDescending(s => s.OrderDate);
                    break;
                default: // Name ascending 
                    orders = objList1.OrderBy(s => s.FirstName);
                    break;

            }
            if (!string.IsNullOrEmpty(searchString))
            {
                var orders2 = orders.Where(s => s.FirstName.Contains(searchString)
                                                  || s.Email.Contains(searchString) || s.OrderStatus.Contains(searchString));
                pageSize = 5;
                pageNumber = (page ?? 1);
                return View(orders2.ToPagedList(pageNumber, pageSize)); 
            }
            else
            {
                pageSize = 5;
                pageNumber = (page ?? 1);
                return View(orders.ToPagedList(pageNumber, pageSize)); 
            }

        }
    }

       
       public IActionResult Details(Guid id)
       {
           OrderShoppingList = new OrderShoppingList()
           {
               OrderShopping = _unitOfWork.Orders.GetFirstOrDefault(u => u.Id == id, includeProperties: "User"),
               OrderShoppingDetails = _unitOfWork.OrderDetails.GetAll(u => u.OrderId == id, includeProperties: "Product"),
             
           };
           return View(OrderShoppingList);
       }
        
        
        

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            OrderShoppingList.OrderShopping= _unitOfWork.Orders.GetFirstOrDefault(u => u.Id == OrderShoppingList.OrderShopping.Id);
            OrderShoppingList.OrderShoppingDetails = _unitOfWork.OrderDetails.GetAll(u => u.OrderId == OrderShoppingList.OrderShopping.Id);

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
                SuccessUrl = domain + $"Company/OrderShopping/PaymentConfirmation?orderShoppingId={OrderShoppingList.OrderShopping.Id}",
                CancelUrl = domain + $"Company/OrderShopping/Details?orderId={OrderShoppingList.OrderShopping.Id}",
            };

            foreach (var item in OrderShoppingList.OrderShoppingDetails)
            {

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "kr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },

                    },
                    Quantity = item.Quantity,
                };
                options.LineItems.Add(sessionLineItem);

            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.Orders.UpdateStripePaymentID(OrderShoppingList.OrderShopping.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.SaveChanges();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(Guid orderShoppingId)
        {
            OrderShopping orderShopping = _unitOfWork.Orders.GetFirstOrDefault(u => u.Id == orderShoppingId);
            if (orderShopping.PaymentStatus == PaymentService.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderShopping.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Orders.UpdateStatus(orderShoppingId, orderShopping.OrderStatus, PaymentService.PaymentStatusApproved);
                    _unitOfWork.SaveChanges();
                }
            }
            return View(orderShoppingId);
        }

        [HttpPost]
        [Authorize(Roles = RoleService.Role_User_Comp + "," + RoleService.Role_User_Empl)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrder()
        {
            var order = _unitOfWork.Orders.GetFirstOrDefault(u => u.Id == OrderShoppingList.OrderShopping.Id,tracked:false);
            order.FirstName = OrderShoppingList.OrderShopping.FirstName;
            order.LastName = OrderShoppingList.OrderShopping.LastName;
            order.PhoneNumber = OrderShoppingList.OrderShopping.PhoneNumber;
            order.Street1 = OrderShoppingList.OrderShopping.Street1;
            order.Street2 = OrderShoppingList.OrderShopping.Street2;
            order.City = OrderShoppingList.OrderShopping.City;
            order.State =  OrderShoppingList.OrderShopping.State;
            order.ZipCode =  OrderShoppingList.OrderShopping.ZipCode;
            order.Country =  OrderShoppingList.OrderShopping.Country;
            order.OrderStatus =  OrderShoppingList.OrderShopping.OrderStatus;
            if (order.OrderStatus==StatusService.ReadyForPickup)
            {
                _emailSender.SendEmailAsync(order.Email, "New Information - Fresh724", "<p>Hello! Dear Customer! Your order is ready for PickUp</p>");
            }
            if (order.OrderStatus==StatusService.Completed)
            {
                _emailSender.SendEmailAsync(order.Email, "New Information - Fresh724", "<p>Thank You Dear Customer! Your order is completed</p>");
            }
            
            _unitOfWork.Orders.Update(order);
            _unitOfWork.SaveChanges();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Index", "OrderShopping", new { orderId = order.Id });
        }

        [HttpPost]
        [Authorize(Roles = RoleService.Role_User_Comp + "," + RoleService.Role_User_Empl)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.Orders.UpdateStatus(OrderShoppingList.OrderShopping.Id, StatusService.InProcess);
            _unitOfWork.SaveChanges();
            TempData["Success"] = "Order Status Updated Successfully.";
            
            _emailSender.SendEmailAsync(OrderShoppingList.OrderShopping.Email, "New Information - Fresh724", "<p>Hello! Dear Customer! Your order is in process</p>");
            return RedirectToAction("Index", "OrderShopping", new { orderId = OrderShoppingList.OrderShopping.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = RoleService.Role_User_Comp + "," + RoleService.Role_User_Empl)]
        [ValidateAntiForgeryToken]
        public IActionResult StartPickUp()
        {
            _unitOfWork.Orders.UpdateStatus(OrderShoppingList.OrderShopping.Id, StatusService.ReadyForPickup);
            _unitOfWork.SaveChanges();
            TempData["Success"] = "Order Status Updated Successfully.";
            
            _emailSender.SendEmailAsync(OrderShoppingList.OrderShopping.Email, "New Information - Fresh724", "<p>Hello! Dear Customer! Your order is ready for PickUp</p>");
            return RedirectToAction("Index", "OrderShopping", new { orderId = OrderShoppingList.OrderShopping.Id });
        }

        

        [HttpPost]
        [Authorize(Roles = RoleService.Role_User_Comp + "," + RoleService.Role_User_Empl)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderShopping = _unitOfWork.Orders.GetFirstOrDefault(u => u.Id == OrderShoppingList.OrderShopping.Id, tracked: false);
            if (orderShopping.PaymentStatus == PaymentService.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderShopping.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.Orders.UpdateStatus(orderShopping.Id, StatusService.Cancelled, StatusService.Refunded);
                _emailSender.SendEmailAsync(orderShopping.Email, "New Information - Fresh724", "<p>Hello! Dear Customer! Your order is cancelled and Payment is refunded</p>");
            }
            else
            {
                _unitOfWork.Orders.UpdateStatus(orderShopping.Id, StatusService.Cancelled, StatusService.Cancelled);
                
            }
            _unitOfWork.SaveChanges();
            TempData["Success"] = "Order Status Updated Successfully.";
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Index", "OrderShopping", new { orderId = OrderShoppingList.OrderShopping.Id });
        }
        
        
        
        [HttpGet]
        public IActionResult Delete(Guid? id)
        {
            var order = _unitOfWork.Orders.GetFirstOrDefault(u=>u.Id==id);
           

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
            
            
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            var order = _unitOfWork.Orders.GetFirstOrDefault(u=>u.Id==id);
            if (order == null)
            {
                return NotFound();
            }

            _unitOfWork.Orders.Remove(order);
            _unitOfWork.SaveChanges();
            TempData["success"] = "Employee deleted successfully";
            return RedirectToAction("Index");
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderShopping> orderShopping;

            if (User.IsInRole(RoleService.Role_User_Comp) || User.IsInRole(RoleService.Role_User_Empl)) { 
                orderShopping = _unitOfWork.Orders.GetAll(includeProperties: "User");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                
                orderShopping = _unitOfWork.Orders.GetAll(u=>u.UserId==claim.Value,includeProperties: "User");
            }

            switch (status)
            {
                case "pending":
                    orderShopping = orderShopping.Where(u => u.PaymentStatus == PaymentService.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderShopping = orderShopping.Where(u => u.OrderStatus == StatusService.InProcess);
                    break;
                case "completed":
                    orderShopping = orderShopping.Where(u => u.OrderStatus == StatusService.Completed);
                    break;
                case "ReadyForPickup":
                    orderShopping = orderShopping.Where(u => u.OrderStatus == StatusService.ReadyForPickup);
                    break;
                case "approved":
                    orderShopping = orderShopping.Where(u => u.OrderStatus == StatusService.Approved);
                    break;
                default:
                    break;
            }


            return Json(new { data = orderShopping });
        }
        #endregion
        
        
        
}