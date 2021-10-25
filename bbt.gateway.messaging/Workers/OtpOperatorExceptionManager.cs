using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class OtpOperatorExceptionManager
    {
        List<OtpOperatorException> exceptions = new List<OtpOperatorException>();

        private OtpOperatorExceptionManager()
        {
            loadExceptions();
        }

        private static readonly Lazy<OtpOperatorExceptionManager> lazy = new Lazy<OtpOperatorExceptionManager>(() => new OtpOperatorExceptionManager());
        public static OtpOperatorExceptionManager Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private void loadExceptions()
        {
            using (var db = new DatabaseContext())
            {
                exceptions = db.OtpOperatorExceptions.Where(e => e.Status == "active" && e.ValidTo > DateTime.Now).ToList();
            }
        }

        private void saveExceptions(OtpOperatorException exception)
        {
            using (var db = new DatabaseContext())
            {
                //TODO: Meanwhile, dont forget to inform other pods to invalidate Exceptions cahce.
                db.OtpOperatorExceptions.Add(exception);
                db.SaveChanges();
                loadExceptions();
            }
        }
    }
}
