using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace DAL.Tests
{
    public class PisekContextTest 
    {
        [Fact]
        public void Check_PisekContext_ConnectSuccess()
        {
            var context = new PisekContext();
            context.Database.Connection.Open();
            Assert.NotNull(context);
        }

        [Fact]
        public void Load_SkladDbSet_FirstTwo()
        {
            using (var context = new PisekContext())
            {
                var data = context.MojeTable.Take(2).ToArray();
                var cnt = data.Length;
                Assert.Equal(2, cnt);
            }
        }

        [Fact]
        public void UpdateExact_MojebSet_TypBySingle()
        {
            int mojeId = 1;
            var logger = new LoggerDb();
            using (var context = new PisekContext(logger))
            {
                // zde aktualizuji tento záznam v ManagementStudiu
                // update moje set mujtyp = 'XXX' where MojeId = 1
                logger.StartNewRecord();
                var one = context.MojeTable.Single(p => p.MojeId == mojeId);
                Debug.WriteLine(one.MujTyp);

                // zde aktualizuji tento záznam v ManagementStudiu
                // update moje set mujtyp = 'XX0' where MojeId = 1
                logger.StartNewRecord();
                var two = context.MojeTable.Single(p => p.MojeId == mojeId);
                // a zde bych očekával, že entita bude mít nastavenou hodnotu XX0
                Debug.WriteLine(two.MujTyp);
                 
                var three = context.MojeTable.Where(p => p.MojeId == 1).AsNoTracking().First();
                Debug.WriteLine(three.MujTyp);

                two.MujTyp = "NNN";

                var data = context.MojeTable.FirstOrDefault(p => p.MujTyp == "NNN");
                if (data != null) Debug.WriteLine(data.MujTyp);

                logger.StartNewRecord();
                context.SaveChanges();
                var sql = logger.Current.ToString();
                Assert.ContainsCount(1, "MujTyp", sql);
            }
        }
    }
}