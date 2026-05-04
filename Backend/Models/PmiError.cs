using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("pmierrors")]
    public class PmiError
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("error")]
        public string? Error { get; set; }

        // Navigation property
        public virtual ICollection<PmiOrder>? Orders { get; set; }
    }
}
