using bbt.gateway.common.Models;
using bbt.gateway.messaging.ui.Data;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;


namespace bbt.gateway.messaging.ui.Pages
{
    public partial class SearchMessages : ComponentBase
    {
        private IEnumerable<Transaction>? transactions;
        private SearchModel searchModel = new SearchModel();
        private int pageCount = 10;
        private int rowsCount = 0;
        private bool useSpinner;
        private RadzenDataGrid<Transaction> grid;

        void SelectionChanged(int i)
        {
            searchModel.SelectedSearchType = i;
        }

        protected override async Task OnInitializedAsync()
        {
            useSpinner = false;
            dialogService.OnOpen += Open;
            dialogService.OnClose += Close;
        }

        public void Dispose()
        {
            // The DialogService is a singleton so it is advisable to unsubscribe.
            dialogService.OnOpen -= Open;
            dialogService.OnClose -= Close;
        }

        void Open(string title, Type type, Dictionary<string, object> parameters, DialogOptions options)
        {
            
        }

        void Close(dynamic result)
        {
            
        }

        public async Task OpenSmsDetails(Transaction txn)
        {

            await dialogService.OpenAsync<MessageDetails>("title", new Dictionary<string, object>() { { "Txn", txn} }, new DialogOptions() { ShowTitle = false, Style = "min-height:auto;min-width:auto;max-height:600px;width:60%", CloseDialogOnEsc = false });

        }

        public bool CheckSmsStatus(Transaction txn)
        {
            if (txn.TransactionType == TransactionType.Otp)
            {
                if (txn.OtpRequestLog != null)
                { 
                    if(txn.OtpRequestLog.ResponseLogs != null)
                    {
                        return txn.OtpRequestLog.ResponseLogs.Any(l => l.TrackingStatus == SmsTrackingStatus.Delivered);
                    }
                }
            }
            if (txn.TransactionType == TransactionType.TransactionalMail || txn.TransactionType == TransactionType.TransactionalTemplatedMail)
            {
                if (txn.MailRequestLog != null)
                {
                    if (txn.MailRequestLog.ResponseLogs != null)
                    {
                        return txn.MailRequestLog.ResponseLogs.Any(l => l.ResponseCode == "0");
                    }
                }
            }
            if (txn.TransactionType == TransactionType.TransactionalSms || txn.TransactionType == TransactionType.TransactionalTemplatedSms)
            {
                if (txn.SmsRequestLog != null)
                {
                    if (txn.SmsRequestLog.ResponseLogs != null)
                    {
                        return txn.SmsRequestLog.ResponseLogs.Any(l => l.OperatorResponseCode == 0);
                    }
                }
            }
            if (txn.TransactionType == TransactionType.TransactionalPush || txn.TransactionType == TransactionType.TransactionalTemplatedPush)
            {
                if (txn.PushNotificationRequestLog != null)
                {
                    if (txn.PushNotificationRequestLog.ResponseLogs != null)
                    {
                        return txn.PushNotificationRequestLog.ResponseLogs.Any(l => l.ResponseCode == "0");
                    }
                }
            }

            return false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                grid.EmptyText = "Hiç Kayıt Bulunamadı.";
            }
        }

        async Task SearchTransactions(LoadDataArgs args = null)
        {
            useSpinner = true;
            if (args == null)
            {
                searchModel.Skip = 0;
                searchModel.Take = pageCount;
            }
            else
            {
                var skip = args.Skip ?? 0;
                var top = args.Top ?? pageCount;
                searchModel.Skip = skip / top;
                searchModel.Take = top;
            }
            switch (searchModel.SelectedSearchType)
            {
                case 1:
                    await SearchWithCustomerNo();
                    break;
                case 2:
                    await SearchWithCitizenshipNo();
                    break;
                case 3:
                    await SearchWithPhone();
                    break;
                case 4:
                    await SearchWithMail();
                    break;
                default:
                    throw new Exception();
            }
            useSpinner = false;
        }

        async Task SearchWithPhone()
        {
            var res  = await MessagingGateway.GetTransactionsByPhone(new Phone(searchModel.FilterValue), CreateQueryParams());
            transactions = res.Transactions.AsODataEnumerable();
            rowsCount = res.Count;
        }

        async Task SearchWithMail()
        {
            var res = await MessagingGateway.GetTransactionsByMail(searchModel.FilterValue,CreateQueryParams());
            transactions = res.Transactions;
            rowsCount = res.Count;
        }

        async Task SearchWithCustomerNo()
        {
            try
            {
                var res = await MessagingGateway.GetTransactionsByCustomerNo(Convert.ToUInt64(searchModel.FilterValue), Constants.MessageTypeMap[searchModel.MessageType], CreateQueryParams());
                transactions = res.Transactions;
                rowsCount = res.Count;
            }
            catch (Exception ex)
            { 
                transactions = new List<Transaction>();
                rowsCount = 0;
            }
        }

        async Task SearchWithCitizenshipNo()
        {
            var res = await MessagingGateway.GetTransactionsByCitizenshipNo(searchModel.FilterValue, Constants.MessageTypeMap[searchModel.MessageType], CreateQueryParams());
            transactions = res.Transactions;
            rowsCount = res.Count;
        }

        void OnChange(DateTime? value, string name, string format)
        {
            
        }

        void SelectMessageType(object value, string name)
        {
            searchModel.MessageType = Enum.Parse<MessageTypeEnum>(value.ToString());

        }

        void SelectSmsType(object value, string name)
        {
            searchModel.SmsType = Enum.Parse<SmsTypeEnum>(value.ToString());

        }

        void MessageTableSort(DataGridColumnSortEventArgs<Transaction> args)
        {
            //PropertyDescriptor? prop = TypeDescriptor.GetProperties(typeof(Transaction)).Find(args.Column.Property,true);
            
            
        }

        QueryParams CreateQueryParams()
        {
            return new QueryParams()
            {
                StartDate = searchModel.StartDate.Date,
                EndDate = searchModel.EndDate.Date.AddDays(1),
                page = searchModel.Skip,
                pageSize = searchModel.Take,
                smsType = Constants.SmsTypeMap[searchModel.SmsType]
            };
        }
    }

    
}
