using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TowerScaffolding.Models
{
    public class LeadingHand
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Code")]
        public int ID { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Scaffold Ticket")]
        public string ScaffoldTicket { get; set; }

        [Display(Name = "Drivers Licence")]
        public string DriversLicence { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public ICollection<Task> Tasks { get; set; }
    }
}