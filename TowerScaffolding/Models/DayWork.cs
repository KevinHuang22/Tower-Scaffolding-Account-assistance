using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TowerScaffolding.Models
{
    public class DayWork
    {
        [Display(Name = "Day work No.")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DayWorkID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Task No.")]
        public int TaskID { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public string Uom { get { return "hrs"; }  }
        public string Truck { get; set; }
        public string Scaffolder { get; set; }

        [Display(Name = "Number of workers")]
        public int NumOfWorkers { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Task Task { get; set; }
    }
}