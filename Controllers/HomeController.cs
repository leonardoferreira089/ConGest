using ConGest.Data;
using ConGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConGest.Controllers
{
    [Authorize] // Autorise tous les utilisateurs connectés
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Récupérer toutes les demandes de congé avec les collaborateurs associés
            var demandesConge = await _context.DemandesConge
                .Include(dc => dc.Collaborateur)
                .ToListAsync();

            // Récupérer tous les jours bloqués
            var joursBloques = await _context.JoursBloques.ToListAsync();

            // Récupérer le collaborateur de l'utilisateur connecté
            Collaborateur currentCollaborateur = null;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                currentCollaborateur = await _context.Collaborateurs
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            }

            // Récupérer tous les collaborateurs actifs
            var allCollaborateurs = await _context.Collaborateurs
                .Where(c => c.EstActif)
                .ToListAsync();

            // Créer un modèle pour la vue
            var model = new CalendarViewModel
            {
                DemandesConge = demandesConge,
                JoursBloques = joursBloques,
                CurrentCollaborateur = currentCollaborateur,
                AllCollaborateurs = allCollaborateurs
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class CalendarViewModel
    {
        public List<DemandeConge> DemandesConge { get; set; } = new List<DemandeConge>();
        public List<JourBloque> JoursBloques { get; set; } = new List<JourBloque>();
        public Collaborateur CurrentCollaborateur { get; set; }
        public List<Collaborateur> AllCollaborateurs { get; set; } = new List<Collaborateur>();
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}