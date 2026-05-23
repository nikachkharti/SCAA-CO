using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCAA_API.Entities
{
    public class Supplier
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string SupplierName { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
