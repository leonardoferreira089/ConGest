using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConGest.Models
{
    public class Collaborateur
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required]
        [StringLength(7)] // Format hexadécimal #FFFFFF
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "La couleur doit être au format hexadécimal #FFFFFF")]
        public string Couleur { get; set; }

        [Required]
        [StringLength(50)]
        public string Fonction { get; set; }

        public bool EstActif { get; set; } = true;

        // Relation avec ApplicationUser
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        // Relation avec DemandeConge
        public ICollection<DemandeConge> DemandesConge { get; set; } = new List<DemandeConge>();
    }
}