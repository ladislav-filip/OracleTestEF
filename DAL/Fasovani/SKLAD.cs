using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Fasovani
{
    [Table("M_SKLAD")]
    public class SKLAD
    {
        [Key, Column(Order = 1)]
        public string M_SKLAD { get; set; }

        [Key, Column(Order = 2)]
        public string M_CISLOZAK { get; set; }

        [Key, Column(Order = 3)]
        public string M_CISLOM { get; set; }

        public double? M_MNOZSTVI { get; set; }

        public double? M_CENAJ { get; set; }
    }
}