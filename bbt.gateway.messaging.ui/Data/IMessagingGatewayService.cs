using bbt.gateway.common.Models;
using bbt.gateway.messaging.ui.Data;
using Refit;

namespace bbt.gateway.messaging.ui.Data
{
    public interface IMessagingGatewayService
    {
        [Get("/api/v1/Administration/transactions/{phone.CountryCode}/{phone.Prefix}/{phone.Number}")]
        Task<List<Transaction>> GetTransactionsByPhone(Phone phone, QueryParams queryParams);

    }
}
