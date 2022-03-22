using bbt.gateway.common.Extensions;
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bbt.gateway.messaging.Workers
{
    public class OperatorManager
    {
        List<Operator> operators = new List<Operator>();
        private readonly IRepositoryManager _repositoryManager;
        public OperatorManager(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
            loadOperators();
        }

        public OperatorInfo[] Get()
        {            
            var operatorList = _repositoryManager.Operators.GetAll();
            var operatorInfoList = new List<OperatorInfo>();

            foreach (var item in operatorList)
            {
                operatorInfoList.Add(item.MapTo<OperatorInfo>());
            }

            return operatorInfoList.ToArray();
        }

        public Operator Get(OperatorType type)
        {
            return _repositoryManager.Operators.FirstOrDefault(o => o.Type == type);
        }


        public void Save(Operator data)
        {
            
            if (!operators.Any(o => o.Id == data.Id))
            {
                throw new NotSupportedException("Adding new operator is not allowed.");
            }
            else
            {
                _repositoryManager.Operators.Update(data);
            }
            _repositoryManager.SaveChanges();

            //TODO: Meanwhile, dont forget to inform other pods to invalidate headers cahce.
            loadOperators();
        }

        private void loadOperators()
        {
            operators = _repositoryManager.Operators.GetAll().ToList();
        }
    }
}
