using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class EmployeeModel
    {
        public int Id { get; set; }

        [Column("access_control")]
        public List<string> AccessControl { get; set; }
        public long UserId { get; set; }
        public UserModel User { get; set; }
    }


}
