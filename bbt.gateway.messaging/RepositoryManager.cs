using bbt.gateway.messaging.Repositories;

namespace bbt.gateway.messaging
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

        public RepositoryManager(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public IHeaderRepository Headers => _headerRepository = _headerRepository ?? new HeaderRepository(_databaseContext);

        public IOperatorRepository Operators => _operatorRepository = _operatorRepository ?? new OperatorRepository(_databaseContext);

        public IBlacklistEntryRepository BlackListEntries => _blacklistEntryRepository = _blacklistEntryRepository ?? new BlacklistEntryRepository(_databaseContext);

        public IPhoneConfigurationRepository PhoneConfigurations => _phoneConfigurationRepository = _phoneConfigurationRepository ?? new PhoneConfigurationRepository(_databaseContext);

        public IOtpRequestLogRepository OtpRequestLogs => _otpRequestLogRepository = _otpRequestLogRepository ?? new OtpRequestLogRepository(_databaseContext);

        public ISmsLogRepository SmsLogs => _smsLogRepository = _smsLogRepository ?? new SmsLogRepository(_databaseContext);

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
