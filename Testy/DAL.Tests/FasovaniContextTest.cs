using System.Data.Entity;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using Xunit;

namespace DAL.Tests
{
    public class FasovaniContextTest
    {
        [Fact]
        public void Check_FasovaniContext_ConnectSuccess()
        {
            var context = new FasovaniContext();
            context.Database.Connection.Open();
            Assert.NotNull(context);
        }

        [Fact]
        public void Load_SkladDbSet_FirstFive()
        {
            using (var context = new FasovaniContext())
            {
                var data = context.SkladTable.Take(5).ToArray();
                var cnt = data.Length;
                Assert.Equal(5, cnt);
            }
        }

        [Fact]
        public void Test_SkladDbSet_FindNoCache()
        {
            string cisloM;
            string cisloZak;
            string sklad;
            var logger = new LoggerDb();
            using (var context = new FasovaniContext(logger))
            {
                var data = context.SkladTable.First(p => p.M_MNOZSTVI > 10);
                sklad = data.M_SKLAD;
                cisloZak = data.M_CISLOZAK;
                cisloM = data.M_CISLOM;
            }

            using (var context2 = new FasovaniContext(logger))
            {
                logger.StartNewRecord();
                var one = context2.SkladTable.Find(sklad, cisloZak, cisloM);
                Assert.NotNull(one);
            }
        }

        [Fact]
        public void Update_SkladDbSet_MnozstviByFind()
        {
            var logger = new LoggerDb();

            using (var context = new FasovaniContext(logger))
            {
                var data = context.SkladTable.Where(p => p.M_MNOZSTVI > 10).Take(5).ToArray();
                var tmp = data.First();

                var one = context.SkladTable.Find(tmp.M_SKLAD, tmp.M_CISLOZAK, tmp.M_CISLOM);
                one.M_MNOZSTVI += 1;

                logger.StartNewRecord();
                context.SaveChanges();

                var sql = logger.Current.ToString();

                Assert.Contains("M_MNOZSTVI", sql);
            }
        }

        [Fact]
        public void Update_SkladDbSet_MnozstviBySingle()
        {
            var logger = new LoggerDb();

            using (var context = new FasovaniContext(logger))
            {
                using (var trn = context.Database.BeginTransaction())
                {
                    var data = context.SkladTable.Where(p => p.M_MNOZSTVI > 10 && p.M_CENAJ > 0).Take(5).ToArray();
                    var tmp = data.First();

                    logger.StartNewRecord();
                    var one = context.SkladTable.Single(p =>
                        p.M_SKLAD == tmp.M_SKLAD && p.M_CISLOZAK == tmp.M_CISLOZAK && p.M_CISLOM == tmp.M_CISLOM);
                    one.M_MNOZSTVI += 1;

                    logger.StartNewRecord();
                    var two = context.SkladTable.Single(p =>
                        p.M_SKLAD == tmp.M_SKLAD && p.M_CISLOZAK == tmp.M_CISLOZAK && p.M_CISLOM == tmp.M_CISLOM);
                    two.M_MNOZSTVI += 2;

                    logger.StartNewRecord();
                    context.SaveChanges();

                    var sql = logger.Current.ToString();

                    Assert.ContainsCount(1, "M_MNOZSTVI", sql);

                    trn.Rollback();
                }

            }
        }

        /// <summary>
        /// Kontrola na datové typy jake předává .NET Oracle driver
        /// </summary>
        [Fact]
        public void DataType_Double_Check()
        {
            using (var context = new FasovaniContext())
            {
                var conn = context.Database.Connection as OracleConnection;
                var cmd = new OracleCommand("SELECT M_CELKPOC, m_mnozstvi, m_cenaj FROM m_sklad WHERE m_cenaj > 0 AND m_mnozstvi > 10 and ROWNUM < 5", conn);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var value = r.GetValue(0);
                        var typ = value.GetType();
                        Assert.True(typeof(double) == typ);
                        break;
                    }
                    r.Close();
                }
            }

        }
    }
}