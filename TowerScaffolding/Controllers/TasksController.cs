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
using TowerScaffolding.Models.TowerScaffoldingViewModels;

namespace TowerScaffolding.Controllers
{
    public class TasksController : Controller
    {
        private readonly TowerScaffoldingContext _context;

        public TasksController(TowerScaffoldingContext context)
        {
            _context = context;
        }

        public IQueryable<Models.Task> TasksQuery(DateTime? start, DateTime? end, string searchString, int? lhId)
        {
            var tasks = from c in _context.Tasks
                        select c;
            tasks = tasks.Include(t => t.Site)
                .ThenInclude(t => t.Customer)
                .Include(t => t.LeadingHand);

            if (!string.IsNullOrEmpty(searchString))
            {
                //var SEARCHSTRING = searchString.ToUpper();
                tasks = tasks.Where(t => t.Site.Site.Contains(searchString)
                                      || searchString.Contains(t.LeadingHand.LastName)
                                      || searchString.Contains(t.LeadingHand.FirstName)
                                      || t.Site.Customer.CustomerName.Contains(searchString)
                                      || t.LeadingHand.LastName.Contains(searchString)
                                      || t.LeadingHand.FirstName.Contains(searchString)
                                      );
            }

            if (null != lhId)
            {
                tasks = tasks.Where(t => t.LeadingHandID.Equals(lhId));
            }
            if (start != null && end != null)
            {
                tasks = tasks.Where(dw => dw.Date.CompareTo(start) >= 0 && dw.Date.CompareTo(end) <= 0);

                DateTime Start = new DateTime();
                Start = Convert.ToDateTime(start);
                string staDate = Start.ToString("yyyy-MM-dd");

                DateTime End = new DateTime();
                End = Convert.ToDateTime(end);
                string endDate = End.ToString("yyyy-MM-dd");

                ViewData["Start"] = staDate;
                ViewData["End"] = endDate;
            }
            return tasks;
        }
        // GET: Tasks
        public async Task<IActionResult> Index(string sortOrder, DateTime? start, DateTime? end,
                            string searchString, string currentFilter, string returned, string status,
                            int? page, int? lhId = null,  bool export = false)
        {
            //var tasks = from c in _context.Tasks
            //            select c;
            //tasks = tasks.Include(t => t.Site).ThenInclude(t => t.Customer).Include(t => t.LeadingHand);

            ViewData["CurrentPage"] = "Index";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["LHID"] = lhId;
            ViewData["IDSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CustomerSortParm"] = sortOrder == "Customer" ? "customer_desc" : "Customer";
            ViewData["QualitySortParm"] = sortOrder == "Quality" ? "quality_desc" : "Quality";
            ViewData["SiteSortParm"] = sortOrder == "Site" ? "site_desc" : "Site";
            ViewData["LHSortParm"] = sortOrder == "LH" ? "lh_desc" : "LH";
            ViewData["TotalSortParm"] = sortOrder == "Total" ? "total_desc" : "Total";

            if (searchString != null) page = 1;
            else searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;

            var tasks = TasksQuery(start, end, searchString, lhId);

            if (!string.IsNullOrEmpty(returned))
            {
                tasks = tasks.Where(t => t.Returned.ToString() == returned);
                ViewData["Returned"] = returned;
            }
            if(!string.IsNullOrEmpty(status))
            {
                tasks = tasks.Where(t => t.TaskStatus.ToString() == status);
                ViewData["Status"] = status;
            }
            switch (sortOrder)
            {
                case "Site":
                    tasks = tasks.OrderBy(t => t.Site.Site); break;
                case "site_desc":
                    tasks = tasks.OrderByDescending(t => t.Site.Site); break;
                case "Date":
                    tasks = tasks.OrderBy(t => t.Date); break;
                case "date_desc":
                    tasks = tasks.OrderByDescending(t => t.Date); break;
                case "Customer":
                    tasks = tasks.OrderBy(t => t.Site.Customer.CustomerName); break;
                case "customer_desc":
                    tasks = tasks.OrderByDescending(t => t.Site.Customer.CustomerName); break;
                case "Quality":
                    tasks = tasks.OrderBy(t => t.Quality); break;
                case "quality_desc":
                    tasks = tasks.OrderByDescending(t => t.Quality); break;
                case "LH":
                    tasks = tasks.OrderBy(t => t.LeadingHand.LastName); break;
                case "lh_desc":
                    tasks = tasks.OrderByDescending(t => t.LeadingHand.LastName); break;
                case "Total":
                    tasks = tasks.OrderBy(t => t.Total); break;
                case "total_desc":
                    tasks = tasks.OrderByDescending(t => t.Total); break;
                case "Returned":
                    tasks = tasks.OrderBy(t => t.Returned); break;
                case "returned_desc":
                    tasks = tasks.OrderByDescending(t => t.Returned); break;
                case "Status":
                    tasks = tasks.OrderBy(t => t.TaskStatus); break;
                case "status_desc":
                    tasks = tasks.OrderByDescending(t => t.TaskStatus); break;
                default:
                    tasks = tasks.OrderByDescending(t => t.TaskID); break;
            }
            int pageSize = 30;
            return View(await PaginatedList<Models.Task>.CreateAsync(tasks.AsNoTracking(), page ?? 1, pageSize));
        }
        public async Task<IActionResult> ViewDayWorks(int? id, string sortOrder, string currentFillter, string searchString, string preUrl,
                                                      int? lhId = null, bool isNewCreate = false)
        {
            if (preUrl == null)
            {
                ViewBag.preUrl = Request.Headers["Referer"].ToString();
            }
            else
            {
                ViewBag.preUrl = preUrl;
            }
            if (isNewCreate)
            {
                TempData["alertMessage"] = "New Task has been created succussfully!!";
            }
            ViewData["CurrentPage"] = "ViewDayWorks";
            ViewData["LHID"] = lhId;
            ViewData["TaskID"] = id.Value;
            var viewModel = new ViewDayWorks
            {
                Tasks = await _context.Tasks
                            .Include(t => t.LeadingHand)
                            .Include(t => t.Site)
                            .ThenInclude(t => t.Customer)
                            .Where(t => t.TaskID == id.Value)
                            .AsNoTracking()
                            .ToListAsync(),
                DayWorks = await _context.DayWorks
                           .Where(dw => dw.TaskID.Equals(id))
                           .OrderBy(dw => dw.DayWorkID)
                           .AsNoTracking()
                           .ToListAsync()
            };
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IDSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["QtySortParm"] = sortOrder == "Qty" ? "qty_desc" : "Qty";
            ViewData["TypeSortParm"] = sortOrder == "Site" ? "type_desc" : "Type";

            if (searchString == null) searchString = currentFillter;
            ViewData["CurrentFilter"] = searchString;
            if (!string.IsNullOrEmpty(searchString))
            {
                viewModel.DayWorks = viewModel.DayWorks.Where(dw => dw.DayWorkID.ToString().Contains(searchString) || dw.Scaffolder.Contains(searchString)
                                          || dw.Type.Contains(searchString) || dw.Truck.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "id":
                    viewModel.DayWorks = viewModel.DayWorks.OrderBy(dw => dw.DayWorkID); break;
                case "Date":
                    viewModel.DayWorks = viewModel.DayWorks.OrderBy(dw => dw.Date); break;
                case "date_desc":
                    viewModel.DayWorks = viewModel.DayWorks.OrderByDescending(dw => dw.Date); break;
                case "Quality":
                    viewModel.DayWorks = viewModel.DayWorks.OrderBy(dw => dw.Qty); break;
                case "quality_desc":
                    viewModel.DayWorks = viewModel.DayWorks.OrderByDescending(dw => dw.Qty); break;
                case "Type":
                    viewModel.DayWorks = viewModel.DayWorks.OrderBy(dw => dw.Type); break;
                case "type_desc":
                    viewModel.DayWorks = viewModel.DayWorks.OrderByDescending(dw => dw.Type); break;
                default:
                    viewModel.DayWorks = viewModel.DayWorks.OrderByDescending(dw => dw.TaskID); break;
            }
            return View(viewModel);
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id, string preUrl)
        {
            if (preUrl == null)
            {
                ViewBag.preUrl = Request.Headers["Referer"].ToString();
            }
            else
            {
                ViewBag.preUrl = preUrl;
            }
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.LeadingHand)
                .Include(t => t.Site)
                .FirstOrDefaultAsync(m => m.TaskID == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        private void PopulateDropDownList(object selectedValue = null)
        {
            var leadingHandsQuery = from lh in _context.LeadingHands
                                    orderby lh.LastName
                                    select lh;
            ViewBag.LeadingHandID = new SelectList(leadingHandsQuery.AsNoTracking(), "ID", "FullName", selectedValue);

            var projectsQuery = from p in _context.Projects
                                orderby p.Site
                                select p;
            ViewBag.ProjectID = new SelectList(projectsQuery.AsNoTracking(), "SiteID", "Site", selectedValue);

            ViewBag.Returned = new List<SelectListItem>
            {
                new SelectListItem {Text = Returned.Assisted.ToString(), Value = Returned.Assisted.ToString()},
                new SelectListItem {Text = Returned.Late.ToString(), Value = Returned.Late.ToString()},
                new SelectListItem {Text = Returned.Missing.ToString(), Value = Returned.Missing.ToString()},
                new SelectListItem {Text = Returned.OnTime.ToString(), Value = Returned.OnTime.ToString()},
            };

            ViewBag.Quality = new List<SelectListItem>
            {
                new SelectListItem {Text = Quality.Good.ToString(), Value = Quality.Good.ToString()},
                new SelectListItem {Text = Quality.Bad.ToString(), Value = Quality.Bad.ToString()},
                new SelectListItem {Text = Quality.Average.ToString(), Value = Quality.Average.ToString()},
            };
            ViewBag.TaskStatus = new List<SelectListItem>
            {
                new SelectListItem {Text = Models.TaskStatus.Verified.ToString(), Value = Models.TaskStatus.Verified.ToString()},
                new SelectListItem {Text = Models.TaskStatus.Unverified.ToString(), Value = Models.TaskStatus.Unverified.ToString()},
                new SelectListItem {Text = Models.TaskStatus.Invoiced.ToString(), Value = Models.TaskStatus.Invoiced.ToString()},
            };
        }
        // GET: Tasks/Create
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public IActionResult Create(string preUrl)
        {
            if (preUrl == null)
            {
                ViewBag.preUrl = Request.Headers["Referer"].ToString();
            }
            else
            {
                ViewBag.preUrl = preUrl;
            }
            //ViewData["LeadingHandID"] = new SelectList(_context.LeadingHands, "LeadingHandID", "LeadingHandID");
            //ViewData["SiteID"] = new SelectList(_context.Projects, "SiteID", "SiteID");
            PopulateDropDownList();
            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public async Task<IActionResult> Create([Bind("Date,SiteID,LeadingHandID,U,S,H,WorkDescription,Staff,NumberOfStaff,Progress,Start,Finish,Vehicle,Returned,Quality,TaskStatus,Comment")] Models.Task task, string preUrl)
        {
            if (task.Finish - task.Start < new TimeSpan(1))
            {
                ModelState.AddModelError("", "Finish time must greater than start time.");
            }
            if (ModelState.IsValid)
            {
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewDayWorks", new { id = task.TaskID, isNewCreate = true, preUrl});
            }
            //ViewData["LeadingHandID"] = new SelectList(_context.LeadingHands, "LeadingHandID", "LeadingHandID", task.LeadingHandID);
            //ViewData["SiteID"] = new SelectList(_context.Projects, "SiteID", "SiteID", task.SiteID);
            PopulateDropDownList(task.TaskID);
            return View(task);
        }

        // GET: Tasks/Edit/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id, string preUrl)
        {
            if(preUrl == null)
            {
                ViewBag.preUrl = Request.Headers["Referer"].ToString();
            }
            else
            {
                ViewBag.preUrl = preUrl;
            }
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .AsNoTracking()
                .SingleOrDefaultAsync(t => t.TaskID == id);

            if (task == null)
            {
                return NotFound();
            }
            //ViewData["LeadingHandID"] = new SelectList(_context.LeadingHands, "LeadingHandID", "LeadingHandID", task.LeadingHandID);
            //ViewData["SiteID"] = new SelectList(_context.Projects, "SiteID", "SiteID", task.SiteID);
            PopulateDropDownList(task.TaskID);
            return View(task);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id, string submit, string preUrl, byte[] rowVersion, Models.Task obj)
        {
            switch (submit)
            {
                case "Save As New Task":
                    { 
                    IActionResult newTask = await Create(obj, preUrl);
                    return newTask;
                    }
            }

            if (id == null)
            {
                return NotFound();
            }

            var taskToUpdate = await _context.Tasks.SingleOrDefaultAsync(t => t.TaskID == id);

            if (taskToUpdate == null)
            {
                Models.Task deletedTask = new Models.Task();
                await TryUpdateModelAsync(deletedTask);
                ModelState.AddModelError(string.Empty, "Unable to save changes. This task was deleted by another user.");
                PopulateDropDownList(taskToUpdate.TaskID);
                return View(deletedTask);
            }

            _context.Entry(taskToUpdate).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync(taskToUpdate, "", t => t.Date, t => t.SiteID, t => t.LeadingHandID, t => t.TaskStatus,
                                                            t => t.Staff, t => t.Start, t => t.Finish, t => t.U, t => t.S, t => t.H,
                                                            t => t.Vehicle, t => t.NumberOfStaff, t => t.WorkDescription,
                                                            t => t.Progress, t => t.Quality, t => t.Returned, t => t.Comment))
            {
                try
                {
                    if (taskToUpdate.Finish - taskToUpdate.Start < new TimeSpan(1))
                    {
                        ModelState.AddModelError("", "Finish time must greater than start time.");
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                        return Redirect(preUrl);
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValue = (Models.Task)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save changes. The customer was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Models.Task)databaseEntry.ToObject();

                        if (databaseValues.Date != clientValue.Date)
                        {
                            ModelState.AddModelError("Date", $"Current Date: {databaseValues.Date}");
                        }
                        if (databaseValues.LeadingHandID != clientValue.LeadingHandID)
                        {
                            ModelState.AddModelError("LeadingHandID", $"Current Leading Hand: {databaseValues.LeadingHand.FullName}");
                        }
                        if (databaseValues.WorkDescription != clientValue.WorkDescription)
                        {
                            ModelState.AddModelError("WorkDescription", $"Current Work Description: {databaseValues.WorkDescription}");
                        }
                        if (databaseValues.Staff != clientValue.Staff)
                        {
                            ModelState.AddModelError("Staff", $"Current Staff: {databaseValues.Staff}");
                        }
                        if (databaseValues.NumberOfStaff != clientValue.NumberOfStaff)
                        {
                            ModelState.AddModelError("NumberOfStaff", $"Current Number Of Staff: {databaseValues.NumberOfStaff}");
                        }
                        if (databaseValues.Progress != clientValue.Progress)
                        {
                            ModelState.AddModelError("Progress", $"Current Progress: {databaseValues.Progress}");
                        }
                        if (databaseValues.Start != clientValue.Start)
                        {
                            ModelState.AddModelError("Start", $"Current Start: {databaseValues.Start}");
                        }
                        if (databaseValues.Finish != clientValue.Finish)
                        {
                            ModelState.AddModelError("Finish", $"Current Finish: {databaseValues.Finish}");
                        }
                        if (databaseValues.U != clientValue.U)
                        {
                            ModelState.AddModelError("U", $"Current U: {databaseValues.U}");
                        }
                        if (databaseValues.S != clientValue.S)
                        {
                            ModelState.AddModelError("S", $"Current S: {databaseValues.S}");
                        }
                        if (databaseValues.H != clientValue.H)
                        {
                            ModelState.AddModelError("H", $"Current H: {databaseValues.H}");
                        }
                        if (databaseValues.Returned != clientValue.Returned)
                        {
                            ModelState.AddModelError("Returned", $"Current Returned: {databaseValues.Returned.ToString()}");
                        }
                        if (databaseValues.Quality != clientValue.Quality)
                        {
                            ModelState.AddModelError("Quality", $"Current Quality: {databaseValues.Quality.ToString()}");
                        }
                        if (databaseValues.TaskStatus != clientValue.TaskStatus)
                        {
                            ModelState.AddModelError("TaskStatus", $"Current TaskStatus: {databaseValues.TaskStatus.ToString()}");
                        }
                        if (databaseValues.Comment != clientValue.Comment)
                        {
                            ModelState.AddModelError("Comment", $"Current Comment: {databaseValues.Comment}");
                        }

                        ModelState.AddModelError("", "The record you attempted to edit " +
                            "was modified by another user after you got the original value.");
                        ModelState.AddModelError("", "The edit operation was canceled and the current valurs in the database " +
                            "have been displayed.");
                        ModelState.AddModelError("", "If you still want to edit this record, click the save button again. " +
                            "Otherwise click the Back to list to the previous page.");
                        taskToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
            }
            PopulateDropDownList(taskToUpdate.TaskID);
            return View(taskToUpdate);
        }

        // GET: Tasks/Delete/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError, string preUrl)
        {
            if (preUrl == null)
            {
                ViewBag.preUrl = Request.Headers["Referer"].ToString();
            }
            else
            {
                ViewBag.preUrl = preUrl;
            }
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.LeadingHand)
                .Include(t => t.Site)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.TaskID == id);

            if (task == null)
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

            return View(task);
        }

        // POST: Tasks/Delete/5
        [Authorize(Roles = "Admin,Accountant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Models.Task task)
        {
            try
            {
                if (await _context.Tasks.AnyAsync(t => t.TaskID == task.TaskID))
                {
                    _context.Tasks.Remove(task);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = task.TaskID, concurrencyError = true });
            }
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.TaskID == id);
        }

        public FileResult ExportExcel(DateTime? start, DateTime? end, string searchString, int? lhId, string returned, string status)
        {
            var tasksExport = TasksQuery(start, end, searchString, lhId);
            if (!string.IsNullOrEmpty(returned))
            {
                tasksExport = tasksExport.Where(t => t.Returned.ToString() == returned);
            }
            if (!string.IsNullOrEmpty(status))
            {
                tasksExport = tasksExport.Where(t => t.TaskStatus.ToString() == status);
            }
            var sbHtml = new StringBuilder();
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            //string[] columnNames = typeof(Models.Task).GetProperties()
            //            .Select(property => property.Name)
            //            .ToArray();
            var lstTitle = new List<string> { "Task No.", "Date", "Customer", "Site", "Leading hand", "Work Description",
                                              "Progress", "Start", "Finish", "Total", "U", "S", "H", "Staff", "Number of Staff",
                                              "Vehicle", "Returned", "Quality", "Status", "Comment"};

            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var task in tasksExport)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.TaskID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Date);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Site.Customer.CustomerName);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Site.Site);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.LeadingHand.FullName);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.WorkDescription);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Progress);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Start);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Finish);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Total);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.U);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.S);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.H);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Staff);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.NumberOfStaff);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Vehicle);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Returned);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Quality);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.TaskStatus);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", task.Comment);
                sbHtml.Append("</tr>");
            }

            string Sday = "";
            string Eday = "";
            string Smonth = "";
            string Emonth = "";
            string Syear = "";
            string Eyear = "";
            if (null != start)
            {
                DateTime Start = new DateTime();
                Start = Convert.ToDateTime(start);
                DateTime End = new DateTime();
                End = Convert.ToDateTime(end);
                Sday = Start.ToString("dd");
                Eday = End.ToString("dd");
                Smonth = Start.ToString("MMM");
                Emonth = End.ToString("MMM");
                Syear = Start.ToString("yyyy");
                Eyear = End.Year.ToString();
                Syear = Eyear == Syear ? "" : Syear;
            }
            byte[] fileContents = Encoding.Default.GetBytes(sbHtml.ToString());
            return File(fileContents, "application/ms-excel", "Diary Summary "
                + Sday + " " + Smonth + " " + Syear + "  -  " + Eday + " " + Emonth + " " + Eyear + ".xls");

        }
    }
}
