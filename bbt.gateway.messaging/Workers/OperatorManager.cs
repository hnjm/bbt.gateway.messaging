using bbt.gateway.messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Workers
{
    public class OperatorManager
    {
        List<Operator> operators = new List<Operator>();

        private OperatorManager()
        {
            loadOperators();
        }

        private static readonly Lazy<OperatorManager> lazy = new Lazy<OperatorManager>(() => new OperatorManager());
        public static OperatorManager Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public Operator Get(OperatorType type)
        {
            return operators.FirstOrDefault(o => o.Type == type);
        }

        public void Save(Operator data)
        {
            using (var db = new DatabaseContext())
            {
                if (!operators.Any(o => o.Id == data.Id))
                {
                    throw new NotSupportedException("Adding new operator is not allowed.");
                }
                else
                {
                    db.Operators.Update(data);
                }
                db.SaveChanges();
            }

            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
            loadOperators();
        }




        private void loadOperators()
        {
            using (var db = new DatabaseContext())
            {
                operators = db.Operators.ToList();
            }
        }
    }
}
