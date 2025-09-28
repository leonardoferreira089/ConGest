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
            Collaborateur collaborateur = null;
            if (user != null)
            {
                collaborateur = await _context.Collaborateurs.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            }

            // Si l'utilisateur est un admin, il peut choisir n'importe quel collaborateur
            if (User.IsInRole("Admin"))
            {
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Collaborateurs, "Id", "Nom");
            }
            else if (collaborateur != null)
            {
                // Si l'utilisateur est un collaborateur, pré-remplir son ID
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    new List<Collaborateur> { collaborateur }, "Id", "Nom", collaborateur.Id);
            }
            else
            {
                // Si l'utilisateur n'est associé à aucun collaborateur, retourner une erreur
                return Problem("Vous n'êtes pas associé à un collaborateur.");
            }

            return View();
        }

        // POST: DemandesConge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CollaborateurId,DateDebut,DateFin")] DemandeConge demandeConge)
        {
            // Si l'utilisateur n'est pas admin, vérifier qu'il ne modifie pas le CollaborateurId
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                var collaborateur = await _context.Collaborateurs.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);

                if (collaborateur == null)
                {
                    return Problem("Vous n'êtes pas associé à un collaborateur.");
                }

                // Forcer le CollaborateurId à celui de l'utilisateur connecté
                demandeConge.CollaborateurId = collaborateur.Id;
            }

            if (ModelState.IsValid)
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
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return await PrepareCreateView(demandeConge);
        }

        private async Task<IActionResult> PrepareCreateView(DemandeConge demandeConge)
        {
            var user = await _userManager.GetUserAsync(User);
            Collaborateur collaborateur = null;
            if (user != null)
            {
                collaborateur = await _context.Collaborateurs.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Collaborateurs, "Id", "Nom", demandeConge.CollaborateurId);
            }
            else if (collaborateur != null)
            {
                ViewData["CollaborateurId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    new List<Collaborateur> { collaborateur }, "Id", "Nom", collaborateur.Id);
            }

            return View(demandeConge);
        }

        // Les autres méthodes du contrôleur restent inchangées...
    }
}