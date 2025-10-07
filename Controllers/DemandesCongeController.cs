using ConGest.Data;
using ConGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConGest.Controllers
{
    [Authorize] // Autorise tous les utilisateurs connectés
    public class DemandesCongeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DemandesCongeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: DemandesConge
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DemandesConge.Include(d => d.Collaborateur);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DemandesConge/Create
        public async Task<IActionResult> Create()
        {
            // Récupérer l'utilisateur connecté
            var user = await _userManager.GetUserAsync(User);

            // Récupérer le collaborateur associé à l'utilisateur
            Collaborateur currentCollaborateur = null;
            if (user != null)
            {
                currentCollaborateur = await _context.Collaborateurs.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            }

            // Récupérer tous les collaborateurs actifs
            var allCollaborateurs = await _context.Collaborateurs
                .Where(c => c.EstActif)
                .ToListAsync();

            // Préparer la liste des collaborateurs pour la vue
            // Si l'utilisateur est un collaborateur, pré-sélectionner son nom
            if (currentCollaborateur != null)
            {
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    allCollaborateurs, "Id", "Nom", currentCollaborateur.Id);
            }
            else
            {
                // Sinon, afficher tous les collaborateurs sans pré-sélection
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    allCollaborateurs, "Id", "Nom");
            }

            return View();
        }

        // POST: DemandesConge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CollaborateurId,DateDebut,DateFin")] DemandeConge demandeConge)
        {
            // Journalisation pour le débogage
            System.Diagnostics.Debug.WriteLine($"CollaborateurId: {demandeConge.CollaborateurId}");
            System.Diagnostics.Debug.WriteLine($"DateDebut: {demandeConge.DateDebut}");
            System.Diagnostics.Debug.WriteLine($"DateFin: {demandeConge.DateFin}");

            try
            {
                // Vérifier si les dates sont valides
                if (demandeConge.DateFin < demandeConge.DateDebut)
                {
                    ModelState.AddModelError("", "La date de fin doit être postérieure à la date de début.");
                    return await PrepareCreateView(demandeConge);
                }

                // Vérifier si les dates ne sont pas déjà prises par d'autres congés
                var existingConges = await _context.DemandesConge
                    .Where(dc => dc.CollaborateurId != demandeConge.CollaborateurId &&
                                ((dc.DateDebut >= demandeConge.DateDebut && dc.DateDebut <= demandeConge.DateFin) ||
                                 (dc.DateFin >= demandeConge.DateDebut && dc.DateFin <= demandeConge.DateFin) ||
                                 (dc.DateDebut <= demandeConge.DateDebut && dc.DateFin >= demandeConge.DateFin)))
                    .ToListAsync();

                if (existingConges.Any())
                {
                    ModelState.AddModelError("", "Date indisponible - Congé déjà enregistré par un collaborateur");
                    return await PrepareCreateView(demandeConge);
                }

                // Vérifier si les dates ne sont pas bloquées
                var joursBloques = await _context.JoursBloques
                    .Where(jb => jb.DateBloquee >= demandeConge.DateDebut && jb.DateBloquee <= demandeConge.DateFin)
                    .ToListAsync();

                if (joursBloques.Any())
                {
                    ModelState.AddModelError("", "Date bloquée par l'administrateur - Manque de personnel prévu");
                    return await PrepareCreateView(demandeConge);
                }

                // Ajouter la demande de congé
                demandeConge.DateCreation = DateTime.Now;
                _context.Add(demandeConge);
                var result = await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Résultat de SaveChanges: {result}");

                // Rediriger vers la page d'accueil
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Journaliser l'exception
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                ModelState.AddModelError("", "Une erreur est survenue: " + ex.Message);
                return await PrepareCreateView(demandeConge);
            }
        }

        private async Task<IActionResult> PrepareCreateView(DemandeConge demandeConge)
        {
            // Récupérer l'utilisateur connecté
            var user = await _userManager.GetUserAsync(User);

            // Récupérer le collaborateur associé à l'utilisateur
            Collaborateur currentCollaborateur = null;
            if (user != null)
            {
                currentCollaborateur = await _context.Collaborateurs.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            }

            // Récupérer tous les collaborateurs actifs
            var allCollaborateurs = await _context.Collaborateurs
                .Where(c => c.EstActif)
                .ToListAsync();

            // Préparer la liste des collaborateurs pour la vue
            // Si l'utilisateur est un collaborateur, pré-sélectionner son nom
            if (currentCollaborateur != null)
            {
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    allCollaborateurs, "Id", "Nom", currentCollaborateur.Id);
            }
            else
            {
                // Sinon, afficher tous les collaborateurs sans pré-sélection
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    allCollaborateurs, "Id", "Nom", demandeConge.CollaborateurId);
            }

            return View(demandeConge);
        }


        // GET: DemandesConge/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeConge = await _context.DemandesConge
                .Include(d => d.Collaborateur)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (demandeConge == null)
            {
                return NotFound();
            }

            return View(demandeConge);
        }

        // GET: DemandesConge/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeConge = await _context.DemandesConge.FindAsync(id);
            if (demandeConge == null)
            {
                return NotFound();
            }

            // Récupérer tous les collaborateurs actifs pour la liste déroulante
            var allCollaborateurs = await _context.Collaborateurs
                .Where(c => c.EstActif)
                .ToListAsync();

            ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                allCollaborateurs, "Id", "Nom", demandeConge.CollaborateurId);

            return View(demandeConge);
        }

        // POST: DemandesConge/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CollaborateurId,DateDebut,DateFin,DateCreation")] DemandeConge demandeConge)
        {
            if (id != demandeConge.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(demandeConge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DemandeCongeExists(demandeConge.Id))
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

            // En cas d'erreur, préparer à nouveau la liste des collaborateurs
            var allCollaborateurs = await _context.Collaborateurs
                .Where(c => c.EstActif)
                .ToListAsync();

            ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                allCollaborateurs, "Id", "Nom", demandeConge.CollaborateurId);

            return View(demandeConge);
        }

        // GET: DemandesConge/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeConge = await _context.DemandesConge
                .Include(d => d.Collaborateur)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (demandeConge == null)
            {
                return NotFound();
            }

            return View(demandeConge);
        }

        // POST: DemandesConge/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var demandeConge = await _context.DemandesConge.FindAsync(id);
            if (demandeConge != null)
            {
                _context.DemandesConge.Remove(demandeConge);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DemandeCongeExists(int id)
        {
            return _context.DemandesConge.Any(e => e.Id == id);
        }


    }
}