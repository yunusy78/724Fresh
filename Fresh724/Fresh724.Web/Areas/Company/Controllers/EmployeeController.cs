using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Mvc.Core;

namespace Fresh724.Web.Controllers;
[Area(RoleService.Role_User_Comp)]
[Authorize(Roles = RoleService.Role_User_Comp + "," + RoleService.Role_Admin)]
// GET
    public class EmployeeController : Controller
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _um;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EmployeeController(ILogger<EmployeeController> logger, IUnitOfWork unitOfWork,IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> um)
        {
        
            _logger = logger;
            _unitOfWork = unitOfWork;
            _um = um;
            _hostEnvironment = hostEnvironment;
        
        }
       
        
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        { 
            var user = _um.GetUserAsync(User).Result;
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "first_desc" : "";
        ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "comp_desc" : "";
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

        var employee = from s in _unitOfWork.Employees.GetAll()
            select s;
        if (!string.IsNullOrEmpty(searchString))
        {
            employee = employee.Where(s => s.FirstName.Contains(searchString)
                                           || s.Email.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "first_desc":
                employee = employee.OrderByDescending(s => s.FirstName);
                break;
            case "comp_desc":
                employee = employee.OrderByDescending(s => s.CompanyId);
                break;
            case "Date":
                employee = employee.OrderBy(s => s.CreatedDateTime);
                break;
            case "date_desc":
                employee = employee.OrderByDescending(s => s.CreatedDateTime);
                break;
            default: // Name ascending 
                employee = employee.OrderBy(s => s.FirstName);
                break;

        }

        int pageSize;
        int pageNumber;
        
       
           
        if ( User.IsInRole(RoleService.Role_Admin))
        {
            pageSize = 5;
            pageNumber = (page ?? 1);
            return View(employee.ToPagedList(pageNumber, pageSize));
        }
        else
        {
            var objList = _unitOfWork.Employees.OrderByDescending().ToList();
            foreach(var i in objList.ToArray())
            {
            
                if (i.CompanyId != user.CompanyId) objList.Remove(i);
            }
            switch (sortOrder)
            {
                case "first_desc":
                    employee = objList.OrderByDescending(s => s.FirstName);
                    break;
                case "comp_desc":
                    employee = objList.OrderByDescending(s => s.CompanyId);
                    break;
                case "Date":
                    employee = objList.OrderBy(s => s.CreatedDateTime);
                    break;
                case "date_desc":
                    employee = objList.OrderByDescending(s => s.CreatedDateTime);
                    break;
                default: // Name ascending 
                    employee = objList.OrderBy(s => s.FirstName);
                    break;

            }
            if (!string.IsNullOrEmpty(searchString))
            {
                var product = employee.Where(s => s.FirstName.Contains(searchString)
                                                 || s.Email.Contains(searchString));
                pageSize = 5;
                pageNumber = (page ?? 1);
                return View(product.ToPagedList(pageNumber, pageSize)); 
            }
            else
            {
                pageSize = 5;
                pageNumber = (page ?? 1);
                return View(employee.ToPagedList(pageNumber, pageSize)); 
            }
            
        }
        
        
        
    }
    
    [Authorize]
    [HttpGet]
    public IActionResult  Add()
    {
        var user = _um.GetUserAsync(User).Result;
        var company = _unitOfWork.Companies.GetFirstOrDefault(u=>u.Id == user.CompanyId);
        Employee employee = new();
        employee.CreatedBy = company.Name;
        employee.CompanyId = company.Id;
        employee.CreatedDateTime = DateTime.Now;
        employee.Status=StatusService.ItemStatusActive;;
        if(employee == null)
        {
            return View();
        }
        
            
        
        return View(employee);
    }
//[Bind("FirstName,LastName,CreatedBy,CompanyName, CreatedDateTime,ImageUrl, User, UserId") ]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Employee employee,IFormFile? file)
    {
        var user = _um.GetUserAsync(User).Result;
        var company = _unitOfWork.Companies.GetFirstOrDefault(u=>u.Id == user.CompanyId);
        employee.CreatedBy = company.Name;
        employee.CompanyId = company.Id;
        employee.CreatedDateTime = DateTime.Now;
        employee.Status=StatusService.ItemStatusActive;;
        ModelState.ClearValidationState(nameof(Employee));
        if (!TryValidateModel(employee, nameof(Employee)))
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images\employee");
                var extension = Path.GetExtension(file.FileName);

                if (employee.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, employee.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                employee.ImageUrl = @"\images\employee\" + fileName + extension;

            }
            _unitOfWork.Employees.Add(employee);
            _unitOfWork.SaveChanges();
            TempData["success"] = "Employee added successfully";
            return RedirectToAction("Index");

        }
        return View(employee);
    }

   
    [HttpGet]
    public IActionResult Edit(Guid? id)
    { 
        
        var employee = _unitOfWork.Employees.GetFirstOrDefault(u=>u.Id==id);
        
        employee.ModifiedDateTime = DateTime.Now;
        var user = _um.GetUserAsync(User).Result;
        var company = _unitOfWork.Companies.GetFirstOrDefault(u=>u.Id == user.CompanyId);
        employee.ModifiedBy = company.Name;
        
        if (id == null)
        {
            return NotFound();
        }
        
        if (employee == null || employee.CompanyId !=user.CompanyId )
        {
            return RedirectToAction(nameof(Index));
        }
        
        return View(employee);
       
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
   //[Bind("FirstName,LastName,CreatedBy,CompanyName, CreatedDateTime,ImageUrl, User, UserId") ]
   public IActionResult Edit(Employee employee,IFormFile? file)
   {

       var user = _um.GetUserAsync(User).Result;
       var company = _unitOfWork.Companies.GetFirstOrDefault(u=>u.Id == user.CompanyId);
       employee.ModifiedBy = company.Name;
       employee.CompanyId = company.Id;
       employee.ModifiedDateTime = DateTime.Now;
       ModelState.Clear();

       // Reevaluate the model with the added fields
       if (!TryValidateModel(employee))
       {
           string wwwRootPath = _hostEnvironment.WebRootPath;
           if (file != null)
           {
               string fileName = Guid.NewGuid().ToString();
               var uploads = Path.Combine(wwwRootPath, @"images\employee");
               var extension = Path.GetExtension(file.FileName);

               if (employee.ImageUrl != null)
               {
                   var oldImagePath = Path.Combine(wwwRootPath, employee.ImageUrl.TrimStart('\\'));
                   if (System.IO.File.Exists(oldImagePath))
                   {
                       System.IO.File.Delete(oldImagePath);
                   }
               }

               using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
               {
                   file.CopyTo(fileStreams);
               }
               employee.ImageUrl = @"\images\employee\" + fileName + extension;

           }
           _unitOfWork.Employees.Update(employee);
           _unitOfWork.SaveChanges();
           TempData["success"] = "Employee added successfully";
           return RedirectToAction("Index");

       }
       return View(employee);
   }
   

            [Authorize]
            [HttpGet]
            public IActionResult Delete(Guid? id)
            {
                var employee = _unitOfWork.Employees.GetFirstOrDefault(u=>u.Id==id);
                employee.CreatedBy = employee.CreatedBy;

                if (employee == null)
                {
                    return NotFound();
                }

                return View(employee);
            }
            
            
             [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public IActionResult Delete(Guid id, Employee employee)
            {
                var employeeFromDb = _unitOfWork.Employees.GetFirstOrDefault(u=>u.Id==id);
                if (employeeFromDb == null)
                {
                    return NotFound();
                }

                _unitOfWork.Employees.Remove(employeeFromDb);
                _unitOfWork.SaveChanges();
                TempData["success"] = "Employee deleted successfully";
                return RedirectToAction("Index");
            }

            //Get Details
        [Authorize]
        [HttpGet]       
        public IActionResult Details(Guid? id)
        {
            var employee = _unitOfWork.Employees.GetFirstOrDefault(u=>u.Id==id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u => u.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }
    
}


        