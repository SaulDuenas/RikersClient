using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebServiceTest.DTOs;

namespace WebServiceTest.Models
{
    public class InBoundFileMock
    {

        static public List<InBoundFileDTO> FileList = new List<InBoundFileDTO>() { new InBoundFileDTO { Id=1,  FileName = "File01", FullPath = @"C:\pruebita\", DateReg = DateTime.Today.ToString("yyyy-MM-dd"), DateCreated =DateTime.Today.ToString("yyyy-MM-dd") },
                                                                                   new InBoundFileDTO {Id=2, FileName = "File02", FullPath = @"C:\pruebita\" , DateReg = DateTime.Today.ToString("yyyy-MM-dd"), DateCreated =DateTime.Today.ToString("yyyy-MM-dd")  },
                                                                                   new InBoundFileDTO {Id=3, FileName = "File03", FullPath = @"C:\pruebita\"  , DateReg = DateTime.Today.ToString("yyyy-MM-dd"), DateCreated =DateTime.Today.ToString("yyyy-MM-dd")  }
                                                                                 };


    }
}