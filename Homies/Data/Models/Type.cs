using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Homies.Data.Models
{
    /// <summary>
    /// Type database entity
    /// </summary>
    [Comment("Type database entity")]
    public class Type
    {
        /// <summary>
        /// Type identifier
        /// </summary>
        [Key]
        [Comment("Type identifier")]
        public int Id { get; set; }

        /// <summary>
        /// Type name
        /// </summary>
        [Required]
        [MaxLength(DataConstants.TypeNameMaxLength)]
        [Comment("Type name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// All events
        /// </summary>
        [Comment("All events")]
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
