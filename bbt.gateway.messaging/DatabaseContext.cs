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
        public DbSet<Operator> Operators { get; set; }
        public DbSet<BlackListEntry> BlackListEntries { get; set; }
        public DbSet<OtpRequestLog> OtpRequestLogs { get; set; }
        public DbSet<SmsLog> SmsLogs { get; set; }
        


        public string DbPath { get; set; }


        public DatabaseContext()
        {
            DbPath = $"messaging.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}; Foreign Keys = False");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PhoneConfiguration>().OwnsOne(i => i.Phone);
            builder.Entity<PhoneConfigurationLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<OtpRequestLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<OtpRequestLog>().OwnsOne(i => i.Phone);
            builder.Entity<SmsLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<BlackListEntry>().OwnsOne(i => i.CreatedBy);
            builder.Entity<BlackListEntry>().OwnsOne(i => i.ResolvedBy);
            builder.Entity<BlackListEntryLog>().OwnsOne(i => i.CreatedBy);

            //builder.Entity<PhoneConfiguration>().HasKey(c => c.Id).ForSqlServerIsClustered(false);
            //builder.Entity<PhoneConfiguration>().HasIndex("Id").HasName("ClusteredId").ForSqlServerIsClustered(true);

            builder.Entity<Operator>().HasData(new Operator { Id = 1, Type = OperatorType.Turkcell, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 2, Type = OperatorType.Vodafone, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 3, Type = OperatorType.TurkTelekom, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 4, Type = OperatorType.MarketingChannel, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 5, Type = OperatorType.IVN, ControlDaysForOtp = 60, Status = OperatorStatus.Active });


            builder.Entity<Header>().HasData(new Header { Id = Guid.NewGuid(), SmsSender = "BATMAN", SmsPrefix = "Dear Honey,", SmsSuffix = ":)", EmailTemplatePrefix = "generic", SmsTemplatePrefix = "generic" });
            builder.Entity<Header>().HasData(new Header { Id = Guid.NewGuid(), Branch = 2000, SmsSender = "ZEUS", SmsPrefix = "OBEY:", EmailTemplatePrefix = "on", SmsTemplatePrefix = "on" });
        }

    }
}
