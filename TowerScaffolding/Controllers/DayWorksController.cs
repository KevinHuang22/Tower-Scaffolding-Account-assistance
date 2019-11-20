using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
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
    public class DayWorksController : Controller
    {
        private readonly TowerScaffoldingContext _context;

        public DayWorksController(TowerScaffoldingContext context)
        {
            _context = context;
        }

        public IQueryable<DayWork> DayWorksQuery(DateTime? start, DateTime? end, string searchString)
        {
            var dayWorks = from dw in _context.DayWorks
                           select dw;
            dayWorks = dayWorks.Include(dw => dw.Task)
                    .ThenInclude(dw => dw.Site)
                    .ThenInclude(dw => dw.Customer)
                    .Include(dw => dw.Task)
                    .ThenInclude(dw => dw.LeadingHand);

            if (!string.IsNullOrEmpty(searchString))
            {
                dayWorks = dayWorks.Where(dw => dw.Task.Site.Site.Contains(searchString) || dw.Task.Site.Customer.CustomerName.Contains(searchString)
                                      || dw.Type.Contains(searchString) /*|| dw.TaskID.ToString().Contains(searchString)*/
                                      || dw.Scaffolder.Contains(searchString) || dw.Task.LeadingHand.LastName.Contains(searchString)
                                      || dw.Task.LeadingHand.FirstName.Contains(searchString) || dw.DayWorkID.ToString().Contains(searchString)
                                      );
            }

            if (start != null && end != null)
            {
                dayWorks = dayWorks.Where(dw => dw.Date.CompareTo(start) >= 0 && dw.Date.CompareTo(end) <= 0);

                DateTime Start = new DateTime();
                Start = Convert.ToDateTime(start);
                string staDate = Start.ToString("yyyy-MM-dd");

                DateTime End = new DateTime();
                End = Convert.ToDateTime(end);
                string endDate = End.ToString("yyyy-MM-dd");

                ViewData["Start"] = staDate;
                ViewData["End"] = endDate;
            }
            return dayWorks;
        }
        // GET: DayWorks
        public async Task<IActionResult> Index(int? id, DateTime? start, DateTime? end, string sortOrder, string currentFilter, string searchString, int? page)
        {
            //var dayWorks = from dw in _context.DayWorks
            //               select dw;
            //dayWorks = dayWorks.Include(dw => dw.Task)
            //        .ThenInclude(dw => dw.Site)
            //        .ThenInclude(dw => dw.Customer)
            //        .Include(dw => dw.Task)
            //        .ThenInclude(dw => dw.LeadingHand);

            ViewData["CurrentPage"] = "Index";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IDSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CustomerSortParm"] = sortOrder == "Customer" ? "customer_desc" : "Customer";
            ViewData["TaskSortParm"] = sortOrder == "Task" ? "task_desc" : "Task";
            ViewData["SiteSortParm"] = sortOrder == "Site" ? "site_desc" : "Site";
            ViewData["TypeSortParm"] = sortOrder == "Type" ? "type_desc" : "Type";
            ViewData["LHSortParm"] = sortOrder == "LH" ? "lh_desc" : "LH";

            if (searchString != null) page = 1;
            else searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;

            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    dayWorks = dayWorks.Where(dw => dw.Task.Site.Site.Contains(searchString) || dw.Task.Site.Customer.CustomerName.Contains(searchString)
            //                          || dw.Type.Contains(searchString) || dw.TaskID.ToString().Contains(searchString)
            //                          || dw.Scaffolder.Contains(searchString) || dw.Task.LeadingHand.LastName.Contains(searchString)
            //                          || dw.Task.LeadingHand.FirstName.Contains(searchString));

            //}

            //if (start != null && end != null)
            //{
            //    dayWorks = dayWorks.Where(dw => dw.Date.CompareTo(start) >= 0 && dw.Date.CompareTo(end) <= 0);

            //    DateTime Start = new DateTime();
            //    Start = Convert.ToDateTime(start);
            //    string staDate = Start.ToString("yyyy-MM-dd");

            //    DateTime End = new DateTime();
            //    End = Convert.ToDateTime(end);
            //    string endDate = End.ToString("yyyy-MM-dd");

            //    ViewData["Start"] = staDate;
            //    ViewData["End"] = endDate;
            //}

            var dayWorks = DayWorksQuery(start, end, searchString);

            switch (sortOrder)
            {
                case "id":
                    dayWorks = dayWorks.OrderBy(dw => dw.TaskID); break;
                case "Site":
                    dayWorks = dayWorks.OrderBy(dw => dw.Task.Site.Site); break;
                case "site_desc":
                    dayWorks = dayWorks.OrderByDescending(dw => dw.Task.Site.Site); break;
                case "Date":
                    dayWorks = dayWorks.OrderBy(dw => dw.Date); break;
                case "date_desc":
                    dayWorks = dayWorks.OrderByDescending(dw => dw.Date); break;
                case "Customer":
                    dayWorks = dayWorks.OrderBy(dw => dw.Task.Site.Customer.CustomerName); break;
                case "customer_desc":
                    dayWorks = dayWorks.OrderByDescending(dw => dw.Task.Site.Customer.CustomerName); break;
                case "Task":
                    dayWorks = dayWorks.OrderBy(dw => dw.Task); break;
                case "task_desc":
                    dayWorks = dayWorks.OrderByDescending(dw => dw.Task); break;
                case "Tydwe":
                    dayWorks = dayWorks.OrderBy(dw => dw.Type); break;
                case "type_desc":
                    dayWorks = dayWorks.OrderByDescending(dw => dw.Type); break;
                case "LH":
                    dayWorks = dayWorks.OrderBy(dw => dw.Task.LeadingHand.LastName); break;
                case "lh_desc":
                    dayWorks = dayWorks.OrderByDescending(dw => dw.Task.LeadingHand.LastName); break;
                default:
                    dayWorks = dayWorks.OrderByDescending(dw => dw.TaskID); break;
            }
            int pageSize = 30;
            return View(await PaginatedList<DayWork>.CreateAsync(dayWorks.AsNoTracking(), page ?? 1, pageSize));
        }

        // GET: DayWorks/Details/5
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

            var dayWork = await _context.DayWorks
                .Include(dw => dw.Task)
                    .ThenInclude(dw => dw.Site)
                    .ThenInclude(dw => dw.Customer)
                .Include(dw => dw.Task)
                    .ThenInclude(dw => dw.LeadingHand)
                .FirstOrDefaultAsync(m => m.DayWorkID == id);
            if (dayWork == null)
            {
                return NotFound();
            }

            return View(dayWork);
        }

        // GET: DayWorks/Create
        public IActionResult Create(int taskId, string preUrl)
        {
            if (preUrl == null)
            {
                ViewBag.preUrl = Request.Headers["Referer"].ToString();
            }
            else
            {
                ViewBag.preUrl = preUrl;
            }
            //ViewData["TaskID"] = new SelectList(_context.Tasks, "TaskID", "TaskID", taskId);
            ViewData["TaskID"] = taskId;
            return View();
        }

        // POST: DayWorks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DayWorkID,Date,TaskID,Type,Description,Qty,Truck,Scaffolder,NumOfWorkers")] DayWork dayWork, string preUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(dayWork);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewDayWorks", "Tasks", new { id = dayWork.TaskID, preUrl });
                }
            }
            catch (DbUpdateException ex)
            {
                SqlException innerException = ex.InnerException as SqlException;
                switch (innerException.Number)
                {
                    case 2627: //Violation of primary key. Handle Exception
                            ModelState.AddModelError("", "Unable to create. " +
                                "Assign a new Day Work No. and try again." +
                                "if the problem persists, please contact your system administrator.");
                        break;
                    case 2601:
                        ModelState.AddModelError("", "Unable to save creation. " +
                       "Assign a new Day Work No. and try again." +
                       "if the problem persists, please contact your system administrator."); break;
                    default:
                        ModelState.AddModelError("", "Unable to save changes. " +
                            "Try again, and if the problem persists, " +
                            "please contact your system administrator.");
                        break;
                }
                //Log the error (uncomment ex variable name and write a log.
                Console.Write(ex);
                
            }
            return View(dayWork);
        }

        // GET: DayWorks/Edit/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id, string preUrl)
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

            var dayWork = await _context.DayWorks
                .AsNoTracking()
                .SingleOrDefaultAsync(dw => dw.DayWorkID == id);
            if (dayWork == null)
            {
                return NotFound();
            }
            PopulateDropDownList(dayWork.TaskID);
            return View(dayWork);
        }

        // POST: DayWorks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id, string submit, byte[] rowVersion, DayWork obj, string preUrl)
        {
            switch (submit)
            {
                case "Save As New D/W":
                    {
                        IActionResult newDW = await Create(obj, preUrl);
                        return newDW;
                    }
            }
            
            if (id == null)
            {
                return NotFound();
            }

            var dayWorkToUpdate = await _context.DayWorks.SingleOrDefaultAsync(dw => dw.DayWorkID == id);

            if (dayWorkToUpdate == null)
            {
                DayWork deletedDayWork = new DayWork();
                await TryUpdateModelAsync(deletedDayWork);
                ModelState.AddModelError(string.Empty, "Unable to save changes. The Day Work you want to update wa deleted by another user.");
                return View(deletedDayWork);
            }

            _context.Entry(dayWorkToUpdate).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync(dayWorkToUpdate, "", dw => dw.Date, dw => dw.Type,
                                                               dw => dw.Description, dw => dw.Qty,
                                                               dw => dw.Truck, dw => dw.Scaffolder, dw => dw.NumOfWorkers))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(preUrl);
                }
                catch (DbUpdateConcurrencyException  ex )
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValue = (DayWork)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save changes. This Day work was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (DayWork)databaseEntry.ToObject();

                        if (databaseValues.Date != clientValue.Date)
                        {
                            ModelState.AddModelError("Date", $"Current value: {databaseValues.Date}");
                        }
                        if (databaseValues.Type != clientValue.Type)
                        {
                            ModelState.AddModelError("Type", $"Current Type: {databaseValues.Type}");
                        }
                        if (databaseValues.Description != clientValue.Description)
                        {
                            ModelState.AddModelError("Description", $"Current Description: {databaseValues.Description}");
                        }
                        if (databaseValues.Qty != clientValue.Qty)
                        {
                            ModelState.AddModelError("Qty", $"Current Qty: {databaseValues.Qty}");
                        }
                        if (databaseValues.Truck != clientValue.Truck)
                        {
                            ModelState.AddModelError("Truck", $"Current Truck: {databaseValues.Truck}");
                        }
                        if (databaseValues.Scaffolder != clientValue.Scaffolder)
                        {
                            ModelState.AddModelError("Scaffolder", $"Current Scaffolder: {databaseValues.Scaffolder}");
                        }
                        if (databaseValues.NumOfWorkers != clientValue.NumOfWorkers)
                        {
                            ModelState.AddModelError("NumOfWorkers", $"Current NumOfWorkers: {databaseValues.NumOfWorkers}");
                        }

                        ModelState.AddModelError("", "The record you attempted to edit " +
                            "was modified by another user after you got the original value.");
                        ModelState.AddModelError("", "The edit operation was canceled and the current valurs in the database " +
                            "have been displayed.");
                        ModelState.AddModelError("", "If you still want to edit this record, click the save button again. " +
                            "Otherwise click the Back to list to the previous page.");
                        dayWorkToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
            }
            //ViewData["TaskID"] = new SelectList(_context.Tasks, "TaskID", "TaskID", dayWorkToUpdate.TaskID);
            PopulateDropDownList(dayWorkToUpdate.TaskID);
            return View(dayWorkToUpdate);
        }
        private void PopulateDropDownList(object selectedValue = null)
        {
            var taskQuery = _context.Tasks
                                    .Include(t => t.LeadingHand)
                                    .Include(t => t.Site)
                                    //orderby t.TaskID
                                    .Select(t => new
                                    {
                                        TaskID = t.TaskID,
                                        Site_Date_LH = string.Format("{0}  -  {1:dd/MM/yyyy}  -  {2}", t.Site.Site, t.Date, t.LeadingHand.FullName)
                                    })
                                    .OrderByDescending(t => t.TaskID)
                                    .ToList();
            ViewBag.TaskID = new SelectList(taskQuery, "TaskID", "Site_Date_LH", selectedValue);
        }
        // GET: DayWorks/Delete/5
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

            var dayWork = await _context.DayWorks
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.DayWorkID == id);
            if (dayWork == null)
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

            return View(dayWork);
        }

        // POST: DayWorks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(DayWork dayWork)
        {
            try
            {
                if(await _context.DayWorks.AnyAsync(dw => dw.DayWorkID == dayWork.DayWorkID))
                {
                    _context.DayWorks.Remove(dayWork);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch(DbUpdateConcurrencyException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = dayWork.DayWorkID, concurrencyError = true });
            }
        }

        private bool DayWorkExists(int id)
        {
            return _context.DayWorks.Any(e => e.DayWorkID == id);
        }

        public FileResult ExportExcel(DateTime? start, DateTime? end, string searchString)
        {
            var dayWorksExport = DayWorksQuery(start, end, searchString);
            var sbHtml = new StringBuilder();
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { "Day Work No.", "Date", "Customer", "Site", "Type", "Leading hand", "Qty",
                                              "Uom", "Truck", "Scaffolder", "NumOfWorkers", "Desciption"};
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            foreach (var dayWork in dayWorksExport)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.DayWorkID);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Date);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Task.Site.Customer.CustomerName);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Task.Site.Site);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Type);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Task.LeadingHand.FullName);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Qty);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Uom);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Truck);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Scaffolder);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.NumOfWorkers);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", dayWork.Description);
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
            return File(fileContents, "application/ms-excel", "Day Work Summary "
                + Sday + " " + Smonth + " " + Syear + "  -  " + Eday + " " + Emonth + " " + Eyear + ".xls");

        }
    }
}
