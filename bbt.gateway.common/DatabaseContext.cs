using bbt.gateway.common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace bbt.gateway.common
{
    public class DatabaseContext : DbContext
    {
        public DbSet<PhoneConfiguration> PhoneConfigurations { get; set; }
        public DbSet<Header> Headers { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<BlackListEntry> BlackListEntries { get; set; }
        public DbSet<OtpRequestLog> OtpRequestLogs { get; set; }
        public DbSet<OtpResponseLog> OtpResponseLog { get; set; }
        public DbSet<SmsLog> SmsLogs { get; set; }
        public DbSet<MailConfiguration> MailConfigurations { get; set; }
        public DbSet<MailRequestLog> MailRequestLog { get; set; }
        public DbSet<MailResponseLog> MailResponseLog { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PhoneConfiguration>().OwnsOne(i => i.Phone);
            builder.Entity<PhoneConfigurationLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<MailConfigurationLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<MailRequestLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<OtpRequestLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<OtpRequestLog>().OwnsOne(i => i.Phone);
            builder.Entity<SmsLog>().OwnsOne(i => i.CreatedBy);
            builder.Entity<BlackListEntry>().OwnsOne(i => i.CreatedBy);
            builder.Entity<BlackListEntry>().OwnsOne(i => i.ResolvedBy);
            builder.Entity<BlackListEntryLog>().OwnsOne(i => i.CreatedBy);
            
            //Non-cluster Guid index sample
            builder.Entity<PhoneConfiguration>()
                .HasIndex(c => c.Id)
                .IsClustered(false);


            builder.Entity<Operator>().HasData(new Operator { Id = 1, Type = OperatorType.Turkcell, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 2, Type = OperatorType.Vodafone, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 3, Type = OperatorType.TurkTelekom, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 4, Type = OperatorType.MarketingChannel, ControlDaysForOtp = 60, Status = OperatorStatus.Active });
            builder.Entity<Operator>().HasData(new Operator { Id = 5, Type = OperatorType.IVN, ControlDaysForOtp = 60, Status = OperatorStatus.Active });

            builder.Entity<Header>().HasData(new Header { Id = Guid.NewGuid(), SmsSender = SenderType.Burgan, SmsPrefix = "Dear Honey,", SmsSuffix = ":)", EmailTemplatePrefix = "generic", SmsTemplatePrefix = "generic" });
            builder.Entity<Header>().HasData(new Header { Id = Guid.NewGuid(), Branch = 2000, SmsSender = SenderType.On, SmsPrefix = "OBEY:", EmailTemplatePrefix = "on", SmsTemplatePrefix = "on" });
        }

    }
}
