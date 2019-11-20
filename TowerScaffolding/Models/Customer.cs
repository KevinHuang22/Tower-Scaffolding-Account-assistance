using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TowerScaffolding.Models
{
    public class Customer
    {
        public int ID { get; set; }
        [Display(Name = "Customer")]
        [StringLength(30, ErrorMessage = "The customer name could not be longer than 30 characters")]
        public string CustomerName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}