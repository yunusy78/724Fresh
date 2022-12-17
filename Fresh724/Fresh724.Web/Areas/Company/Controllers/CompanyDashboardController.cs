using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fresh724.Web.Areas.Company.Controllers;
[Area(RoleService.Role_User_Comp)]
[Authorize(Roles = RoleService.Role_User_Comp + "," + RoleService.Role_Admin + "," + RoleService.Role_User_Empl)]
public class CompanyDashboardController : Controller
{
    
    private readonly ILogger<CompanyDashboardController > _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _um;
    private readonly IWebHostEnvironment _hostEnvironment;

    public CompanyDashboardController(ILogger<CompanyDashboardController > logger,IWebHostEnvironment hostEnvironment,IUnitOfWork unitOfWork,ApplicationDbContext db, UserManager<ApplicationUser> um)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _um = um;
        _db = db;
        _hostEnvironment= hostEnvironment;
    }
    // GET
    public IActionResult Index()
    {
        var user = _um.GetUserAsync(User).Result;
        var employees = _unitOfWork.Employees.OrderByDescending().ToList();
        foreach(var i in employees.ToArray())
        {
            
            if (i.CompanyId != user.CompanyId) employees.Remove(i);
        }
        ViewBag.Employees = employees;
        
        var products = _unitOfWork.Products.OrderByDescending().ToList();
        foreach(var i in products.ToArray())
        {
            
            if (i.CompanyId != user.CompanyId) products.Remove(i);
        }
        ViewBag.Products = products;
        
        
        var objList = _unitOfWork.Orders.OrderByDescending().ToList();
            
        foreach(var i in objList.ToArray())
        {
            var obj = _unitOfWork.OrderDetails.GetAll().Where(x => x.OrderId == i.Id).ToList();
            
            foreach(var j in obj.ToArray())
            {
                var obj2 = _unitOfWork.Products.GetAll().Where(x => x.Id == j.ProductId).ToList();
                    
                foreach(var k in obj2.ToArray())
                {
                    if (k.CompanyId != user.CompanyId) objList.Remove(i);

                }
                    
            }
        }
        ViewBag.Orders = objList;
        
        
        double todayTotalPrice=0;
        double mountTotalPrice=0;
        double yearTotalPrice=0;
        foreach(var i in objList.ToArray())
        {
            if (i.OrderDate.Month == DateTime.Today.Month) 
                mountTotalPrice += i.TotalPrice;
        }
        foreach(var i in objList.ToArray())
        {
            if (i.OrderDate.Year == DateTime.Today.Year) 
                yearTotalPrice += i.TotalPrice;
        }
        foreach(var i in objList.ToArray())
        {
            if (i.OrderDate <= DateTime.Today) 
                objList.Remove(i);
            
        }
        foreach(var i in objList)
        {
            todayTotalPrice += i.TotalPrice;
        }
        ViewBag.TodayTotalPrice = todayTotalPrice;
        ViewBag.totalIncome = yearTotalPrice*0.8;
        ViewBag.mountTotalPrice = mountTotalPrice;
        ViewBag.yearTotalPrice = yearTotalPrice;
        return View();
    }
}