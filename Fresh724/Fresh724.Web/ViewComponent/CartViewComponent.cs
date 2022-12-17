using System.Security.Claims;
using Fresh724.Data.Repository.Abstract;
using Microsoft.AspNetCore.Mvc;
using Fresh724.Data.Context;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Fresh724.Web.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Fresh724.Web.ViewComponent;


    public class CartViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        private readonly ILogger<AddressUserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _um;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        
        public CartViewComponent(ILogger<AddressUserController> logger,IEmailSender emailSender,ApplicationDbContext db, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, UserManager<ApplicationUser> um)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _um = um;
            _roleManager = roleManager;
            _db = db;
            _emailSender = emailSender;
        
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _um.GetUserAsync((ClaimsPrincipal)User).Result;
            if (user != null)
            {
                if (HttpContext.Session.GetInt32(PaymentService.SessionCart) != null)
                {
                    return View(HttpContext.Session.GetInt32(PaymentService.SessionCart));
                }
                else
                {
                    HttpContext.Session.SetInt32(PaymentService.SessionCart,
                        _unitOfWork.Carts.GetAll(u => u.UserId == claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(PaymentService.SessionCart));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
