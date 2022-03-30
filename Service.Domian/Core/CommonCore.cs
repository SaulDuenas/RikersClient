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


    static public class CommonCore
    {
       static public byte FILE_RESPONSE_NO_CREATE = 0;
       static public byte FILE_RESPONSE_CREATE = 1;

        static public byte FILE_NOT_EXIST = 2;
        static public byte FILE_NOT_MOVE = 0;
       static public byte FILE_MOVE = 1;

       static public byte FILE_PROCESSED = 1;
       static public byte FILE_NOT_PROCESSED = 0;

    }
}
