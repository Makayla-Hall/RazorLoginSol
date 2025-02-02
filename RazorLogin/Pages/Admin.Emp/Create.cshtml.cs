﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RazorLogin.Models;

namespace RazorLogin.Pages.Admin.Emp
{
    public class CreateModel : PageModel
    {
        private readonly RazorLogin.Models.ZooDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateModel(RazorLogin.Models.ZooDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }



        public IActionResult OnGet()
        {
            ViewData["FoodStoreId"] = new SelectList(_context.FoodStores, "FoodStoreId", "FoodStoreId");

            List<SelectListItem> selectList = _context.FoodStores
                .AsNoTracking()
                .Select(x => new SelectListItem()
                {
                    Value = x.FoodStoreId.ToString(),

                })
                .ToList();

            selectList.Insert(0, new SelectListItem()
            {
                Value = "",
                Text = "--- Select Related Entity ---"
            });

            ViewData["RelatedEntity_Id"] = selectList;

            ViewData["ShopId"] = new SelectList(_context.GiftShops, "ShopId", "ShopId");
            ViewData["SupervisorId"] = new SelectList(_context.Managers, "ManagerId", "ManagerId");
            return Page();
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Role { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Employees.Add(Employee);
            await _context.SaveChangesAsync();

            var user = new IdentityUser
            {
                UserName = Employee.EmployeeEmail,
                Email = Employee.EmployeeEmail
            };

            var result = await _userManager.CreateAsync(user, Password);

            if (!result.Succeeded)
            {
                // Handle errors (e.g., display error messages)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            if (!string.IsNullOrEmpty(Role))
            {
                await _userManager.AddToRoleAsync(user, Role);

                // Generate a random numeric suffix
                var randomSuffix = new Random().Next(10000, 99999); // Random number between 10000 and 99999

                if (Role == "Manager")
                {
                    // Create a new Manager entry
                    var manager = new Manager
                    {
                        EmployeeId = Employee.EmployeeId,
                        // Concatenate EmployeeId with a random suffix
                        ManagerId = int.Parse($"{Employee.EmployeeId}{randomSuffix}"), // Adjusted to be an int
                        Department = Employee.Department, // Assuming you want to set the department
                        ManagerEmploymentDate = DateOnly.FromDateTime(DateTime.Now) // Set to today's date
                    };
                    _context.Managers.Add(manager);
                    await _context.SaveChangesAsync();
                }
                else if (Role == "Zookeeper")
                {
                    // Create a new Zookeeper entry
                    var zookeeper = new Zookeeper
                    {
                        EmployeeId = Employee.EmployeeId,
                        // Concatenate EmployeeId with a random suffix
                        ZookeeperId = int.Parse($"{Employee.EmployeeId}{randomSuffix}"), // Adjusted to be an int
                        TrainingRenewalDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)) // Set to one year from now
                                                                                              // LastTrainingDate is left empty
                    };
                    _context.Zookeepers.Add(zookeeper);
                    await _context.SaveChangesAsync(); 
                }

                    // Save the new Manager or Zookeeper entry
                    await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
