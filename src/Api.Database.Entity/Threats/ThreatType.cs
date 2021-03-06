using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Database.Entity.Threats
{
    public class ThreatType : BaseEntity
    {

        public ThreatType()
        {
            Threats = new List<Threat>();
        }

        [Required]
        [StringLength(50)]
        public string Name
        {
            get;
            set;
        }

        public virtual ICollection<Threat> Threats { get; set; }
    }
}
