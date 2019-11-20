using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TowerScaffolding.Data;
using TowerScaffolding.Models;
using TowerScaffolding.Models.TowerScaffoldingViewModels;

namespace TowerScaffolding.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TowerScaffoldingContext _context;

        public CustomersController(TowerScaffoldingContext context)
        {
            _context = context;
        }
        // GET: Customers
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentPage"] = "Index";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null) page = 1;
            else searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;
            var customers = from c in _context.Customers
                            select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.CustomerName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    customers = customers.OrderByDescending(c => c.CustomerName); break;
                default:
                    customers = customers.OrderBy(c => c.CustomerName); break;
            }
            int pageSize = 30;
            return View(await PaginatedList<Customer>.CreateAsync(customers.AsNoTracking(), page ?? 1, pageSize));

        }
        public async Task<IActionResult> CustomerIndexDetail(int? id, int? projectID, string sortOrder, string currentFillter, string searchString)
        {
            ViewData["CurrentPage"] = "CustomerIndexDetail";
            var viewModel = new CustomerIndexData();
            viewModel.Customers = await _context.Customers
            .Include(c => c.Projects)
                .ThenInclude(c => c.Tasks)
            .AsNoTracking()
            .ToListAsync();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["IDSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["AddressSortParm"] = sortOrder == "Address" ? "address_desc" : "Address";
            ViewData["TMSortParm"] = sortOrder == "TM" ? "tm_desc" : "TM";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";

            if (searchString == null) searchString = currentFillter;
            ViewData["CurrentFilter"] = searchString;

            if (!string.IsNullOrEmpty(searchString))
            {
                viewModel.Projects = viewModel.Projects.Where(p => p.Site.Contains(searchString) || p.SiteManager.Contains(searchString)
                                          || p.TowerManager.Contains(searchString) || p.Status.ToString().Contains(searchString));
            }

            if (id != null)
            {
                ViewData["ID"] = id.Value;
                Customer customer = viewModel.Customers.Where(c => c.ID == id.Value).Single();
                viewModel.Projects = customer.Projects;
            }
            if (projectID != null)
            {
                ViewData["projectId"] = projectID.Value;
                viewModel.Tasks = viewModel.Projects.Where(p => p.SiteID == projectID).Single().Tasks;
            }

            switch (sortOrder)
            {
                case "id_desc":
                    viewModel.Projects = viewModel.Projects.OrderByDescending(p => p.SiteID); break;
                case "Name":
                    viewModel.Projects = viewModel.Projects.OrderBy(p => p.Site); break;
                case "name_desc":
                    viewModel.Projects = viewModel.Projects.OrderByDescending(p => p.Site); break;
                case "Address":
                    viewModel.Projects = viewModel.Projects.OrderBy(p => p.Address); break;
                case "address_desc":
                    viewModel.Projects = viewModel.Projects.OrderByDescending(p => p.Address); break;
                case "TM":
                    viewModel.Projects = viewModel.Projects.OrderBy(p => p.TowerManager); break;
                case "tm_desc":
                    viewModel.Projects = viewModel.Projects.OrderByDescending(p => p.TowerManager); break;
                case "Status":
                    viewModel.Projects = viewModel.Projects.OrderBy(p => p.Status); break;
                case "status_desc":
                    viewModel.Projects = viewModel.Projects.OrderByDescending(p => p.Status); break;
                default:
                    viewModel.Projects = viewModel.Projects.OrderBy(p => p.SiteID); break;
            }
            return View(viewModel);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id, string sortOrder)
        {
            if (id == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers
                .Include(c => c.Projects)
                //.ThenInclude(p => p.Tasks)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);

            if (customer == null)
            {
                return NotFound();
            }

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            var projects = from p in _context.Projects
                           where p.CustomerID == id
                           select p;
            switch (sortOrder)
            {
                case "name_desc":
                    projects = projects.OrderByDescending(p => p.Site); break;
                default:
                    projects = projects.OrderBy(p => p.Site); break;
            }

            return View(await projects.AsNoTracking().ToListAsync());
        }
        // GET: Customers/Create
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public async Task<IActionResult> Create([Bind("ID,CustomerName,Email")] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                "Try again, and if the problem persists " +
                "see your system administrator.");
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.ID == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id, byte[] rowVersion)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerToUpdate = await _context.Customers.SingleOrDefaultAsync(c => c.ID == id);

            if (customerToUpdate == null)
            {
                Customer deletedCustomer = new Customer();
                await TryUpdateModelAsync(deletedCustomer);
                ModelState.AddModelError(string.Empty, "Unable to save changes. The customer was deleted by another user.");
                return View(deletedCustomer);
            }

            _context.Entry(customerToUpdate).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync<Customer>(customerToUpdate, "", c => c.CustomerName, c => c.Email))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Customer)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save changes. The customer was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Customer)databaseEntry.ToObject();

                        if (databaseValues.CustomerName != clientValues.CustomerName)
                        {
                            ModelState.AddModelError("CustomerName", $"Current customer: {databaseValues.CustomerName}");
                        }
                        if (databaseValues.Email != clientValues.Email)
                        {
                            ModelState.AddModelError("Email", $"Current email: {databaseValues.Email}");
                        }

                        ModelState.AddModelError("", "The record you attempted to edit " +
                            "was modified by another user after you got the original value.");
                        ModelState.AddModelError("", "The edit operation was canceled and the current valurs in the database " +
                            "have been displayed.");
                        ModelState.AddModelError("", "If you still want to edit this record, click the save button again. " +
                            "Otherwise click the Back to list to the previous page.");
                        customerToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
            }
            return View(customerToUpdate);
        }

        // GET: Customers/Delete/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);

            if (customer == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }

            if (concurrencyError.GetValueOrDefault())
            {
                ViewData["ConcurrencyErrorMessage"] = "Dalete failed. "
                    + "The record you attempted to delete "
                    + "was modified by another user afer you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. "
                    + "If you still want to delete this record, click the Delete button again."
                    + " Otherwise click the Back to List to the previous page.";
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(Customer customer)
        {
            try
            {
                if (await _context.Customers.AnyAsync(c=>c.ID == customer.ID))
                {
                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = customer.ID, concurrencyError = true });
            }
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.ID == id);
        }
    }
}
