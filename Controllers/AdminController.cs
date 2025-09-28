using ConGest.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConGest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Collaborateurs()
        {
            var collaborateurs = await _context.Collaborateurs.ToListAsync();
            return View(collaborateurs);
        }

        public async Task<IActionResult> DemandesConge()
        {
            var demandes = await _context.DemandesConge
                .Include(dc => dc.Collaborateur)
                .ToListAsync();
            return View(demandes);
        }

        public async Task<IActionResult> JoursBloques()
        {
            var jours = await _context.JoursBloques.ToListAsync();
            return View(jours);
        }
    }
}