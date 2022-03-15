using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Core
{
    public enum CacheStatus
    {
        Found = 200,
        Create = 201,
        NotFound = 404,
        Conflict = 409,
        InternalError = 500
    }


    public enum FileStatus
    {
        Empty = 10,
        Busy = 20,
        Available = 30,
        Dispached = 40,
        Quarantine = 50,
        TryAgain = 60

    }


    class CommonCore
    {



    }
}
