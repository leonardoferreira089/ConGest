using ConGest.Data;
using ConGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConGest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class JoursBloquesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JoursBloquesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: JoursBloques
        public async Task<IActionResult> Index()
        {
            return View(await _context.JoursBloques.ToListAsync());
        }

        // GET: JoursBloques/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: JoursBloques/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateBloquee,Raison")] JourBloque jourBloque)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                jourBloque.UtilisateurBloqueur = user?.Email ?? "Inconnu";
                _context.Add(jourBloque);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(jourBloque);
        }

        // GET: JoursBloques/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jourBloque = await _context.JoursBloques.FindAsync(id);
            if (jourBloque == null)
            {
                return NotFound();
            }
            return View(jourBloque);
        }

        // POST: JoursBloques/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateBloquee,Raison,UtilisateurBloqueur")] JourBloque jourBloque)
        {
            if (id != jourBloque.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jourBloque);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JourBloqueExists(jourBloque.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(jourBloque);
        }

        // GET: JoursBloques/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jourBloque = await _context.JoursBloques
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jourBloque == null)
            {
                return NotFound();
            }

            return View(jourBloque);
        }

        // POST: JoursBloques/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jourBloque = await _context.JoursBloques.FindAsync(id);
            if (jourBloque != null)
            {
                _context.JoursBloques.Remove(jourBloque);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JourBloqueExists(int id)
        {
            return _context.JoursBloques.Any(e => e.Id == id);
        }
    }
}