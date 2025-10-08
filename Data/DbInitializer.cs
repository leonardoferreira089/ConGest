using ConGest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConGest.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Créer la base de données si elle n'existe pas
            context.Database.EnsureCreated();

            // Créer les rôles s'ils n'existent pas
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Collaborateur"))
            {
                await roleManager.CreateAsync(new IdentityRole("Collaborateur"));
            }

            // Créer les utilisateurs par défaut
            var adminUser = await userManager.FindByEmailAsync("admin@congest.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@congest.com",
                    Email = "admin@congest.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var collaborateurUser = await userManager.FindByEmailAsync("collaborateur@congest.com");
            if (collaborateurUser == null)
            {
                collaborateurUser = new ApplicationUser
                {
                    UserName = "collaborateur@congest.com",
                    Email = "collaborateur@congest.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(collaborateurUser, "Collaborateur@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(collaborateurUser, "Collaborateur");
                }
            }

            // Créer des collaborateurs par défaut si nécessaire
            if (!context.Collaborateurs.Any())
            {
                // Liste de couleurs uniques
                var couleursDisponibles = new List<string> { "#FF5733", "#33FF57", "#3357FF", "#F333FF", "#FF33A1", "#33FFF6" };

                // Créer un collaborateur pour l'utilisateur admin
                var adminCollaborateur = new Collaborateur
                {
                    Nom = "Administrateur",
                    Couleur = couleursDisponibles[0], // Utiliser la première couleur
                    Fonction = "Administrateur",
                    ApplicationUserId = adminUser.Id
                };
                context.Collaborateurs.Add(adminCollaborateur);

                // Créer un collaborateur pour l'utilisateur collaborateur
                var collaborateur1 = new Collaborateur
                {
                    Nom = "Collaborateur Test",
                    Couleur = couleursDisponibles[1], // Utiliser la deuxième couleur
                    Fonction = "Préparateur de commandes",
                    ApplicationUserId = collaborateurUser.Id
                };
                context.Collaborateurs.Add(collaborateur1);

                // Autres collaborateurs sans utilisateur associé
                var collaborateur2 = new Collaborateur
                {
                    Nom = "Marie Martin",
                    Couleur = couleursDisponibles[2], // Utiliser la troisième couleur
                    Fonction = "Cariste"
                };
                context.Collaborateurs.Add(collaborateur2);

                await context.SaveChangesAsync();
            }
            else
            {
                // Si les collaborateurs existent déjà, vérifier si l'utilisateur collaborateur est associé à un collaborateur
                var collaborateur = await context.Collaborateurs.FirstOrDefaultAsync(c => c.ApplicationUserId == collaborateurUser.Id);
                if (collaborateur == null)
                {
                    // Trouver une couleur non utilisée
                    var couleursUtilisees = context.Collaborateurs.Select(c => c.Couleur).ToList();
                    var couleursDisponibles = new List<string> { "#FF5733", "#33FF57", "#3357FF", "#F333FF", "#FF33A1", "#33FFF6" };
                    var nouvelleCouleur = couleursDisponibles.FirstOrDefault(c => !couleursUtilisees.Contains(c)) ?? "#" + Guid.NewGuid().ToString("N").Substring(0, 6);

                    // Si non, créer un collaborateur pour cet utilisateur
                    collaborateur = new Collaborateur
                    {
                        Nom = "Collaborateur Test",
                        Couleur = nouvelleCouleur,
                        Fonction = "Préparateur de commandes",
                        ApplicationUserId = collaborateurUser.Id
                    };
                    context.Collaborateurs.Add(collaborateur);
                    await context.SaveChangesAsync();
                }
            }

            // Créer des jours bloqués par défaut si nécessaire
            if (!context.JoursBloques.Any())
            {
                var jourBloque1 = new JourBloque
                {
                    DateBloquee = new DateTime(2023, 12, 25),
                    Raison = "Jour férié: Noël",
                    UtilisateurBloqueur = adminUser.Email
                };
                context.JoursBloques.Add(jourBloque1);

                var jourBloque2 = new JourBloque
                {
                    DateBloquee = new DateTime(2023, 1, 1),
                    Raison = "Jour férié: Jour de l'an",
                    UtilisateurBloqueur = adminUser.Email
                };
                context.JoursBloques.Add(jourBloque2);

                await context.SaveChangesAsync();
            }
        }
    }
}