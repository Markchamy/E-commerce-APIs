using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    public class EmployeeModel : IStoreScoped
    {
        public int Id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        [Column("access_control")]
        public List<string> AccessControl { get; set; }
        public long UserId { get; set; }
        public UserModel User { get; set; }
    }


}
