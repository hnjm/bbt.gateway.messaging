using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Repositories
{
    public interface IRepositoryManager : IDisposable
    {
        IHeaderRepository Headers { get; }
        IOperatorRepository Operators { get; }
        IBlacklistEntryRepository BlackListEntries { get; }
        IPhoneConfigurationRepository PhoneConfigurations { get; }
        IOtpRequestLogRepository OtpRequestLogs { get; }
        ISmsLogRepository SmsLogs { get;  }
        int SaveChanges();
    }
}
