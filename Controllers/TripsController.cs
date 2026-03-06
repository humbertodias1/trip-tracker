using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripTracker.Data;
using TripTracker.Models;
using TripTracker.Services;
using TripTracker.ViewModels;

namespace TripTracker.Controllers;

[Authorize]
public class TripsController : Controller
{
    private readonly ApplicationDbContext _db;

    public TripsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var trips = await _db.Trips
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

        return View(trips);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Trip
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(3),
            HomeCurrency = "USD"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Trip model)
    {
        // UserId comes from the authenticated user, not from the form post.
        ModelState.Remove(nameof(Trip.UserId));
        ValidateTripDates(model.StartDate, model.EndDate);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.UserId = GetUserId();
        model.HomeCurrency = model.HomeCurrency.ToUpperInvariant();
        model.CreatedAt = DateTime.UtcNow;
        _db.Trips.Add(model);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Dashboard), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(int id, ItineraryItemType? typeFilter = null, ItineraryStatus? statusFilter = null, string tab = "itinerary")
    {
        var trip = await GetTripForUserAsync(id, includeRelated: true);
        if (trip is null)
        {
            return NotFound();
        }

        var filteredItems = trip.ItineraryItems.AsQueryable();
        if (typeFilter.HasValue)
        {
            filteredItems = filteredItems.Where(i => i.Type == typeFilter.Value);
        }
        if (statusFilter.HasValue)
        {
            filteredItems = filteredItems.Where(i => i.Status == statusFilter.Value);
        }

        var itineraryByDay = BuildItineraryByDay(trip, filteredItems.OrderBy(i => i.StartDateTime).ToList());
        var now = DateTime.Now;
        var nextItem = trip.ItineraryItems
            .Where(i => i.StartDateTime >= now && i.Status != ItineraryStatus.Canceled)
            .OrderBy(i => i.StartDateTime)
            .FirstOrDefault();

        var expenseTotals = trip.Expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.ConvertedAmount));

        var completion = trip.PackingItems.Count == 0
            ? 0
            : trip.PackingItems.Count(i => i.IsPacked) * 100.0 / trip.PackingItems.Count;

        var vm = new TripDashboardViewModel
        {
            Trip = trip,
            ItineraryDays = itineraryByDay,
            NextItem = nextItem,
            TypeFilter = typeFilter,
            StatusFilter = statusFilter,
            Expenses = trip.Expenses.OrderByDescending(e => e.Date).ToList(),
            ExpenseTotals = expenseTotals,
            ExpenseTotal = trip.Expenses.Sum(e => e.ConvertedAmount),
            PackingItems = trip.PackingItems.OrderBy(i => i.Category).ThenBy(i => i.Name).ToList(),
            PackingCompletionPercentage = completion,
            PackingTemplates = await _db.PackingTemplates.Include(t => t.Items).OrderBy(t => t.Name).ToListAsync(),
            DocumentLinks = trip.DocumentLinks.OrderBy(d => d.Title).ToList(),
            ActiveTab = tab
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(Trip model)
    {
        // UserId is not editable in settings and should not be required from the form.
        ModelState.Remove(nameof(Trip.UserId));
        var trip = await GetTripForUserAsync(model.Id);
        if (trip is null)
        {
            return NotFound();
        }

        ValidateTripDates(model.StartDate, model.EndDate);
        if (!ModelState.IsValid)
        {
            return await Dashboard(model.Id, tab: "settings");
        }

        trip.Name = model.Name;
        trip.Destinations = model.Destinations;
        trip.StartDate = model.StartDate.Date;
        trip.EndDate = model.EndDate.Date;
        trip.HomeCurrency = model.HomeCurrency.ToUpperInvariant();
        trip.Notes = model.Notes;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = trip.Id, tab = "settings" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var trip = await GetTripForUserAsync(id);
        if (trip is null)
        {
            return NotFound();
        }

        _db.Trips.Remove(trip);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItinerary(int tripId, ItineraryItem item)
    {
        var trip = await GetTripForUserAsync(tripId);
        if (trip is null)
        {
            return NotFound();
        }

        if (item.EndDateTime.HasValue && item.EndDateTime.Value < item.StartDateTime)
        {
            TempData["Error"] = "Itinerary end date/time cannot be before start date/time.";
            return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "itinerary" });
        }

        item.TripId = tripId;
        _db.ItineraryItems.Add(item);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "itinerary" });
    }

    [HttpGet]
    public async Task<IActionResult> EditItinerary(int id)
    {
        var item = await _db.ItineraryItems
            .Include(i => i.Trip)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (item?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItinerary(ItineraryItem model)
    {
        var existing = await _db.ItineraryItems
            .Include(i => i.Trip)
            .FirstOrDefaultAsync(i => i.Id == model.Id);
        if (existing?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }

        if (model.EndDateTime.HasValue && model.EndDateTime.Value < model.StartDateTime)
        {
            ModelState.AddModelError(nameof(model.EndDateTime), "End date/time cannot be before start date/time.");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        existing.Title = model.Title;
        existing.Type = model.Type;
        existing.StartDateTime = model.StartDateTime;
        existing.EndDateTime = model.EndDateTime;
        existing.LocationName = model.LocationName;
        existing.Address = model.Address;
        existing.MapLink = model.MapLink;
        existing.ConfirmationNumber = model.ConfirmationNumber;
        existing.Status = model.Status;
        existing.Notes = model.Notes;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Dashboard), new { id = existing.TripId, tab = "itinerary" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItinerary(int id)
    {
        var item = await _db.ItineraryItems.Include(i => i.Trip).FirstOrDefaultAsync(i => i.Id == id);
        if (item?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }

        var tripId = item.TripId;
        _db.ItineraryItems.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "itinerary" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExpense(int tripId, Expense expense)
    {
        var trip = await GetTripForUserAsync(tripId);
        if (trip is null)
        {
            return NotFound();
        }

        expense.TripId = tripId;
        expense.OriginalCurrency = expense.OriginalCurrency.ToUpperInvariant();
        if (!CurrencyEstimator.TryConvert(expense.OriginalAmount, expense.OriginalCurrency, trip.HomeCurrency, out var estimated))
        {
            TempData["Error"] = $"Unsupported currency code. Use: {CurrencyEstimator.SupportedCodes()}";
            return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "expenses" });
        }
        expense.ConvertedAmount = estimated;
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "expenses" });
    }

    [HttpGet]
    public async Task<IActionResult> EditExpense(int id)
    {
        var expense = await _db.Expenses.Include(e => e.Trip).FirstOrDefaultAsync(e => e.Id == id);
        if (expense?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        ViewBag.HomeCurrency = expense.Trip.HomeCurrency;
        return View(expense);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExpense(Expense model)
    {
        var existing = await _db.Expenses.Include(e => e.Trip).FirstOrDefaultAsync(e => e.Id == model.Id);
        if (existing?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        existing.Date = model.Date;
        existing.Category = model.Category;
        existing.OriginalAmount = model.OriginalAmount;
        existing.OriginalCurrency = model.OriginalCurrency.ToUpperInvariant();
        if (!CurrencyEstimator.TryConvert(existing.OriginalAmount, existing.OriginalCurrency, existing.Trip?.HomeCurrency ?? "USD", out var estimated))
        {
            ModelState.AddModelError(nameof(model.OriginalCurrency), $"Unsupported currency code. Use: {CurrencyEstimator.SupportedCodes()}");
        }
        if (!ModelState.IsValid)
        {
            ViewBag.HomeCurrency = existing.Trip?.HomeCurrency ?? "USD";
            return View(model);
        }
        existing.ConvertedAmount = estimated;
        existing.PaidBy = model.PaidBy;
        existing.SplitWith = model.SplitWith;
        existing.Notes = model.Notes;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = existing.TripId, tab = "expenses" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await _db.Expenses.Include(e => e.Trip).FirstOrDefaultAsync(e => e.Id == id);
        if (expense?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        var tripId = expense.TripId;
        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "expenses" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPackingItem(int tripId, PackingItem item)
    {
        var trip = await GetTripForUserAsync(tripId);
        if (trip is null)
        {
            return NotFound();
        }

        item.TripId = tripId;
        _db.PackingItems.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "packing" });
    }

    [HttpGet]
    public async Task<IActionResult> EditPackingItem(int id)
    {
        var item = await _db.PackingItems.Include(p => p.Trip).FirstOrDefaultAsync(p => p.Id == id);
        if (item?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPackingItem(PackingItem model)
    {
        var existing = await _db.PackingItems.Include(p => p.Trip).FirstOrDefaultAsync(p => p.Id == model.Id);
        if (existing?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        existing.Name = model.Name;
        existing.Quantity = model.Quantity;
        existing.Category = model.Category;
        existing.IsPacked = model.IsPacked;
        existing.AssignedTo = model.AssignedTo;
        existing.Notes = model.Notes;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = existing.TripId, tab = "packing" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePacked(int id)
    {
        var item = await _db.PackingItems.Include(p => p.Trip).FirstOrDefaultAsync(p => p.Id == id);
        if (item?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        item.IsPacked = !item.IsPacked;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = item.TripId, tab = "packing" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePackingItem(int id)
    {
        var item = await _db.PackingItems.Include(p => p.Trip).FirstOrDefaultAsync(p => p.Id == id);
        if (item?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        var tripId = item.TripId;
        _db.PackingItems.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "packing" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyTemplate(int tripId, int templateId)
    {
        var trip = await GetTripForUserAsync(tripId);
        if (trip is null)
        {
            return NotFound();
        }

        var template = await _db.PackingTemplates.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == templateId);
        if (template is null)
        {
            return NotFound();
        }

        var existingNames = await _db.PackingItems
            .Where(p => p.TripId == tripId)
            .Select(p => p.Name.ToLower())
            .ToListAsync();

        var itemsToAdd = template.Items
            .Where(i => !existingNames.Contains(i.Name.ToLower()))
            .Select(i => new PackingItem
            {
                TripId = tripId,
                Name = i.Name,
                Quantity = i.Quantity,
                Category = i.Category
            });

        _db.PackingItems.AddRange(itemsToAdd);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "packing" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDocumentLink(int tripId, DocumentLink link)
    {
        var trip = await GetTripForUserAsync(tripId);
        if (trip is null)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return await Dashboard(tripId, tab: "documents");
        }

        link.TripId = tripId;
        _db.DocumentLinks.Add(link);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "documents" });
    }

    [HttpGet]
    public async Task<IActionResult> EditDocumentLink(int id)
    {
        var link = await _db.DocumentLinks.Include(d => d.Trip).FirstOrDefaultAsync(d => d.Id == id);
        if (link?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        return View(link);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentLink(DocumentLink model)
    {
        var existing = await _db.DocumentLinks.Include(d => d.Trip).FirstOrDefaultAsync(d => d.Id == model.Id);
        if (existing?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        existing.Title = model.Title;
        existing.Url = model.Url;
        existing.Notes = model.Notes;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = existing.TripId, tab = "documents" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocumentLink(int id)
    {
        var link = await _db.DocumentLinks.Include(d => d.Trip).FirstOrDefaultAsync(d => d.Id == id);
        if (link?.Trip?.UserId != GetUserId())
        {
            return NotFound();
        }
        var tripId = link.TripId;
        _db.DocumentLinks.Remove(link);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Dashboard), new { id = tripId, tab = "documents" });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    private void ValidateTripDates(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date > endDate.Date)
        {
            ModelState.AddModelError(nameof(Trip.EndDate), "End date must be on or after start date.");
        }
    }

    private async Task<Trip?> GetTripForUserAsync(int tripId, bool includeRelated = false)
    {
        var userId = GetUserId();

        var query = _db.Trips.AsQueryable();
        if (includeRelated)
        {
            query = query
                .Include(t => t.ItineraryItems)
                .Include(t => t.Expenses)
                .Include(t => t.PackingItems)
                .Include(t => t.DocumentLinks);
        }

        return await query.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
    }

    private static IReadOnlyList<ItineraryDayGroupViewModel> BuildItineraryByDay(Trip trip, IReadOnlyList<ItineraryItem> items)
    {
        var grouped = items.GroupBy(i => i.StartDateTime.Date).ToDictionary(g => g.Key, g => g.ToList());
        var minItemDate = items.Count > 0 ? items.Min(i => i.StartDateTime.Date) : trip.StartDate.Date;
        var maxItemDate = items.Count > 0 ? items.Max(i => i.StartDateTime.Date) : trip.EndDate.Date;

        // Include itinerary entries even if they fall outside the trip date range.
        var current = trip.StartDate.Date < minItemDate ? trip.StartDate.Date : minItemDate;
        var end = trip.EndDate.Date > maxItemDate ? trip.EndDate.Date : maxItemDate;
        var result = new List<ItineraryDayGroupViewModel>();

        while (current <= end)
        {
            result.Add(new ItineraryDayGroupViewModel
            {
                Date = current,
                Items = grouped.TryGetValue(current, out var dayItems)
                    ? dayItems.OrderBy(i => i.StartDateTime).ToList()
                    : new List<ItineraryItem>()
            });
            current = current.AddDays(1);
        }

        return result;
    }
}
