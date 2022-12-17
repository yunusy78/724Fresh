using Fresh724.Data.Context;
using Fresh724.Data.Repository.Abstract;
using Fresh724.Entity.Entities;
using Fresh724.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using X.PagedList.Mvc.Core;
namespace Fresh724.Web.Controllers;


public class ContactController : Controller
{ 
    private readonly ILogger<ContactController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _um;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IEmailSender _emailSender;

    public ContactController(ILogger<ContactController> logger,IEmailSender emailSender, IUnitOfWork unitOfWork,IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> um)
    {
        
        _logger = logger;
        _unitOfWork = unitOfWork;
        _um = um;
        _hostEnvironment = hostEnvironment;
        _emailSender = emailSender;
        
    }
    
    
    [Authorize(Roles = RoleService.Role_Admin )]
    public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
    {
        var user = _um.GetUserAsync(User).Result;
        ViewBag.CurrentSort = sortOrder;
        ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewBag.EmailSortParm =string.IsNullOrEmpty(sortOrder) ? "email_desc" : "";

        if (searchString != null)
        {
            page = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        ViewBag.CurrentFilter = searchString;

        var company = from s in _unitOfWork.CompanyApplies.GetAll()
            select s;
        if (!string.IsNullOrEmpty(searchString))
        {
            company = company.Where(s => s.CompanyName.Contains(searchString)
                                         || s.CompanyEmail.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                company = company.OrderByDescending(s => s.CompanyName);
                break;
            case "email_desc":
                company = company.OrderByDescending(s => s.CompanyEmail);
                break;
         

        }

       
        int pageSize;
        int pageNumber;
        
                    pageSize = 3;
                    pageNumber = (page ?? 1);
                    return View(company.ToPagedList(pageNumber, pageSize));

    }
    
   
    [HttpGet]
    public IActionResult CompanyAdd()
    {
        return View();
    }
    
        
    [HttpPost]
    public IActionResult CompanyAdd(CompanyApply company)
        {
            ModelState.Clear();

            // Reevaluate the model with the added fields
            if (TryValidateModel(company))
            {
                _unitOfWork.CompanyApplies.Add(company);
                _unitOfWork.SaveChanges();
                _emailSender.SendEmailAsync(company.CompanyEmail, "New Apply - Fresh724", "<p>Thank You!  Your application has been received. You can be sure that our technical team will contact you as soon as possible.! </p>");
                _emailSender.SendEmailAsync("fresh724fresh@gmail.com", "New Apply - company", "<p>New Apply </p>");
                return RedirectToAction("Success", "Contact");
            }
            return View(company);
        }
    
    
    [Authorize]
    [HttpGet]
    public IActionResult Edit(Guid? id)
    {
        if(id==null || id == Guid.Empty)
        {
            return NotFound();
        }
        
        var companyApply = _unitOfWork.CompanyApplies.GetFirstOrDefault(u=>u.Id==id);
        
        if (companyApply == null)
        {
            return NotFound();
        }

        return View(companyApply);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyApply companyApply)
    {
        var user = _um.GetUserAsync(User).Result;
        ModelState.Clear();

        // Reevaluate the model with the added fields
        if (TryValidateModel(companyApply))
        {
            _unitOfWork.CompanyApplies.Update(companyApply);
            _unitOfWork.SaveChanges();
            if (companyApply.ApplicationStatus == StatusService.InProcess)
            {
                _emailSender.SendEmailAsync(companyApply.CompanyEmail, "New Apply - Fresh724", "<p>Thank You!  Your application InProcess! </p>"); 
            }
            if (companyApply.ApplicationStatus == StatusService.Rejected)
            {
                _emailSender.SendEmailAsync(companyApply.CompanyEmail, "New Apply - Fresh724", "<p>Thank You!  Your application Rejected! </p>"); 
            }
            if (companyApply.ApplicationStatus == StatusService.Completed)
            {
                _emailSender.SendEmailAsync(companyApply.CompanyEmail, "New Apply - Fresh724", "<p>Thank You!  Your application Completed! </p>"); 
            }
            
            return RedirectToAction("Index");
        }

        return View(companyApply);
    }
    
    // GET: Address/Delete/5
    public IActionResult Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var companyApply =  _unitOfWork.CompanyApplies.GetFirstOrDefault(m => m.Id == id);
        
        if (companyApply == null)
        {
            return NotFound();
        }

        return View(companyApply);
    }
    
    
    // POST: Application/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        var application =  _unitOfWork.CompanyApplies.GetFirstOrDefault(u => u.Id == id);;
        _unitOfWork.CompanyApplies.Remove(application);
        _unitOfWork.SaveChanges();
        TempData["success"] = "Address deleted successfully";
        return RedirectToAction(nameof(Index));
    }

    
    
    
    public IActionResult Success()
    {
        return View();
    }
}
    
  
    