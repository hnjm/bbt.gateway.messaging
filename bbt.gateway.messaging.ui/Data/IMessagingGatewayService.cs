using bbt.gateway.common.Models;
using bbt.gateway.messaging.ui.Data;
using Refit;

namespace bbt.gateway.messaging.ui.Data
{
    public interface IMessagingGatewayService
    {
        [Get("/api/v1/Administration/transactions/phone/{phone.CountryCode}/{phone.Prefix}/{phone.Number}")]
        Task<TransactionsDto> GetTransactionsByPhone(Phone phone,QueryParams queryParams);
        [Get("/api/v1/Administration/transactions/mail/{mail}")]
        Task<TransactionsDto> GetTransactionsByMail(string mail, QueryParams queryParams);
        [Get("/api/v1/Administration/transactions/customer/{customerNo}/{messageType}")]
        Task<TransactionsDto> GetTransactionsByCustomerNo(ulong customerNo,int messageType, QueryParams queryParams);
        [Get("/api/v1/Administration/transactions/citizen/{citizenshipNo}/{messageType}")]
        Task<TransactionsDto> GetTransactionsByCitizenshipNo(string citizenshipNo, int messageType, QueryParams queryParams);

        [Get("/api/v1/Administration/blacklists/{countryCode}/{prefix}/{number}")]
        Task<IEnumerable<BlackListEntry>> GetBlackListByPhone(int countryCode, int prefix, int number, QueryParams queryParams);
        [Get("/api/v1/Administration/blacklists/customer/{customerNo}")]
        Task<BlackListEntriesDto> GetBlackListEntriesByCustomerNo(ulong customerNo, QueryParams queryParams);
        [Get("/api/v1/Administration/blacklists/phone/{countryCode}/{prefix}/{number}")]
        Task<BlackListEntriesDto> GetBlackListEntriesByPhone(int countryCode, int prefix, int number, QueryParams queryParams);
        [Get("/api/v1/Administration/user/control/{userName}")]
        Task<bool> GetUserControl(string userName);

    }
}
