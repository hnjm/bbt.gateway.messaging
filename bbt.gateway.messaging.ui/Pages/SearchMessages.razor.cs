using bbt.gateway.common.Models;
using bbt.gateway.messaging.ui.Data;
using Microsoft.AspNetCore.Components;

namespace bbt.gateway.messaging.ui.Pages
{
    public partial class SearchMessages : ComponentBase
    {
        private List<Transaction>? transactions = null;
        private int selectedFilterType = 1;
        private string filterValue { get; set; } = "";
        void SelectionChanged(int i)
        {
            selectedFilterType = i;
        }

        protected override async Task OnInitializedAsync()
        {

        }

        async Task SearchTransactions()
        {
            switch (selectedFilterType)
            {
                case 1:
                    throw new Exception();
                case 2:
                    throw new Exception();
                case 3:
                    await SearchWithPhone();
                    break;
                case 4:
                    throw new Exception();
                default:
                    throw new Exception();
            }
        }

        async Task SearchWithPhone()
        {
            transactions = await MessagingGateway.GetTransactionsByPhone(new Phone(filterValue), new QueryParams()
            {
                page = 0,
                pageSize = 10
            });
        }
    }
}
