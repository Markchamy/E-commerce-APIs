using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    public class BadgesModel : IStoreScoped
    {
        public int Id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        public string body_html { get; set; }
    }
}
