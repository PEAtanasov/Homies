using Homies.Data;
using System.ComponentModel.DataAnnotations;

namespace Homies.Models
{
    public class EventFormModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(DataConstants.EventNameMaxLength, MinimumLength = DataConstants.EventNameMinLength)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(DataConstants.EventDescriptionMaxLength, MinimumLength = DataConstants.EventDescriptionMinLength)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Start { get; set; } = string.Empty;

        [Required]
        public string End { get; set; } = string.Empty;

        [Required]
        public int TypeId { get; set; }

        public string OrganiserId { get; set; } = string.Empty;
        public ICollection<TypeViewModel> Types { get; set; } = new List<TypeViewModel>();
    }
}
