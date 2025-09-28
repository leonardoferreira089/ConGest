using ConGest.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConGest.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Collaborateur> Collaborateurs { get; set; }
        public DbSet<DemandeConge> DemandesConge { get; set; }
        public DbSet<JourBloque> JoursBloques { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration pour garantir l'unicité de la couleur par collaborateur
            modelBuilder.Entity<Collaborateur>()
                .HasIndex(c => c.Couleur)
                .IsUnique();

            // Configuration pour la relation entre DemandeConge et Collaborateur
            modelBuilder.Entity<DemandeConge>()
                .HasOne(d => d.Collaborateur)
                .WithMany(c => c.DemandesConge)
                .HasForeignKey(d => d.CollaborateurId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration pour garantir qu'une date bloquée est unique
            modelBuilder.Entity<JourBloque>()
                .HasIndex(j => j.DateBloquee)
                .IsUnique();

            // Configuration de la relation entre Collaborateur et ApplicationUser
            modelBuilder.Entity<Collaborateur>()
                .HasOne(c => c.ApplicationUser)
                .WithOne()
                .HasForeignKey<Collaborateur>(c => c.ApplicationUserId);

            // Contrainte pour s'assurer que DateFin >= DateDebut
            modelBuilder.Entity<DemandeConge>()
                .HasCheckConstraint("CK_DemandeConge_DateFin", "DateFin >= DateDebut");
        }
    }
}