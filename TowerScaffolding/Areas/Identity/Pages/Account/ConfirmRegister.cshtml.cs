using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TowerScaffolding.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmRegisterModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}