using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConGest.Models
{
    public class DemandeConge
    {
        public int Id { get; set; }

        [Required]
        public int CollaborateurId { get; set; }
        public Collaborateur Collaborateur { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateDebut { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateFin { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [NotMapped]
        public int NombreDeJours => (DateFin - DateDebut).Days + 1;
    }
}