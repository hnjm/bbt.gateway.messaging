﻿using bbt.gateway.common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bbt.gateway.common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace bbt.gateway.messaging.Workers.OperatorGateway
{
    public abstract class OperatorGatewayBase
    {
        private OperatorType type;
        protected readonly IConfiguration Configuration;
        private DbContextOptions<DatabaseContext> _dbOptions;
        protected OperatorGatewayBase(IConfiguration configuration) 
        {
            Configuration = configuration;
            _dbOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .Options;
        }

        protected OperatorType Type
        {
            get { return type; }
            set
            {
                type = value;
                using var databaseContext = new DatabaseContext(_dbOptions);
                OperatorConfig = databaseContext.Operators.FirstOrDefault(o => o.Type == type);
            }
        }
        protected Operator OperatorConfig { get; set; }

        protected void SaveOperator()
        {
            using var databaseContext = new DatabaseContext(_dbOptions);
            databaseContext.Operators.Update(OperatorConfig);
            databaseContext.SaveChanges();
        }

        protected PhoneConfiguration GetPhoneConfiguration(Phone phone)
        {
            using var databaseContext = new DatabaseContext(_dbOptions);
            return databaseContext.PhoneConfigurations.Where(i =>
                i.Phone.CountryCode == phone.CountryCode &&
                i.Phone.Prefix == phone.Prefix &&
                i.Phone.Number == phone.Number
                )
                .FirstOrDefault();
        }

    }



}
