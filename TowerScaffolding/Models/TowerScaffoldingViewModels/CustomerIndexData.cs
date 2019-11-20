using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerScaffolding.Models.TowerScaffoldingViewModels
{
    public class CustomerIndexData
    {
        public IEnumerable<Customer> Customers { get; set; }
        public IEnumerable<Project> Projects { get; set; }
        public IEnumerable<Task> Tasks { get; set; }
    }
}
