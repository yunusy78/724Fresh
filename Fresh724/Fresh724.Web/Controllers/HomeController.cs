using System.Diagnostics;
using System.Security.Claims;
using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Fresh724.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Mvc.Core;

namespace Fresh724.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _um;
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole> _rm;

    public HomeController(ILogger<HomeController> logger,RoleManager<IdentityRole> rm, ApplicationDbContext db,IUnitOfWork unitOfWork, UserManager<ApplicationUser> um)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _um = um;
        _db = db;
        _rm = rm;
    }

    public ViewResult Index(string currentFilter, string searchString, int? page)
    {
      

        if (searchString != null)
        {
            page = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        ViewBag.CurrentFilter = searchString;
        var categories = _unitOfWork.Categories.GetAll();
        ViewBag.Categories = categories;
        
        var product = from s in _unitOfWork.Products.GetAll()
            select s;
        product = product.Where(s => s.Status == StatusService.Available);
        if (!string.IsNullOrEmpty(searchString))
        {
            product = product.Where(s => s.Title.Contains(searchString) || s.CategoryName.Contains(searchString));
        }

        int pageSize;
        int pageNumber;
        

        pageSize = 50;
        pageNumber = (page ?? 1);
        return View(product.ToPagedList(pageNumber, pageSize));
    }


    public IActionResult Details(Guid productId)
    {
        
        Cart cartObj = new()
        {
            Quantity = 1,
            ProductId = productId,
            Product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category"),
        };
        var product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == productId);
        var company = _unitOfWork.Companies.GetAll(s => s.Id == product.CompanyId);
        ViewBag.Companies = company;

        return View(cartObj);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(Cart shoppingCart)
    {
        var user = _um.GetUserAsync(User).Result;
        var userRole = _um.GetRolesAsync(user).Result;
        var Role = userRole.FirstOrDefault();
        if (Role==null)
        {
            _um.AddToRoleAsync(user, RoleService.Role_User_Indi).Wait();
            _db.SaveChanges();


        }
        
        shoppingCart.UserId = user.Id;

        Cart cartFromDb = _unitOfWork.Carts.GetFirstOrDefault(
            u => u.UserId == user.Id && u.ProductId == shoppingCart.ProductId);


        if (cartFromDb == null)
        {
            _unitOfWork.Carts.Add(shoppingCart);
            _unitOfWork.SaveChanges();
            HttpContext.Session.SetInt32(PaymentService.SessionCart,
                _unitOfWork.Carts.GetAll(u => u.UserId == user.Id).ToList().Count);
        }
        else
        {
            _unitOfWork.Carts.IncrementCount(cartFromDb, shoppingCart.Quantity);
            _unitOfWork.SaveChanges();
        }


        return RedirectToAction(nameof(Index));
    }


    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


   
}