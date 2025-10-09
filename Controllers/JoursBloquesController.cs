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

        // GET: Admin/JoursBloques
        public async Task<IActionResult> Index()
        {
            return View("~/Views/Admin/JoursBloques.cshtml", await _context.JoursBloques.ToListAsync());
        }

        // GET: JoursBloques/Create
        public IActionResult Create()
        {
            var jourBloque = new JourBloque
            {
                DateBloquee = DateTime.Today 
            };
            return View("~/Views/JoursBloques/Create.cshtml", jourBloque);
        }

        // POST: JoursBloques/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DateBloquee,Raison")] JourBloque jourBloque)
        {
            try
            {
                // Remplir automatiquement l'utilisateur sans validation complexe
                var user = await _userManager.GetUserAsync(User);
                jourBloque.UtilisateurBloqueur = user?.Email ?? "Système";

                // Vérification simple de l'unicité de la date
                if (await _context.JoursBloques.AnyAsync(j => j.DateBloquee == jourBloque.DateBloquee))
                {
                    ModelState.AddModelError("DateBloquee", "Cette date est déjà bloquée.");
                    return View(jourBloque);
                }

                // Ajout direct sans validation complexe
                _context.JoursBloques.Add(jourBloque);
                await _context.SaveChangesAsync();

                // Redirection simple vers la liste
                return RedirectToAction("Index", "JoursBloques");
            }
            catch (Exception ex)
            {
                // En cas d'erreur, afficher un message simple
                ModelState.AddModelError("", "Erreur lors de la création: " + ex.Message);
                return View(jourBloque);
            }
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
            return View("~/Views/JoursBloques/Edit.cshtml", jourBloque);
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
                    bool dateExists = await _context.JoursBloques
                        .AnyAsync(j => j.DateBloquee == jourBloque.DateBloquee && j.Id != id);

                    if (dateExists)
                    {
                        ModelState.AddModelError("DateBloquee", "Cette date est déjà bloquée.");
                        return View("~/Views/JoursBloques/Edit.cshtml", jourBloque);
                    }

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
                return RedirectToAction("JoursBloques", "Admin");
            }
            return View("~/Views/JoursBloques/Edit.cshtml", jourBloque);
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

            return View("~/Views/JoursBloques/Delete.cshtml", jourBloque);
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
            return RedirectToAction("JoursBloques", "Admin");
        }

        private bool JourBloqueExists(int id)
        {
            return _context.JoursBloques.Any(e => e.Id == id);
        }
    }
}