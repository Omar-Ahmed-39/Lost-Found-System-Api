using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Infrastructure.Configurations;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        // 1. Seed Roles
        string[] roleNames = { "SuperAdmin", "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role { Name = roleName });
            }
        }

        // 2. Seed SuperAdmin User
        var superAdminEmail = "superadmin@gmail.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
        if (superAdminUser == null)
        {
            superAdminUser = new User
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                Name = "Super Admin",
                IsActive = true,
                EmailConfirmed = true,
                Created = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }

        // 3. Seed Admin User
        var adminEmail = "admin@gmail.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "System Admin",
                IsActive = true,
                EmailConfirmed = true,
                Created = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 4. Seed University & Departments
        if (!await context.Universities.AnyAsync())
        {
            var university = new University { Name = "Global University", CreatedAt = DateTime.UtcNow };
            await context.Universities.AddAsync(university);
            await context.SaveChangesAsync();

            var departments = new List<Department>
            {
                new() { Name = "Computer Science", UniversityId = university.Id, CreatedAt = DateTime.UtcNow },
                new() { Name = "Engineering", UniversityId = university.Id, CreatedAt = DateTime.UtcNow },
                new() { Name = "Business", UniversityId = university.Id, CreatedAt = DateTime.UtcNow }
            };
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        // 5. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Electronics", CreatedAt = DateTime.UtcNow },
                new() { Name = "Documents", CreatedAt = DateTime.UtcNow },
                new() { Name = "Keys", CreatedAt = DateTime.UtcNow },
                new() { Name = "Wallets & Bags", CreatedAt = DateTime.UtcNow },
                new() { Name = "Books", CreatedAt = DateTime.UtcNow }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 6. Seed Locations
        if (!await context.Locations.AnyAsync())
        {
            var dept = await context.Departments.FirstAsync();
            var locations = new List<Location>
            {
                new() { Name = "Main Gate", LocationType = enLocationType.Other, DepartmentId = dept.Id, CreatedAt = DateTime.UtcNow },
                new() { Name = "Student Center", LocationType = enLocationType.Other, DepartmentId = dept.Id, CreatedAt = DateTime.UtcNow },
                new() { Name = "Library", LocationType = enLocationType.Library, DepartmentId = dept.Id, CreatedAt = DateTime.UtcNow }
            };
            await context.Locations.AddRangeAsync(locations);
            await context.SaveChangesAsync();
        }

        // 7. Seed initial test User
        var userEmail = "user@gmail.com";
        var userAdded = false;
        var appUser = await userManager.FindByEmailAsync(userEmail);
        if (appUser == null)
        {
            appUser = new User
            {
                UserName = userEmail,
                Email = userEmail,
                Name = "Regular User",
                IsActive = true,
                EmailConfirmed = true,
                Created = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(appUser, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, "User");
                userAdded = true;
            }
        }
        _ = userAdded;

        // 8. Seed Item Reports (Lost & Found)
        if (!await context.ItemReports.AnyAsync())
        {
            var user = appUser ?? await context.Users.FirstAsync();
            var location1 = await context.Locations.Skip(0).FirstAsync();
            var location2 = await context.Locations.Skip(1).FirstAsync();
            var category1 = await context.Categories.Skip(0).FirstAsync();
            var category2 = await context.Categories.Skip(1).FirstAsync();

            var reports = new List<ItemReport>
            {
                new() {
                    ReportType = enReportType.Lost,
                    ItemName = "iPhone 13",
                    Color = "Black",
                    ConditionType = enConditionType.Good,
                    StatusType = enStatusType.Open,
                    DateReported = DateTime.UtcNow.AddDays(-2),
                    Description = "Lost near the main gate.",
                    UserId = user.Id,
                    LocationId = location1.Id,
                    CategoryId = category1.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new() {
                    ReportType = enReportType.Found,
                    ItemName = "Leather Wallet",
                    Color = "Brown",
                    ConditionType = enConditionType.New,
                    StatusType = enStatusType.Closed,
                    DateReported = DateTime.UtcNow.AddDays(-1),
                    Description = "Found in the student center.",
                    UserId = user.Id,
                    LocationId = location2.Id,
                    CategoryId = category2.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.ItemReports.AddRangeAsync(reports);
            await context.SaveChangesAsync();
        }

        // 9. Seed Feedbacks
        if (!await context.Feedbacks.AnyAsync())
        {
            var user = appUser ?? await context.Users.FirstAsync();

            var feedbacks = new List<Feedback>
            {
                new() { Subject = "Great App", Message = "Really helped me find my lost keys!", Rating = 5, UserId = user.Id, CreatedAt = DateTime.UtcNow },
                new() { Subject = "Needs improvement", Message = "The UI could be better.", Rating = 3, IsReplied = true, AdminReply = "Thank you! We're working on it.", UserId = user.Id, CreatedAt = DateTime.UtcNow }
            };
            await context.Feedbacks.AddRangeAsync(feedbacks);
            await context.SaveChangesAsync();
        }

        // 10. Seed Matches
        if (!await context.Matches.AnyAsync())
        {
            var lostReport = await context.ItemReports.FirstOrDefaultAsync(r => r.ReportType == enReportType.Lost);
            var foundReport = await context.ItemReports.FirstOrDefaultAsync(r => r.ReportType == enReportType.Found);
            var systemAdmin = await userManager.FindByEmailAsync("admin@gmail.com");

            if (lostReport != null && foundReport != null && systemAdmin != null)
            {
                var match = Match.Create(lostReport.Id, foundReport.Id, 85.5, systemAdmin.Id);
                match.Approve(systemAdmin.Id);
                await context.Matches.AddAsync(match);
                await context.SaveChangesAsync();
            }
        }

        // 11. Seed Claims
        if (!await context.Claims.AnyAsync())
        {
            var user = appUser ?? await context.Users.FirstAsync();
            var foundReport = await context.ItemReports.FirstOrDefaultAsync(r => r.ReportType == enReportType.Found);

            if (foundReport != null)
            {
                var claim = new Claim
                {
                    UserId = user.Id,
                    ReportId = foundReport.Id,
                    ApprovalStatus = enApprovalStatus.Approved,
                    Remarks = "User proved ownership with serial number verification.",
                    ClaimDate = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow
                };
                await context.Claims.AddAsync(claim);
                await context.SaveChangesAsync();
            }
        }

        // 12. Seed Handovers
        if (!await context.Handovers.AnyAsync())
        {
            var systemAdmin = await userManager.FindByEmailAsync("admin@gmail.com");
            var claim = await context.Claims.FirstOrDefaultAsync(c => c.ApprovalStatus == enApprovalStatus.Approved);
            var location = await context.Locations.FirstAsync();

            if (claim != null && systemAdmin != null && location != null)
            {
                var handover = new Handover
                {
                    ClaimId = claim.Id,
                    ReceiverUserId = claim.UserId,
                    HandedByUserId = systemAdmin.Id,
                    LocationId = location.Id,
                    IdType = enIdType.NationalId,
                    IdNumber = "1234567890",
                    HandoverDate = DateTime.UtcNow,
                    Notes = "Item successfully returned to the owner.",
                    ImagePath = "dummy-handover-signature.png",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Handovers.AddAsync(handover);
                await context.SaveChangesAsync();
            }
        }
    }
}