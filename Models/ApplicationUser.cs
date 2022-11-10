using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public DateTime TimeCreatedUtc { get; set; }

        public ICollection<DailyDigest> DailyDigests {get;set;} = null!;
    }
}
