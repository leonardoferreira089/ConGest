using ConGest.Models;

namespace ConGest.Models
{
    public class AdministrationViewModel
    {
        public List<DemandeConge> DemandesEnAttente { get; set; } = new List<DemandeConge>();
        public List<Collaborateur> Collaborateurs { get; set; } = new List<Collaborateur>();
        public List<JourBloque> JoursBloques { get; set; } = new List<JourBloque>();
    }
}