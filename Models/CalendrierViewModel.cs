using ConGest.Models;

namespace ConGest.Models
{
    public class CalendrierViewModel
    {
        public int Annee { get; set; }
        public int Mois { get; set; }
        public List<DemandeConge> DemandesConge { get; set; } = new List<DemandeConge>();
        public List<JourBloque> JoursBloques { get; set; } = new List<JourBloque>();
    }
}