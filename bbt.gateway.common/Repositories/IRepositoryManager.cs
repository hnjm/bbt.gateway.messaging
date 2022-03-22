using System;

namespace bbt.gateway.common.Repositories
{
    public interface IRepositoryManager : IDisposable
    {
        IHeaderRepository Headers { get; }
        IOperatorRepository Operators { get; }
        IBlacklistEntryRepository BlackListEntries { get; }
        IPhoneConfigurationRepository PhoneConfigurations { get; }
        IOtpRequestLogRepository OtpRequestLogs { get; }
        ISmsLogRepository SmsLogs { get;  }
        IOtpResponseLogRepository OtpResponseLogs { get; }
        IOtpTrackingLogRepository OtpTrackingLog { get; }
        IUserRepository Users { get; }
        int SaveChanges();
        int SaveDodgeChanges();
    }
}
