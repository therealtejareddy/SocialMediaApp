using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaApp.Areas.Identity.Data;

// Add profile data for application users by adding properties to the SocialMediaAppUser class
public class SocialMediaAppUser : IdentityUser
{
	
	[PersonalData]
    [Column(TypeName = "varchar(max)")]
	public string? uname { get; set; }
    [PersonalData]
    [Column(TypeName = "varchar(max)")]
    public string? fullName { get; set; }
    [PersonalData]
    [Column(TypeName = "varchar(max)")]
    public string? Bio { get; set; }

    [PersonalData]
    [Column(TypeName = "varchar(max)")]
    public string? Education { get; set; }
    [PersonalData]
    [Column(TypeName = "varchar(max)")]
    public string? Work { get; set; }
    [PersonalData]
    [Column(TypeName = "varchar(max)")]
    public string? Country { get; set; }
    [PersonalData]
    [Column(TypeName = "varchar(max)")]
    public string? Address { get; set; }
    [PersonalData]
    [Column(TypeName ="varchar(max)")]
    public string? profilePicURL { get; set; }
    [PersonalData]
    [Column(TypeName = "varchar(max)")]
    public string? coverPicURL { get; set; }

    // Social Media Links
    // Location

}

