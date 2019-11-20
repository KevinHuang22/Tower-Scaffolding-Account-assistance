using TowerScaffolding.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerScaffolding.Data
{
    public static class DbInitializer
    {
        public static void Initialize(TowerScaffoldingContext context)
        {
            context.Database.EnsureCreated();

            //Look for any customers.
            if (context.Customers.Any()) { return; } //DB has been created.

            var customers = new Customer[]
            {
                new Customer{CustomerName = "ANZ", Email = "ANZ@gmail.com"},
            };
            foreach (Customer c in customers)
            {
                context.Customers.Add(c);
            }
            context.SaveChanges();

            //var projects = new Project[]
            //{
                
            //};
            //foreach (Project p in projects)
            //{
            //    context.Projects.Add(p);
            //}
            //context.SaveChanges();

            var leadingHands = new LeadingHand[]
            {
                new LeadingHand {ID = 1, FirstName = "Tongmei", LastName = "Lee", ScaffoldTicket = "05555", DriversLicence = "A029383", }
            };
            foreach (LeadingHand l in leadingHands)
            {
                context.LeadingHands.Add(l);
            }
            context.SaveChanges();

            var tasks = new Models.Task[]
            {
                new Models.Task{Date = DateTime.Parse("2018-06-17"), SiteID = 1, WorkDescription = "blablabla...", Progress = "18%", LeadingHandID = 1, Staff = "Thomas, Jim", NumberOfStaff = 2, U=1,S=2,H=3, Start = new TimeSpan(09,08,10), Finish = new TimeSpan(18,5,60), Vehicle = "AAA555", Returned = Returned.OnTime, Quality = Quality.Good, TaskStatus = 0,},
            };
            foreach (Models.Task t in tasks)
            {
                context.Tasks.Add(t);
            }
            context.SaveChanges();

            //var dayWorks = new DayWork[]
            //{
            //    new DayWork {DayWorkID = 101, Date = DateTime.Parse("2018-06-17"), TaskID = 101, Description = "The description of day work here", Type = "The type of day work here", Truck = "The truck of day work here", Scaffolder = "The Scaffolder of day work here", NumOfWorkers = 11, Uom = "The Uom of day work here", Qty = 13, },
            //};
            //foreach(DayWork dw in dayWorks)
            //{
            //    context.DayWorks.Add(dw);
            //}
            //context.SaveChanges();
        }
    }
}
