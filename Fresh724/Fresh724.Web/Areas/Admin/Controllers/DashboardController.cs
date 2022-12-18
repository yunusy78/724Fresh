using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fresh724.Web.Areas.Admin.Controllers;
[Area(RoleService.Role_Admin)]
[Authorize(Roles =  RoleService.Role_Admin )]
public class DashboardController : Controller
{
    
    private readonly ILogger<DashboardController > _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _um;
    private readonly IWebHostEnvironment _hostEnvironment;

    public DashboardController(ILogger<DashboardController > logger,IWebHostEnvironment hostEnvironment,IUnitOfWork unitOfWork,ApplicationDbContext db, UserManager<ApplicationUser> um)
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
        var company = _unitOfWork.Companies.GetAll();  
        ViewBag.Company = company;
        var category = _unitOfWork.Categories.GetAll();  
        ViewBag.Category = category;
        var objList = _unitOfWork.Orders.OrderByDescending().ToList();
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
        ViewBag.totalIncome = Convert.ToInt32(yearTotalPrice*0.2);
        ViewBag.mountTotalPrice = mountTotalPrice;
        ViewBag.yearTotalPrice = yearTotalPrice;
        return View();
    }
}