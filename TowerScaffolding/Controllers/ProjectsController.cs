using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TowerScaffolding.Data;
using TowerScaffolding.Models;

namespace TowerScaffolding.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly TowerScaffoldingContext _context;

        public ProjectsController(TowerScaffoldingContext context)
        {
            _context = context;
        }

        public IQueryable<Project> ProjectQuery(int? customerId, string searchString)
        {
            var projects = from p in _context.Projects
                           select p;
            projects = projects.Include(p => p.Customer);

            if (customerId != null)
            {
                projects = projects.Where(p => p.CustomerID.Equals(customerId));
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                projects = projects.Where(p => p.Site.Contains(searchString) || p.SiteManager.Contains(searchString)
                                          || p.TowerManager.Contains(searchString) || p.Customer.CustomerName.Contains(searchString)
                                          || p.SiteID.ToString().Contains(searchString));
            }
            return projects;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int? customerId = null)
        {
            //var customerID = id;
            //var projects = from p in _context.Projects
            //               select p;
            //projects = projects.Include(p => p.Customer);
            ViewData["CustomerId"] = customerId;
            ViewData["CurrentPage"] = "Index";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IDSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["AddressSortParm"] = sortOrder == "Address" ? "address_desc" : "Address";
            ViewData["TMSortParm"] = sortOrder == "TM" ? "tm_desc" : "TM";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";

            if (searchString != null) page = 1;
            else searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;
            //if (customerID != null)
            //{
            //    projects = projects.Where(p => p.CustomerID.Equals(customerID));
            //}
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    projects = projects.Where(p => p.Site.Contains(searchString) || p.SiteManager.Contains(searchString)
            //                              || p.TowerManager.Contains(searchString) || p.Customer.CustomerName.Contains(searchString)
            //                              /*|| p.Status.ToString().Contains(searchString)*/);
            //}
            var projects = ProjectQuery(customerId, searchString);
            switch (sortOrder)
            {
                case "id_desc":
                    projects = projects.OrderByDescending(p => p.SiteID); break;
                case "Name":
                    projects = projects.OrderBy(p => p.Site); break;
                case "name_desc":
                    projects = projects.OrderByDescending(p => p.Site); break;
                case "Address":
                    projects = projects.OrderBy(p => p.Address); break;
                case "address_desc":
                    projects = projects.OrderByDescending(p => p.Address); break;
                case "TM":
                    projects = projects.OrderBy(p => p.TowerManager); break;
                case "tm_desc":
                    projects = projects.OrderByDescending(p => p.TowerManager); break;
                case "Status":
                    projects = projects.OrderBy(p => p.Status); break;
                case "status_desc":
                    projects = projects.OrderByDescending(p => p.Status); break;
                default:
                    projects = projects.OrderBy(p => p.SiteID); break;
            }

            int pageSize = 30;
            return View(await PaginatedList<Project>.CreateAsync(projects.AsNoTracking(), page ?? 1, pageSize));
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Customer)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.SiteID == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public IActionResult Create()
        {
            //ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "ID");
            //ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "CustomerName");
            PopulateDropDownList();
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public async Task<IActionResult> Create([Bind("SiteID,CustomerID,Site,Address,TowerManager,Status,QS,SiteManager,Branch,Quote,Invoice")] Project project)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(project);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.");
            }

            //ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "CustomerName",project.CustomerID);
            PopulateDropDownList(project.CustomerID);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.SiteID == id);
            if (project == null)
            {
                return NotFound();
            }
            PopulateDropDownList(project.CustomerID);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> EditPost(int? id, byte[] rowVersion)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectToUpdate = await _context.Projects.SingleOrDefaultAsync(p => p.SiteID == id);

            if (projectToUpdate == null)
            {
                Project deletedProject = new Project();
                await TryUpdateModelAsync(deletedProject);
                PopulateDropDownList(projectToUpdate.CustomerID);
                ModelState.AddModelError(string.Empty, "Unable to save changes. The project was deleted by another user");
                return View(deletedProject);
            }

            _context.Entry(projectToUpdate).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync<Project>(projectToUpdate, "", p => p.Site, p => p.SiteManager, p => p.Status, p => p.TowerManager, p => p.QS,
                                                                        p => p.Address, p => p.CustomerID, p => p.Branch, p => p.Invoice, p => p.Quote))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Project)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save changes. The customer was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Project)databaseEntry.ToObject();

                        if (databaseValues.Site != clientValues.Site)
                        {
                            ModelState.AddModelError("Site", $"Current Site name: {databaseValues.Site}");
                        }
                        if (databaseValues.CustomerID != clientValues.CustomerID)
                        {
                            ModelState.AddModelError("CustomerID", $"Current customer: {databaseValues.Customer.CustomerName}");
                        }
                        if (databaseValues.Address != clientValues.Address)
                        {
                            ModelState.AddModelError("Address", $"Current Address: {databaseValues.Address}");
                        }
                        if (databaseValues.Branch != clientValues.Branch)
                        {
                            ModelState.AddModelError("Branch", $"Current Branch: {databaseValues.Branch.ToString()}");
                        }
                        if (databaseValues.TowerManager != clientValues.TowerManager)
                        {
                            ModelState.AddModelError("TowerManager", $"Current Tower Manager: {databaseValues.TowerManager}");
                        }
                        if (databaseValues.Status != clientValues.Status)
                        {
                            ModelState.AddModelError("Status", $"Current Status: {databaseValues.Status.ToString()}");
                        }
                        if (databaseValues.QS != clientValues.QS)
                        {
                            ModelState.AddModelError("QS", $"Current QS: {databaseValues.QS}");
                        }
                        if (databaseValues.SiteManager != clientValues.SiteManager)
                        {
                            ModelState.AddModelError("SiteManager", $"Current Site Manager: {databaseValues.SiteManager}");
                        }
                        if (databaseValues.Quote != clientValues.Quote)
                        {
                            ModelState.AddModelError("Quote", $"Current Quote: {databaseValues.Quote}");
                        }
                        if (databaseValues.Invoice != clientValues.Invoice)
                        {
                            ModelState.AddModelError("Invoice", $"Current Invoice: {databaseValues.Invoice}");
                        }

                        ModelState.AddModelError("", "The record you attempted to edit " +
                           "was modified by another user after you got the original value.");
                        ModelState.AddModelError("", "The edit operation was canceled and the current valurs in the database " +
                            "have been displayed.");
                        ModelState.AddModelError("", "If you still want to edit this record, click the save button again. " +
                            "Otherwise click the Back to list to the previous page.");
                        projectToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
            }
            PopulateDropDownList(projectToUpdate.CustomerID);
            return View(projectToUpdate);
        }

        private void PopulateDropDownList(object selectedCustomer = null)
        {
            var customersQuery = from c in _context.Customers
                                 orderby c.CustomerName
                                 select c;
            ViewBag.CustomerID = new SelectList(customersQuery.AsNoTracking(), "ID", "CustomerName", selectedCustomer);

            ViewBag.Status = new List<SelectListItem>
            {
                new SelectListItem {Text = Status.Pending.ToString(), Value = Status.Pending.ToString()},
                new SelectListItem {Text = Status.Active.ToString(), Value = "1"},
                new SelectListItem {Text = Status.Finished.ToString(), Value = "2"},
            };
            ViewBag.Branch = new List<SelectListItem>
            {
                new SelectListItem {Text = Branch.Auckland.ToString(), Value = Branch.Auckland.ToString()},
                new SelectListItem {Text = Branch.Christchurch.ToString(), Value = "1"},
                new SelectListItem {Text = Branch.Tauranga.ToString(), Value = "2"},
                new SelectListItem {Text = Branch.Timaru.ToString(), Value = "3"},
            };
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Customer)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.SiteID == id);
            if (project == null)
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

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(Project project)
        {
            try
            {
                if (await _context.Projects.AnyAsync(p => p.SiteID == project.SiteID))
                {

                    _context.Projects.Remove(project);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = project.SiteID, concurrencyError = true });
            }
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.SiteID == id);
        }

        public FileResult ExportExcel(int? customerId, string searchString)
        {
            var projectsExport = ProjectQuery(customerId, searchString);
            var sbHtml = new StringBuilder();
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");

            var lstTitle = new List<string> { "Site No.", "Site Name", "Branch", "Address", "Customer", "Site Manager",
                                              "Status", "Tower Manager", "QS", "Quote", "Invoice"};

            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var project in projectsExport)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.SiteID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.Site);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.Branch);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.Address);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.Customer.CustomerName);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.SiteManager);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.Status);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.TowerManager);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.QS);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.Quote);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", project.Invoice);
                sbHtml.Append("</tr>");
            }

            byte[] fileContents = Encoding.Default.GetBytes(sbHtml.ToString());
            return File(fileContents, "application/ms-excel", "Projects Summary " + " - " + ".xls");

        }
    }
}
