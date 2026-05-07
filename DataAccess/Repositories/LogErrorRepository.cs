using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Repositories
{
    internal class LogErrorRepository : ILogErrorRepository
    {
        public Task AddAsync(LogError logError)
        {
            throw new NotImplementedException();
        }
    }
}
