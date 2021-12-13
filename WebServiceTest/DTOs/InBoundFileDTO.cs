using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServiceTest.DTOs
{
    public class InBoundFileDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string DateReg { get; set; }
        public string DateCreated { get; set; }
    }
}