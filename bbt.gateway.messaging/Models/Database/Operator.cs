using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Models
{
    public class Operator
    {
        public int Id { get; set; }
        public OperatorType Type { get; set; }
        public int ControlDaysForOtp { get; set;}
        public bool UseIvnWhenDeactive { get; set;}
        public OperatorStatus Status  { get; set; }
    }
   
}
