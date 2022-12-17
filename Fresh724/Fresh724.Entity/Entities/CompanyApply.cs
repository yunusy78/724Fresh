using System.ComponentModel;

namespace Fresh724.Entity.Entities;

public class CompanyApply
{
    public Guid Id { get; set; }
    
    

    public string CompanyName { get; set; }=string.Empty;
    public string CompanyPhone { get; set; }=string.Empty;
    public string CompanyEmail { get; set; }=string.Empty;
    public string CompanyState { get; set; }=string.Empty;
    
    [DefaultValue("Pending")]
    public string ApplicationStatus { get; set; }=string.Empty;
    public enum Status
    {
        Pending, Approved, Rejected,Processing,Completed,Cancelled
    }
    
}