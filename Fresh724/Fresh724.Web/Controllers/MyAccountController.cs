using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace Fresh724.Web.Controllers;
[Authorize]
public class MyAccountController : Controller
{
    private readonly ILogger<MyAccountController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _um;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public MyAccountController(ILogger<MyAccountController> logger,ApplicationDbContext db, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, UserManager<ApplicationUser> um)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _um = um;
        _roleManager = roleManager;
        _db = db;
        
    }


    [Authorize]
    [HttpGet]
    public ViewResult Index()
    {
        var user = _um.GetUserAsync(User).Result;
        ViewBag.address =  _unitOfWork.AddressUsers.GetAll(s => s.UserId == user.Id);
        ViewBag.information = _unitOfWork.ApplicationUsers.GetAll(s => s.Id == user.Id);
        ViewBag.order= _unitOfWork.Orders.GetAll(s => s.UserId == user.Id);
       
        return View();
    }
    
    [Authorize]
    [HttpGet]
   
    public IActionResult Edit(string id)
    {
        if(id==null || id == string.Empty)
        {
            return NotFound();
        }
        
        var user = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u=>u.Id==id);
        
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ApplicationUser user)
    {
        var user1=_um .GetUserAsync(User).Result;
        if (ModelState.IsValid)
        {
            user1.FirstName = user.FirstName;
            user1.LastName = user.LastName;
            user1.PhoneNumber = user.PhoneNumber;
            user1.Email = user.Email;
            user1.UserName = user.Email;
            _unitOfWork.ApplicationUsers.Update(user1);
            _unitOfWork.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(user);
    }
   
    

        [HttpGet]
        public IActionResult OrderDetails(Guid? id)
        {
            var user = _um.GetUserAsync(User).Result;
            var order = _unitOfWork.Orders.GetFirstOrDefault(u=>u.Id==id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
        
       
       [HttpGet]
       public ViewResult RatingCompany(Guid? id)
       {
           var user = _um.GetUserAsync(User).Result;
           var order = _unitOfWork.Orders.GetFirstOrDefault(u => u.Id == id);
           ViewBag.orderId = order.Id;
           var orderDetails = _unitOfWork.OrderDetails.GetAll(s => s.OrderId == order.Id);
           var product = _unitOfWork.Products.GetAll(s => s.Id == orderDetails.FirstOrDefault().ProductId);
           var company = _unitOfWork.Companies.GetFirstOrDefault(s => s.Id == product.FirstOrDefault().CompanyId);
           var companyComment= _unitOfWork.CompanyRatings.GetAll(u=>u.CompanyId==company.Id);
           var companyRating = _unitOfWork.CompanyRatings.GetFirstOrDefault(u => u.CompanyId == company.Id && u.UserId == user.Id && u.OrderId==order.Id);
           ViewBag.UserFullName = user.FirstName + " " + user.LastName;
           ViewBag.comment = companyComment;
           var number=_unitOfWork.CompanyRatings.GetAll(u=>u.CompanyId==company.Id).Count();
           var average=_unitOfWork.CompanyRatings.GetAll(u=>u.CompanyId==company.Id).Select(x=>x.Rating).DefaultIfEmpty(0).Average();
           ViewBag.number = number;
           ViewBag.Avg = Math.Round(average);
           return View(company);
       }
       
      
        [HttpPost]
        public IActionResult RatingCompany(Guid id)
        {
            var company = _unitOfWork.Companies.GetFirstOrDefault(u => u.Id == id);
            var user = _um.GetUserAsync(User).Result;
            
            CompanyRating companyRating = new CompanyRating();
            companyRating.CompanyId = company.Id;
            companyRating.UserId = user.Id;
            companyRating.UserFullName = user.FirstName + " " + user.LastName;
            companyRating.Rating = Convert.ToInt32(Request.Form["RatingCompany"]);   
            companyRating.Comment = Request.Form["comment"];
            companyRating.CreateDate = DateTime.Now;
            var orderID = Guid.Parse(Request.Form["OrderId"]);
            companyRating.OrderId = orderID;
            var rating = _unitOfWork.CompanyRatings.GetFirstOrDefault(u => u.CompanyId == company.Id && u.UserId == user.Id && u.OrderId==orderID);
            if (rating != null)
            {
                return RedirectToAction("Index");
            }
            
            if (ModelState.IsValid)
            {
                _unitOfWork.CompanyRatings.Add(companyRating);
                _unitOfWork.SaveChanges();
                return RedirectToAction("RatingCompany");
            }
            return View();
        }
        
   
}