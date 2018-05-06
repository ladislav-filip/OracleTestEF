using System.Data;
using System.Data.Entity;
using System.Diagnostics;
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

                // SetCenaAndCommit(tmp.M_SKLAD, tmp.M_CISLOZAK, tmp.M_CISLOM, -10);

                var one = context.SkladTable.Find(tmp.M_SKLAD, tmp.M_CISLOZAK, tmp.M_CISLOM);
                one.M_MNOZSTVI += 1;

                logger.StartNewRecord();
                context.SaveChanges();

                var sql = logger.Current.ToString();

                Assert.Contains("M_MNOZSTVI", sql);
            }
        }

        /// <summary>
        /// Test na vícenásobnou zmenu entity ale pouze jeden update
        /// </summary>
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
                    var one = context.SkladTable.Single(p => p.M_SKLAD == tmp.M_SKLAD && p.M_CISLOZAK == tmp.M_CISLOZAK && p.M_CISLOM == tmp.M_CISLOM);
                    one.M_MNOZSTVI += 1;

                    // v jiném kontextu nastavím na této entitě cenu, ale v tomto kontextu se to nijak neprojeví!!
                    SetCenaAndCommit(tmp.M_SKLAD, tmp.M_CISLOZAK, tmp.M_CISLOM, -14);

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
        /// Ukázka cache v EF, přestože je proveden SQL dotaz, tak záznamy již jednou načtených entit nejsou aktualizovány podle posledního stavu
        /// </summary>
        [Fact]
        public void UpdateExact_SkladDbSet_MnozstviBySingle()
        {
            string cisloM = "01041";
            string cisloZak = "1";
            string sklad = "10";

            var logger = new LoggerDb();

            using (var context = new FasovaniContext(logger))
            {
                var cenaTest = -5;
                // zde aktualizuji tento záznam v SQL Developeru nebo v jiném contextu
                // update m_sklad set M_CENAJ = -5 where m_sklad = '10' and M_CISLOZAK = '1' and M_CISLOM = '01041'
                SetCenaAndCommit(sklad, cisloZak, cisloM, cenaTest);
                logger.StartNewRecord();
                var one = context.SkladTable.Single(p => p.M_SKLAD == sklad && p.M_CISLOZAK == cisloZak && p.M_CISLOM == cisloM);
                Debug.WriteLine(one.M_CENAJ);

                // zde aktualizuji tento záznam v SQL Developeru nebo v jiném contextu
                // update m_sklad set M_CENAJ = -6 where m_sklad = '10' and M_CISLOZAK = '1' and M_CISLOM = '01041'
                SetCenaAndCommit(sklad, cisloZak, cisloM, -6);
                logger.StartNewRecord();
                var two = context.SkladTable.Single(p => p.M_SKLAD == sklad && p.M_CISLOZAK == cisloZak && p.M_CISLOM == cisloM);
                Debug.WriteLine(two.M_CENAJ);

                // přesto, že došlo v jiném kontextu k aktualizaci záznamu a pak zde k požadavku na jeho nové načtění, tak zde bude stále původní
                // první načtená hodnota
                Assert.Equal(cenaTest, two.M_CENAJ);                
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
                        // kontroluji na double
                        Assert.True(typeof(double) == typ);
                        break;
                    }
                    r.Close();
                }
            }

        }

        private void SetCenaAndCommit(string sklad, string cisloZak, string cisloM, double cena)
        {
            using (var context = new FasovaniContext())
            {
                var ent = context.SkladTable.Find(sklad, cisloZak, cisloM);
                ent.M_CENAJ = cena;
                context.SaveChanges();
            }
        }
    }
}