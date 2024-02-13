using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homies.Data.Models
{
    /// <summary>
    /// mapping entity that maps events and users
    /// </summary>
    [Comment("mapping entity that maps events and users")]
    public class EventParticipant
    {
        /// <summary>
        /// Helper identifier
        /// </summary>
        [Required]
        [ForeignKey(nameof(Helper))]
        [Comment("Helper identifier")]
        public string HelperId { get; set; } = string.Empty;

        /// <summary>
        /// User
        /// </summary>
        [Required]
        public IdentityUser Helper { get; set; } = null!;

        /// <summary>
        /// Event identifier
        /// </summary>
        [Required]
        [ForeignKey(nameof(Event))]
        [Comment("Event identifier")]
        public int EventId { get; set; }

        /// <summary>
        /// Event
        /// </summary>
        [Required]
        public Event Event { get; set; } = null!;
    }
}
