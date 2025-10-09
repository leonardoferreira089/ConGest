using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConGest.Models
{
    public class JourBloque
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateBloquee { get; set; }

        [StringLength(200)]
        public string Raison { get; set; }
        [Required]
        public string UtilisateurBloqueur { get; set; } = "Système";
    }
}