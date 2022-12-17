using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Fresh724.Web.Areas.Admin.Controllers;
[Area(RoleService.Role_Admin)]
[Authorize(Roles = RoleService.Role_Admin)]
// GET
public class CompanyController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CompanyController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _um;
    private readonly IWebHostEnvironment _hostEnvironment;
 

    public CompanyController(ILogger<CompanyController> logger,ApplicationDbContext db,IWebHostEnvironment hostEnvironment, IUnitOfWork unitOfWork, UserManager<ApplicationUser> um)
    {
        
        _logger = logger;
        _unitOfWork = unitOfWork;
        _um = um;
        _hostEnvironment = hostEnvironment;
        _db = db;
        
    }
    
    public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
    {
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
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

        var company = from s in _unitOfWork.Companies.GetAll()
            select s;
        if (!string.IsNullOrEmpty(searchString))
        {
            company = company.Where(s => s.Name.Contains(searchString)
                                           || s.CreatedBy.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                company = company.OrderByDescending(s => s.Name);
                break;
            case "Date":
                company = company.OrderBy(s => s.CreatedDateTime);
                break;
            case "date_desc":
                company = company.OrderByDescending(s => s.CreatedDateTime);
                break;
            default: // Name ascending 
                company = company.OrderBy(s => s.Name);
                break;

        }

        int pageSize = 2;
        int pageNumber = (page ?? 1);
        return View(company.ToPagedList(pageNumber, pageSize));
    }
    

    [HttpGet]
    public IActionResult  Add()
    {
        var user = _um.GetUserAsync(User).Result;
        
        Entity.Entities.Company company = new();
        company.CreatedBy = user.UserName;
        company.CreatedDateTime = DateTime.Now;
        company.Status=StatusService.ItemStatusActive;;
        if(company == null)
        {
            return View();
        }
        
            
        
        return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Entity.Entities.Company company,IFormFile? file)
    {

        var user = _um.GetUserAsync(User).Result;
        company.CreatedBy = user.UserName;
        company.CreatedDateTime = DateTime.Now;
        company.Status=StatusService.ItemStatusActive;
        ModelState.Clear();

        // Reevaluate the model with the added fields
        if (!TryValidateModel(company))
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images/company");
                var extension = Path.GetExtension(file.FileName);

                if (company.IconUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, company.IconUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                company.IconUrl = @"\images\company\" + fileName + extension;

            }
            
            _unitOfWork.Companies.Add(company);
            _unitOfWork.SaveChanges();
            TempData["success"] = "Company added successfully";
            return RedirectToAction("Index");

        }
        return View(company);
    }
    
    
   
    [HttpGet]
    public IActionResult Edit(Guid? id)
    {
        var user = _um.GetUserAsync(User).Result;
        
       
        
        if (id == null)
        {
            return NotFound();
        }
        var company = _unitOfWork.Companies.GetFirstOrDefault(u=>u.Id==id);
        company.CreatedBy = user.UserName;
        company.CreatedDateTime = DateTime.Now;
        company.ModifiedBy = user.UserName;
        company.ModifiedDateTime = DateTime.Now;
        if (company == null)
        {
            return NotFound();
        }

        return View(company);
       
    }
    
   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Entity.Entities.Company company,IFormFile? file)
    {

        var user = _um.GetUserAsync(User).Result;
        company.ModifiedBy = user.UserName;
        company.ModifiedDateTime = DateTime.Now;
        ModelState.ClearValidationState(nameof(Company));
        if (!TryValidateModel(company, nameof(Company)))
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images/company");
                var extension = Path.GetExtension(file.FileName);

                if (company.IconUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, company.IconUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                company.IconUrl = @"\images\company\" + fileName + extension;

            }
            else
            {
                company.IconUrl = company.IconUrl;
               
            }
            _unitOfWork.Companies.Update(company);
            _unitOfWork.SaveChanges();
            TempData["success"] = "Company edited successfully";
            return RedirectToAction("Index");

        }
        return View(company);
    }
    
            
            [HttpGet]
            public IActionResult GetAll()
            {
                var companyList = _unitOfWork.Companies.GetAll();
                return Json(new { data = companyList });
            }

            [Authorize]
            [HttpGet]
            public IActionResult Delete(Guid? id)
            {
                var company = _unitOfWork.Companies.GetFirstOrDefault(u=>u.Id==id);
                //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);

                if (company == null)
                {
                    return NotFound();
                }

                return View(company);
            }

// POST: Movies/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public IActionResult Delete(Guid id, Entity.Entities.Company company)
            {
                company.CreatedDateTime = DateTime.Now;
                var user = _um.GetUserAsync(User).Result;
                ModelState.ClearValidationState(nameof(Company));

                // Reevaluate the model with the added fields
                if (!TryValidateModel(company))
                {
                    _unitOfWork.Companies.Remove(company);
                    _unitOfWork.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(company);
            }

            [HttpGet]
            public IActionResult Details(Guid? id)
            {
                var user = _um.GetUserAsync(User).Result;
                ViewBag.UserFullName = user.FirstName + " " + user.LastName;
                var details = _unitOfWork.Companies.GetFirstOrDefault(u=>u.Id==id);
                var companyComment= _unitOfWork.CompanyRatings.GetAll(u=>u.CompanyId==id);
                ViewBag.comment = companyComment;
                var number=_unitOfWork.CompanyRatings.GetAll(u=>u.CompanyId==id).Count();
                var average=_unitOfWork.CompanyRatings.GetAll(u=>u.CompanyId==id).ToList().Select(x=>x.Rating).DefaultIfEmpty(0).Average();
                ViewBag.number = number;
                ViewBag.Avg = Math.Round(average);
                if (id == null)
                {
                    return NotFound();
                }

                if (details == null)
                {
                    return NotFound();
                }

                return View(details);
            }
            
         
         
            
    
    
}