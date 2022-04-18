using System;

namespace bbt.gateway.common.Repositories
{
    public interface IRepositoryManager : IDisposable
    {
        IHeaderRepository Headers { get; }
        IOperatorRepository Operators { get; }
        IBlacklistEntryRepository BlackListEntries { get; }
        IPhoneConfigurationRepository PhoneConfigurations { get; }
        IMailConfigurationRepository MailConfigurations { get; }
        IOtpRequestLogRepository OtpRequestLogs { get; }
        ISmsResponseLogRepository SmsResponseLogs { get;  }
        ISmsRequestLogRepository SmsRequestLogs { get; }
        IMailRequestLogRepository MailRequestLogs { get; }
        IMailResponseLogRepository MailResponseLogs { get; }
        IOtpResponseLogRepository OtpResponseLogs { get; }
        IOtpTrackingLogRepository OtpTrackingLog { get; }
        ITransactionRepository Transactions { get; }
        IUserRepository Users { get; }
        IDirectBlacklistRepository DirectBlacklists { get; }
        int SaveChanges();
        int SaveDodgeChanges();
        int SaveSmsBankingChanges();
    }
}
