using System.Data.Entity;
using System.Diagnostics;

namespace DAL
{
    public class FasovaniContext : DbContext
    {
        public DbSet<Fasovani.SKLAD> SkladTable { get; set; }

        public FasovaniContext(ILoggerDb logger = null) : base("Fasovani")
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
            Database.SetInitializer<FasovaniContext>(null);
        }        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("UNIFACE");
        }
    }
}