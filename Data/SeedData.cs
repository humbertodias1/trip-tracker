using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TripTracker.Models;

namespace TripTracker.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        if (!await db.PackingTemplates.AnyAsync())
        {
            var templates = new List<PackingTemplate>
            {
                new()
                {
                    Name = "Carry-on",
                    Items =
                    {
                        new PackingTemplateItem { Name = "Passport", Quantity = 1, Category = "Documents" },
                        new PackingTemplateItem { Name = "Phone Charger", Quantity = 1, Category = "Electronics" },
                        new PackingTemplateItem { Name = "Medication", Quantity = 1, Category = "Health" }
                    }
                },
                new()
                {
                    Name = "International",
                    Items =
                    {
                        new PackingTemplateItem { Name = "Travel Adapter", Quantity = 1, Category = "Electronics" },
                        new PackingTemplateItem { Name = "Copies of ID", Quantity = 2, Category = "Documents" },
                        new PackingTemplateItem { Name = "Local Currency", Quantity = 1, Category = "Money" }
                    }
                },
                new()
                {
                    Name = "Winter",
                    Items =
                    {
                        new PackingTemplateItem { Name = "Thermal Jacket", Quantity = 1, Category = "Clothing" },
                        new PackingTemplateItem { Name = "Gloves", Quantity = 1, Category = "Clothing" },
                        new PackingTemplateItem { Name = "Beanie", Quantity = 1, Category = "Clothing" }
                    }
                },
                new()
                {
                    Name = "Beach",
                    Items =
                    {
                        new PackingTemplateItem { Name = "Swimsuit", Quantity = 1, Category = "Clothing" },
                        new PackingTemplateItem { Name = "Sunscreen", Quantity = 1, Category = "Health" },
                        new PackingTemplateItem { Name = "Flip Flops", Quantity = 1, Category = "Footwear" }
                    }
                }
            };

            db.PackingTemplates.AddRange(templates);
            await db.SaveChangesAsync();
        }

        const string demoEmail = "demo@triptracker.local";
        const string demoPassword = "Demo123!";

        var demoUser = await userManager.FindByEmailAsync(demoEmail);
        if (demoUser is null)
        {
            demoUser = new ApplicationUser
            {
                UserName = demoEmail,
                Email = demoEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(demoUser, demoPassword);
            if (!result.Succeeded)
            {
                return;
            }
        }

        if (!await db.Trips.AnyAsync(t => t.UserId == demoUser.Id))
        {
            var sampleTrip = new Trip
            {
                UserId = demoUser.Id,
                Name = "Tokyo Spring Trip",
                Destinations = "Tokyo, Kyoto",
                StartDate = DateTime.Today.AddDays(14),
                EndDate = DateTime.Today.AddDays(20),
                HomeCurrency = "USD",
                Notes = "Cherry blossom season and food tour."
            };

            db.Trips.Add(sampleTrip);
            await db.SaveChangesAsync();

            db.ItineraryItems.AddRange(
                new ItineraryItem
                {
                    TripId = sampleTrip.Id,
                    Title = "Flight to Tokyo",
                    Type = ItineraryItemType.Flight,
                    StartDateTime = sampleTrip.StartDate.AddHours(8),
                    EndDateTime = sampleTrip.StartDate.AddHours(18),
                    Status = ItineraryStatus.Booked,
                    ConfirmationNumber = "JL-4832"
                },
                new ItineraryItem
                {
                    TripId = sampleTrip.Id,
                    Title = "Shibuya Food Walk",
                    Type = ItineraryItemType.Activity,
                    StartDateTime = sampleTrip.StartDate.AddDays(1).AddHours(17),
                    Status = ItineraryStatus.Planned,
                    LocationName = "Shibuya"
                });

            db.Expenses.Add(new Expense
            {
                TripId = sampleTrip.Id,
                Date = sampleTrip.StartDate,
                Category = ExpenseCategory.Transport,
                OriginalAmount = 1200m,
                OriginalCurrency = "USD",
                ConvertedAmount = 1200m,
                PaidBy = "Demo User",
                Notes = "Round trip flights"
            });

            db.PackingItems.Add(new PackingItem
            {
                TripId = sampleTrip.Id,
                Name = "Passport",
                Quantity = 1,
                Category = "Documents",
                IsPacked = false
            });

            db.DocumentLinks.Add(new DocumentLink
            {
                TripId = sampleTrip.Id,
                Title = "Hotel Reservation",
                Url = "https://example.com/reservation",
                Notes = "Keep confirmation handy"
            });

            await db.SaveChangesAsync();
        }
    }
}
