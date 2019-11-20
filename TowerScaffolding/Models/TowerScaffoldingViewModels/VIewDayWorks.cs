using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TowerScaffolding.Models.TowerScaffoldingViewModels
{
    public class ViewDayWorks
    {
        public IEnumerable<Task> Tasks { get; set; }
        public IEnumerable<DayWork> DayWorks { get; set; }
    }
}
