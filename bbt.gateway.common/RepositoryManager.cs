using bbt.gateway.common.Repositories;

namespace bbt.gateway.common
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly DatabaseContext _databaseContext;
        private HeaderRepository _headerRepository;
        private OperatorRepository _operatorRepository;
        private BlacklistEntryRepository _blacklistEntryRepository;
        private PhoneConfigurationRepository _phoneConfigurationRepository;
        private OtpRequestLogRepository _otpRequestLogRepository;
        private SmsLogRepository _smsLogRepository;
        private OtpResponseLogRepository _otpResponseLogRepository;
        private OtpTrackingLogRepository _otpTrackingLogRepository;

        public RepositoryManager(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public IHeaderRepository Headers => _headerRepository ??= new HeaderRepository(_databaseContext);

        public IOperatorRepository Operators => _operatorRepository ??= new OperatorRepository(_databaseContext);

        public IBlacklistEntryRepository BlackListEntries => _blacklistEntryRepository ??= new BlacklistEntryRepository(_databaseContext);

        public IPhoneConfigurationRepository PhoneConfigurations => _phoneConfigurationRepository ??= new PhoneConfigurationRepository(_databaseContext);

        public IOtpRequestLogRepository OtpRequestLogs => _otpRequestLogRepository ??= new OtpRequestLogRepository(_databaseContext);

        public ISmsLogRepository SmsLogs => _smsLogRepository ??= new SmsLogRepository(_databaseContext);
        
        public IOtpResponseLogRepository OtpResponseLogs => _otpResponseLogRepository ??= new OtpResponseLogRepository(_databaseContext);

        public IOtpTrackingLogRepository OtpTrackingLog => _otpTrackingLogRepository ??= new OtpTrackingLogRepository(_databaseContext);

        public int SaveChanges()
        {
            return _databaseContext.SaveChanges();
        }

        public void Dispose()
        {
            _databaseContext.Dispose();
        }
    }
}
