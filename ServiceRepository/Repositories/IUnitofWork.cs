using Service.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Repositories
{ 
    public interface IUnitofWork : IDisposable
    {
       IInBoundFileRepository InboundFileRepository { get; }
       int Complete();

    }
}
