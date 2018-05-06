using System.Data.Entity;
using System.Diagnostics;
using DAL.Pisek;

namespace DAL
{
    public class PisekContext : DbContext
    {
        public DbSet<Moje> MojeTable { get; set; }

        public PisekContext(ILoggerDb logger = null) : base("Pisek")
        {
            if (logger == null)
            {
                Database.Log = s => Debug.WriteLine(s);
            }
            else
            {
                Database.Log = logger.WriteLine;
            }

            // potlaceni code first a modifikaci DB
            Database.SetInitializer<PisekContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");
        }
    }
}