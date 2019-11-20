using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerScaffolding.Models.TowerScaffoldingViewModels
{
    public class LeadingHandIndexData
    {
        public IEnumerable<LeadingHand> LeadingHands { get; set; }
        public IEnumerable<Task> Tasks { get; set; }
        public IEnumerable<DayWork> DayWorks { get; set; }
        public IEnumerable<Project> Sites { get; set; }
    }
}
