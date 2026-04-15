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
        if (await userManager.FindByEmailAsync(userEmail) == null)
        {
            var user = new User
            {
                UserName = userEmail,
                Email = userEmail,
                Name = "Regular User",
                IsActive = true,
                EmailConfirmed = true,
                Created = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
}