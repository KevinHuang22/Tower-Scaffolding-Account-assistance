using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    public class LeadingHandsController : Controller
    {
        private readonly TowerScaffoldingContext _context;
        public LeadingHandsController(TowerScaffoldingContext context)
        {
            _context = context;
        }

        // GET: LeadingHands
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentPage"] = "Index";
            var leadinghands = from lh in _context.LeadingHands
                               select lh;
            leadinghands = leadinghands.Include(lh => lh.Tasks)
                    .ThenInclude(lh => lh.Site)
                        .ThenInclude(lh => lh.Customer)
                        .AsNoTracking();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["codeSortParm"] = string.IsNullOrEmpty(sortOrder) ? "code" : "";
            ViewData["lNameSortParm"] = sortOrder == "lname" ? "lname_desc" : "lname";
            ViewData["fNameSortParm"] = sortOrder == "fname" ? "fname_desc" : "fname";

            if (searchString != null) page = 1;
            else searchString = currentFilter;

            ViewData["CurrentFilter"] = searchString;

            if (!string.IsNullOrEmpty(searchString))
            {
                leadinghands = leadinghands.Where(lh => lh.LastName.Contains(searchString) || lh.FirstName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "code":
                    leadinghands = leadinghands.OrderBy(lh => lh.ID); break;
                case "lname":
                    leadinghands = leadinghands.OrderBy(lh => lh.LastName); break;
                case "lname_desc":
                    leadinghands = leadinghands.OrderByDescending(lh => lh.LastName); break;
                case "fname":
                    leadinghands = leadinghands.OrderBy(lh => lh.FirstName); break;
                case "fname_desc":
                    leadinghands = leadinghands.OrderByDescending(lh => lh.FirstName); break;
                default:
                    leadinghands = leadinghands.OrderByDescending(lh => lh.ID); break;
            }
            int pageSize = 30;
            return View(await PaginatedList<LeadingHand>.CreateAsync(leadinghands.AsNoTracking(), page ?? 1, pageSize));
        }

        // GET: LeadingHands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leadingHand = await _context.LeadingHands
                .Include(lh => lh.Tasks)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (leadingHand == null)
            {
                return NotFound();
            }

            return View(leadingHand);
        }

        // GET: LeadingHands/Create
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: LeadingHands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant,Manager")]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,ScaffoldTicket,DriversLicence")] LeadingHand leadingHand)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    leadingHand.FirstName = string.IsNullOrEmpty(leadingHand.FirstName) ? leadingHand.LastName : leadingHand.FirstName;
                    leadingHand.LastName = string.IsNullOrEmpty(leadingHand.LastName) ? leadingHand.FirstName : leadingHand.LastName;

                    _context.Add(leadingHand);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /*Ex*/)
            {
                //if (Ex.Number == 2627)
                //{
                    string sErrorMsg = "Leading hand's Code havs been allocated, please choose another one.";
                    ModelState.AddModelError("", sErrorMsg);
                //}
            }
            return View(leadingHand);
        }

        // GET: LeadingHands/Edit/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leadingHand = await _context.LeadingHands
                .AsNoTracking()
                .SingleOrDefaultAsync(lh=>lh.ID == id);
            if (leadingHand == null)
            {
                return NotFound();
            }
            return View(leadingHand);
        }

        // POST: LeadingHands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int? id, byte[] rowVersion)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leadingHandToUpdate = await _context.LeadingHands.SingleOrDefaultAsync(m => m.ID == id);

            if(leadingHandToUpdate == null)
            {
                LeadingHand deletedLeadingHand = new LeadingHand();
                await TryUpdateModelAsync(deletedLeadingHand);
                ModelState.AddModelError(string.Empty, "Unable to save changes.The customer was deleted by another user.");
                return View(deletedLeadingHand);
            }

            _context.Entry(leadingHandToUpdate).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync<LeadingHand>(
               leadingHandToUpdate, "", lh => lh.FirstName, lh => lh.LastName, lh => lh.ScaffoldTicket, lh => lh.DriversLicence))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (LeadingHand)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save changes. This Leading Hand was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (LeadingHand)databaseEntry.ToObject();

                        if (databaseValues.FirstName != clientValues.FirstName)
                        {
                            ModelState.AddModelError("FirstName", $"Current First Name: {databaseValues.FirstName}");
                        }
                        if (databaseValues.LastName != clientValues.LastName)
                        {
                            ModelState.AddModelError("LastName", $"Current Last Name: {databaseValues.LastName}");
                        }
                        if (databaseValues.ScaffoldTicket != clientValues.ScaffoldTicket)
                        {
                            ModelState.AddModelError("ScaffoldTicket", $"Current ScaffoldTicket: {databaseValues.ScaffoldTicket}");
                        }
                        if (databaseValues.DriversLicence != clientValues.DriversLicence)
                        {
                            ModelState.AddModelError("DriversLicence", $"Current DriversLicence: {databaseValues.DriversLicence}");
                        }

                        ModelState.AddModelError("", "The record you attempted to edit " +
                            "was modified by another user after you got the original value.");
                        ModelState.AddModelError("", "The edit operation was canceled and the current valurs in the database " +
                            "have been displayed.");
                        ModelState.AddModelError("", "If you still want to edit this record, click the save button again. " +
                            "Otherwise click the Back to list to the previous page.");
                        leadingHandToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
            }
            return View(leadingHandToUpdate);
        }

        // GET: LeadingHands/Delete/5
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leadingHand = await _context.LeadingHands
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (leadingHand == null)
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
            return View(leadingHand);
        }

        // POST: LeadingHands/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(LeadingHand leadingHand)
        {
            try{
                if (await _context.LeadingHands.AnyAsync(m => m.ID == leadingHand.ID))
                {
                    _context.LeadingHands.Remove(leadingHand);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = leadingHand.ID, concurrencyError = true });
            }
        }

        private bool LeadingHandExists(int id)
        {
            return _context.LeadingHands.Any(e => e.ID == id);
        }
    }
}
