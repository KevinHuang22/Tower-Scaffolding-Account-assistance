using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TowerScaffolding.Models
{
    public enum Returned
    {
        OnTime, Late, Missing, Assisted
    }
    public enum Quality
    {
        Good, Average, Bad
    }
    public enum TaskStatus
    {
        Unverified, Verified, Invoiced
    }
    public class Task
    {
        [Display(Name = "Task No.")]
        public int TaskID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        public int SiteID { get; set; }

        [Display(Name = "Code")]
        public int LeadingHandID { get; set; }

        [Display(Name = "Work description")]
        public string WorkDescription { get; set; }
        public string Progress { get; set; }
        public string Staff { get; set; }
        public int NumberOfStaff { get; set; }

        //[DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan Start { get; set; }

        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan Finish { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public double Total => ((Finish - Start) * NumberOfStaff).TotalHours;
        public string Vehicle { get; set; }
        public int U { get; set; }
        public int S { get; set; }
        public int H { get; set; }
        public Returned? Returned { get; set; }
        public Quality? Quality { get; set; }

        [Display(Name = "Status")]
        public TaskStatus TaskStatus { get; set; }
        public string Comment { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public ICollection<DayWork> DayWorks { get; set; }
        public Project Site { get; set; }
        public LeadingHand LeadingHand { get; set; }
    }
}
