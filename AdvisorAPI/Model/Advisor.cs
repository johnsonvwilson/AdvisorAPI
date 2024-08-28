using System.ComponentModel.DataAnnotations;

namespace AdvisorAPI.Model
{
    public class Advisor
    {
        public int Id { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }
        public string SIN { get; set; }
        [StringLength(255, ErrorMessage = "Address cannot be longer than 100 characters.")]
        public string Address { get; set; }
        public string Phone { get; set; }
        public string HealthStatus { get; set; }

    }
}
