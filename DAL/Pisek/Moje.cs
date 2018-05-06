using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Pisek
{
    [Table("Moje")]
    public class Moje
    {
        [Key]
        public int MojeId { get; set; }

        public string MujTyp { get; set; }
    }
}