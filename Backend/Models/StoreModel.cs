using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class StoreModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(63)]
        [Column("slug")]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("domain")]
        public string? Domain { get; set; }

        [MaxLength(255)]
        [Column("logo_url")]
        public string? LogoUrl { get; set; }

        [MaxLength(7)]
        [Column("primary_color")]
        public string? PrimaryColor { get; set; }

        [MaxLength(3)]
        public string? Currency { get; set; }

        [MaxLength(5)]
        public string? Locale { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
