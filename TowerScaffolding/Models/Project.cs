using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TowerScaffolding.Models
{
    public enum Status
    {
        Pending, Active, Finished
    }
    public enum Branch
    {
        Auckland, Christchurch, Tauranga, Timaru
    }
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SiteID { get; set; }
        public int CustomerID { get; set; }
        public Branch Branch { get; set; }
        public string Site { get; set; }
        public string Address { get; set; }

        [Display(Name = "Tower mangager")]
        public string TowerManager { get; set; }

        [Display(Name = "Site mangager")]
        public string SiteManager { get; set; }
        public Status Status { get; set; }
        public string QS { get; set; }
        public string Invoice { get; set; }
        public string Quote { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Customer Customer { get; set; }
        public ICollection<Task> Tasks { get; set; }
    }
}
