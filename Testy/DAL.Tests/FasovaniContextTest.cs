﻿using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Xunit;
using DAL.Tests.Extensions;

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
        public void Update_SkladDbSet_MnozstviByFind()
        {
            var logger = new LoggerDb();

            using (var context = new FasovaniContext(logger))
            {
                var data = context.SkladTable.Where(p => p.M_MNOZSTVI > 10).Take(5).ToArray();
                var tmp = data.First();

                var one = context.SkladTable.Find(tmp.M_SKLAD, tmp.M_CISLOZAK, tmp.M_CISLOM);
                one.M_MNOZSTVI += 1;

                //logger.StartNewRecord();
                //var two = context.SkladTable.First(p =>
                //    p.M_SKLAD == tmp.M_SKLAD && p.M_CISLOZAK == tmp.M_CISLOZAK && p.M_CISLOM == tmp.M_CISLOM);

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
                var data = context.SkladTable.Where(p => p.M_MNOZSTVI > 10).Take(5).ToArray();
                var tmp = data.First();

                logger.StartNewRecord();
                var one = context.SkladTable.Single(p => p.M_SKLAD == tmp.M_SKLAD && p.M_CISLOZAK == tmp.M_CISLOZAK && p.M_CISLOM == tmp.M_CISLOM);
                one.M_MNOZSTVI += 1;

                logger.StartNewRecord();
                var two = context.SkladTable.Single(p => p.M_SKLAD == tmp.M_SKLAD && p.M_CISLOZAK == tmp.M_CISLOZAK && p.M_CISLOM == tmp.M_CISLOM);
                two.M_MNOZSTVI += 2;

                logger.StartNewRecord();
                context.SaveChanges();

                var sql = logger.Current.ToString();

                Assert.Contains("M_MNOZSTVI", sql);
                
                
            }
        }
    }
}