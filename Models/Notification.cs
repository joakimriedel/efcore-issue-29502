using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class DailyDigest
    {
        public int Id { get; set; }
        
        public ApplicationUser? User { get; set; }
        public string? UserId { get; set; }
    }
}
