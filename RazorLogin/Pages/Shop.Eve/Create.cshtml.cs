﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RazorLogin.Data;
using RazorLogin.Models;

namespace RazorLogin.Pages.Shop.Eve
{
    public class CreateModel : PageModel
    {
        private readonly ZooDbContext _context;

        public CreateModel(ZooDbContext context)
        {
            _context = context;
        }


        [BindProperty]
        public Event Event { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Events.Add(Event);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
    
