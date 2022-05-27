
using bbt.gateway.common.Models;

namespace bbt.gateway.messaging.ui.Data
{
    public class Constants
    {
        public static Dictionary<int, FilterInput> Filters = new()
        {
            { 1, new FilterInput {Name="Müşteri No",Helpline="Müşteri numarasını giriniz."} },
            { 2, new FilterInput { Name = "Kimlik No", Helpline = "Müşteri kimlik numarasını giriniz." } },
            { 3, new FilterInput { Name = "Telefon No", Helpline = "Müşteri telefon numarasını boşluksuz giriniz. {ÜlkeKodu}{AlanKodu}{Numara}" } },
            { 4, new FilterInput { Name = "E-Mail", Helpline = "Müşteri E-Mail adresini giriniz." } },
        };

        public static Dictionary<TransactionType, string> TransactionTypeMap = new()
        {
            { TransactionType.Otp, "Otp" },
            { TransactionType.TransactionalPush, "Push Notification" },
            { TransactionType.TransactionalTemplatedPush, "Push Notification" },
            { TransactionType.TransactionalMail, "E-Mail" },
            { TransactionType.TransactionalTemplatedMail, "E-Mail" },
            { TransactionType.TransactionalSms, "Sms" },
            { TransactionType.TransactionalTemplatedSms, "Sms" }
        };
    }
}
