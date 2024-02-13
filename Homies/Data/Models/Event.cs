using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Homies.Data.Models
{
    /// <summary>
    /// Event database entity
    /// </summary>
    [Comment("Event entity")]
    public class Event
    {
        /// <summary>
        /// Event identifier
        /// </summary>
        [Key]
        [Comment("Event identifier")]
        public int Id { get; set; }

        /// <summary>
        /// Event name
        /// </summary>
        [Required]
        [MaxLength(DataConstants.EventNameMaxLength)]
        [Comment("Event name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Event description
        /// </summary>
        [Required]
        [MaxLength(DataConstants.EventDescriptionMaxLength)]
        [Comment("Event description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Event organiser identifier
        /// </summary>
        [Required]
        [Comment("Event organiser identifier")]
        public int OrganiserId { get; set; }

        /// <summary>
        /// Event organiser
        /// </summary>
        [Required]
        [Comment("Event organiser")]
        public IdentityUser Organiser { get; set; } = null!;

        /// <summary>
        /// Date and time event is created
        /// </summary>
        [Required]
        [Comment("Date and time event is created")]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Date and time event starts
        /// </summary>
        [Required]
        [Comment("Date and time event starts")]
        public DateTime Start { get; set; }

        /// <summary>
        /// Date and time event ends
        /// </summary>
        [Required]
        [Comment("Date and time event ends")]
        public DateTime End { get; set; }

        /// <summary>
        /// Event's type identifier
        /// </summary>
        [Required]
        [Comment("Event's type identifier")]
        public int TypeId { get; set; }

        /// <summary>
        /// Event's type
        /// </summary>
        [Required]
        [Comment("Event's type")]
        public Type Type { get; set; } = null!;

        /// <summary>
        /// EventsParticipants
        /// </summary>
        public ICollection<EventParticipant> EventsParticipants { get; set; } = new List<EventParticipant>();
    }
}
