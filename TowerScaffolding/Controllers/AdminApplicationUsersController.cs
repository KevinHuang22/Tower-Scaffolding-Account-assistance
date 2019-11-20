using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TowerScaffolding.Data;
using TowerScaffolding.Models;

namespace TowerScaffolding.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminApplicationUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        //private IEnumerable<ApplicationUser> members;
        //private IEnumerable<ApplicationUser> accountants;
        private IQueryable<ApplicationUser> staffs;

        public AdminApplicationUsersController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        }


        // GET: ApplicationUsers Role is member
        public async Task<IActionResult> Index(string branch, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentPage"] = "Index";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["fNameSortParm"] = sortOrder == "fName" ? "fname_desc" : "fName";
            ViewData["lNameSortParm"] = sortOrder == "lName" ? "lname_desc" : "lName";
            ViewData["roleSortParm"] = string.IsNullOrEmpty(sortOrder)? "role_desc" : "";

            if (searchString != null) page = 1;
            else searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;

            staffs = ReturnAllStaffs(searchString);

            if (null != branch)
            {
                staffs = staffs.Where(s => s.Branch.ToString().Contains(branch));
                ViewData["branch"] = branch;
            }

            switch (sortOrder)
            {
                case "fName":
                    staffs = staffs.OrderBy(s => s.FirstName); break;
                case "fName_desc":
                    staffs = staffs.OrderByDescending(s => s.FirstName); break;
                case "lName":
                    staffs = staffs.OrderBy(s => s.LastName); break;
                case "lName_desc":
                    staffs = staffs.OrderByDescending(s => s.LastName); break;
                case "role_desc":
                    staffs = staffs.OrderByDescending(s => s.UserRoles.SingleOrDefault().Role.Name); break;
                default: staffs = staffs.OrderBy(s => s.UserRoles.SingleOrDefault().Role.Name); break;
            }
            int pageSize = 10;
            return View(await PaginatedList<ApplicationUser>.CreateAsync(staffs.AsNoTracking(), page ?? 1, pageSize));
        }

        public IQueryable<ApplicationUser> ReturnAllStaffs(string searchString = null)
        {
            //members = await _userManager.GetUsersInRoleAsync("Member");
            //accountants = await _userManager.GetUsersInRoleAsync("Accountant");
            //var users = (members ?? Enumerable.Empty<ApplicationUser>()).Concat(accountants ?? Enumerable.Empty<ApplicationUser>());
            var users = from ur in _userManager.Users
                        select ur;
            users = users.Include(u => u.UserRoles)
                         .ThenInclude(u => u.Role);
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.FirstName.Contains(searchString) || s.LastName.Contains(searchString));
            }
            return users;
        }

        public async Task<IActionResult> EnableDisable(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            staffs = ReturnAllStaffs();
            ApplicationUser staff = staffs.SingleOrDefault(u => u.Id == id);
            if (staff == null)
            {
                return NotFound();
            }
            staff.Enabled = !staff.Enabled;
            _context.Update(staff);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        ////GET: ApplicationUsers/Details/5
        //    public async Task<IActionResult> Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var applicationUser = await _context.ApplicationUser
        //        .SingleOrDefaultAsync(m => m.Id == id);
        //    if (applicationUser == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(applicationUser);
        //}

        // GET: ApplicationUsers/Create
        public IActionResult Create()
        {
            PopulateApplicationDetails();
            return View();
        }

        // POST: ApplicationUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Enabled,UserName,Branch,FirstName,LastName,Id,UserName,Email,Phone")] ApplicationUser applicationUser)
        {
            var newUser = new ApplicationUser
            {
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                Branch = applicationUser.Branch,
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                Phone = string.IsNullOrEmpty(applicationUser.Phone) ? "" : applicationUser.Phone,
                Enabled = true
            };

            try
            {
                string UserPassword = "P@ssw0rd";
                var createPowerUser = await _userManager.CreateAsync(newUser, UserPassword);
                if (createPowerUser.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "MemBer");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.");
            }

            PopulateApplicationDetails(applicationUser);

            return View(applicationUser);
        }

        private void PopulateApplicationDetails(object selectApplicationUser = null)
        {
            ViewBag.Branch = new List<SelectListItem>
            {
                new SelectListItem {Text = Branch.Auckland.ToString(), Value = Branch.Auckland.ToString()},
                new SelectListItem {Text = Branch.Christchurch.ToString(), Value = "1"},
                new SelectListItem {Text = Branch.Tauranga.ToString(), Value = "2"},
                new SelectListItem {Text = Branch.Timaru.ToString(), Value = "3"},
            };
            var allRoles = new List<ApplicationRole>();
            foreach (var role in _context.Roles)
            {
                allRoles.Add(new ApplicationRole() { Name = role.Name, Id = role.Id });
            }
            ViewBag.Roles = allRoles;
        }

        // GET: ApplicationUsers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.ApplicationUser
                                .Include(au => au.UserRoles)
                                .ThenInclude(au => au.Role)
                                .AsNoTracking()
                                .SingleOrDefaultAsync(au => au.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }
            PopulateApplicationDetails(applicationUser);
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string[] selectedRoles)
        {
            if (id == null)
            {
                return NotFound();
            }
            var applicationUserToUpdate = await _context.ApplicationUser
            .Include(au => au.UserRoles)
            .ThenInclude(au => au.Role)
            .SingleOrDefaultAsync(au => au.Id == id);
            if (await TryUpdateModelAsync<ApplicationUser>(applicationUserToUpdate, "",
            au => au.LastName, au => au.FirstName, au => au.Enabled, au => au.Phone, au => au.PhoneNumberConfirmed, au => au.UserName,
            au => au.Email, au => au.Branch, au => au.EmailConfirmed))
            {
                UpdateApplicationUserRoles(selectedRoles, applicationUserToUpdate);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateApplicationDetails(applicationUserToUpdate);
            return View(applicationUserToUpdate);
        }
        private void UpdateApplicationUserRoles(string[] selectedRoles, ApplicationUser applicationUserToUpdate)
        {
            if (selectedRoles == null)
            {
                applicationUserToUpdate.UserRoles = new List<ApplicationUserRole>();
                return;
            }
            var rolesToAssignHS = new HashSet<string>(selectedRoles);
            var assignedRoles = new HashSet<string>(applicationUserToUpdate.UserRoles.Select(ur => ur.Role.Id));
            foreach (var rolePot in _context.ApplicationRole)
            {
                if (rolesToAssignHS.Contains(rolePot.Id))
                {
                    if (!assignedRoles.Contains(rolePot.Id))
                    {
                        applicationUserToUpdate.UserRoles.Add(new ApplicationUserRole
                        {
                            UserId = applicationUserToUpdate.Id,
                            RoleId = rolePot.Id
                        });
                    }
                }
                else
                {
                    if (assignedRoles.Contains(rolePot.Id))
                    {
                        ApplicationUserRole roleToRemove = applicationUserToUpdate.UserRoles.SingleOrDefault(au => au.RoleId == rolePot.Id);
                        _context.Remove(roleToRemove);
                    }
                }
            }
        }

        // GET: ApplicationUsers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.ApplicationUser
                .SingleOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: ApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var applicationUser = await _context.ApplicationUser.SingleOrDefaultAsync(m => m.Id == id);
            _context.ApplicationUser.Remove(applicationUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicationUserExists(string id)
        {
            return _context.ApplicationUser.Any(e => e.Id == id);
        }
    }
}
