using bbt.gateway.messaging.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging
{
    public class DatabaseContext : DbContext
    {
        public DbSet<PhoneConfiguration> PhoneConfigurations { get; set; }
        public DbSet<Header> Headers { get; set; }
        public DbSet<OtpBlackListEntry> OtpBlackListEntries { get; set; }
        public DbSet<OtpOperatorException> OtpOperatorExceptions { get; set; }

        
        public string  DbPath { get; set; }


        public DatabaseContext()
        {
            DbPath = $"messaging.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<PhoneConfiguration>().OwnsOne(i => i.Phone);
            builder.Entity<PhoneConfigurationLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<SendOtpRequestLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<SendOtpRequestLog>().OwnsOne(i => i.Phone);
            builder.Entity<SendSmsLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<OtpBlackListEntry>().OwnsOne(i => i.CreatedBy);
            builder.Entity<OtpBlackListEntry>().OwnsOne(i => i.ResolvedBy);
            builder.Entity<OtpBlackListEntry>().OwnsOne(i => i.Phone);
            builder.Entity<OtpBlackListEntryLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<OtpOperatorException>().OwnsOne(i => i.CreatedBy);

            

            //builder.Entity<PhoneConfiguration>().HasKey(c => c.Id).ForSqlServerIsClustered(false);
            //builder.Entity<PhoneConfiguration>().HasIndex("Id").HasName("ClusteredId").ForSqlServerIsClustered(true);


        }
    }

}
