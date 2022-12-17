﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fresh724.Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Fresh724.Entity.Entities
{
    public class Employee:EntityBase
    {
       
        [Required]
        [Display(Name = "First Name")]
        [Column(TypeName = "nvarchar(15)")]
        public string FirstName { get; set; }=string.Empty;
        
        [Required]
        [Display(Name = "Last Name")]
        [Column(TypeName = "nvarchar(15)")]
        public string LastName { get; set; }=string.Empty;
        
        [Required]
        [Display(Name = "Email")]
        [Column(TypeName = "nvarchar(15)")]
        public string Email{ get; set; }=string.Empty;
        public Guid  CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        
     
        [Display(Name = "ImageUrl")]
        [ValidateNever]
        public string ImageUrl { get; set; }
        
    }
}
