using Service.Repositories;
using Service.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public interface IInBoundFileRepository : IRepository<InBoundFileDomain>
    {
        IEnumerable<InBoundFileDomain> getNotProcessed();

        InBoundFileDomain getByFullName(string fullName);

        bool FileProcessed(string fullName);

    }
}
