using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("refresh_tokens")]
    public class RefreshTokenModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        // SHA-256 hex digest of the raw token. The raw token is only returned
        // once to the client; only the hash is persisted, so a DB leak does
        // not let an attacker present valid refresh tokens.
        [Required]
        [MaxLength(64)]
        [Column("token_hash")]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
