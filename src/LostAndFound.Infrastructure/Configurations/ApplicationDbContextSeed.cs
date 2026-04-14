using System;
using System.Threading.Tasks;
using LostAndFound.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Infrastructure.Configurations;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            await context.Users.AddRangeAsync(
                new User { UserName = "admin", Email = "admin@university.edu", Name = "System Admin" },
                new User { UserName = "jdoe", Email = "jdoe@university.edu", Name = "John Doe" },
                new User { UserName = "msmith", Email = "msmith@university.edu", Name = "Mary Smith" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync()) 
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "Electronics" },
                new Category { Name = "Documents" },
                new Category { Name = "Personal Items" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Universities.AnyAsync()) 
        {
            await context.Universities.AddAsync(new University { Name = "Global University" });
            await context.SaveChangesAsync();
        }

        if (!await context.Departments.AnyAsync()) 
        {
            var uni = await context.Universities.FirstAsync();
            await context.Departments.AddAsync(new Department { Name = "General", UniversityId = uni.Id });
            await context.SaveChangesAsync();
        }

        if (!await context.Locations.AnyAsync()) 
        {
            var dept = await context.Departments.FirstAsync();
            await context.Locations.AddRangeAsync(
                new Location { Name = "Main Library", LocationType = Core.Enums.enLocationType.Library, DepartmentId = dept.Id },
                new Location { Name = "Student Union", LocationType = Core.Enums.enLocationType.Other, DepartmentId = dept.Id },
                new Location { Name = "Engineering Building", LocationType = Core.Enums.enLocationType.Classroom, DepartmentId = dept.Id }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.ItemReports.AnyAsync()) 
        {
            var user = await context.Users.FirstAsync();
            var category = await context.Categories.FirstAsync(c => c.Name == "Electronics");
            var location = await context.Locations.FirstAsync(l => l.Name == "Main Library");

            await context.ItemReports.AddRangeAsync(
                new ItemReport 
                { 
                    ItemName = "Lost MacBook Pro", 
                    Description = "Silver MacBook Pro 14-inch left on the second floor.",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    LocationId = location.Id,
                    DateReported = DateTime.UtcNow.AddDays(-2),
                    StatusType = Core.Enums.enStatusType.Open,
                    ReportType = Core.Enums.enReportType.Lost
                },
                new ItemReport 
                { 
                    ItemName = "Found iPhone 13", 
                    Description = "Black iPhone with a red case found near the entrance.",
                    UserId = user.Id,
                    CategoryId = category.Id,
                    LocationId = location.Id,
                    DateReported = DateTime.UtcNow.AddDays(-1),
                    StatusType = Core.Enums.enStatusType.Open,
                    ReportType = Core.Enums.enReportType.Found
                }
            );
            await context.SaveChangesAsync();
        }
    }
}
