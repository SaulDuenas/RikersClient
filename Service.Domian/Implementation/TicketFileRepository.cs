using AutoMapper;
using Service.DataAccess.ORM;
using Service.Domian.Model;
using Service.Domian.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Implementation
{
    class TicketFileRepository : IRepository<TicketFileDomain, StatusFileDomain>, IDisposable
    {

        private DataServiceEntities _context = null;

        public TicketFileRepository()
        {
            _context = new DataServiceEntities();
        }

        public int Create(TicketFileDomain entity)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFileDomain, TicketFile>(); });
            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<TicketFileDomain, TicketFile>(entity);

            _context.TicketFile.Add(destination);

            return _context.SaveChanges();
        }


        public TicketFileDomain Find(string id)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFile, TicketFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

            var source = _context.TicketFile.Where(p => p.FileName == id).FirstOrDefault();

            return iMapper.Map<TicketFile, TicketFileDomain>(source);
        }


        public bool exists(string id)
        {

            var source = _context.TicketFile.Where(p => p.FileName == id).FirstOrDefault();

            return (source != null);
        }


        public bool exists(TicketFileDomain entity)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFileDomain, TicketFile>(); });
            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<TicketFileDomain, TicketFile>(entity);

            var source = _context.TicketFile.Where(p => p.Equals(destination)).FirstOrDefault();

            return (source != null);
        }



        public TicketFileDomain Find(int id)
        {
            throw new NotImplementedException();
        }

        public int Modify(TicketFileDomain entity)
        {
              var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFileDomain, TicketFile>(); });
              IMapper iMapper = config.CreateMapper();
        
            var destiny = _context.TicketFile.First(p => p.FileName == entity.FileName);

           // ticketfile = iMapper.Map<TicketFileDomain, TicketFile>(entity);

            
            if (destiny != null)
            {
                // var destination = iMapper.Map<TicketFileDomain, TicketFile>(entity);
                destiny.FullPath = entity.FullPath;
                destiny.DateCreate = entity.DateCreate;
                destiny.DateModified = entity.DateModified;
                destiny.Length = entity.Length;
                destiny.Status = entity.Status;
                destiny.NoTicket = entity.NoTicket;
                destiny.Attempts = entity.Attempts;
                destiny.Response = entity.Response;
                destiny.CaseNumber = entity.CaseNumber;
                destiny.TransactionId = entity.TransactionId;
                destiny.TransactionDate = entity.TransactionDate;
                destiny.DateResponse = entity.DateResponse;
                destiny.FileResponseCreated = entity.FileResponseCreated;
                destiny.FullPathResponse = entity.FullPathResponse;
                destiny.DateNextAttempt = entity.DateNextAttempt;
                destiny.Message = entity.Message;
                destiny.Processed = entity.Processed;
                destiny.FileMove = entity.FileMove;

            }


            //  var ticketfile = new TicketFile() { FileName = entity.FileName};
            //  _context.TicketFile.Attach(destination);

            //  _context.Configuration.ValidateOnSaveEnabled = false;

            return _context.SaveChanges();

        }


        public int Delete(TicketFileDomain entity)
        {

            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFileDomain, TicketFile>(); });
            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<TicketFileDomain, TicketFile>(entity);

            //  var ticketfile = _context.TicketFile.Find(destination);

            //  var ticketfile = new TicketFile() { FileName = entity.FileName};
            _context.TicketFile.Attach(destination);
            _context.TicketFile.Remove(destination);

          //  _context.Configuration.ValidateOnSaveEnabled = false;

            return _context.SaveChanges();

        }


        public List<TicketFileDomain> ReadAll(StatusFileDomain status)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFile, TicketFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

            return _context.TicketFile.Where(p => (p.Status == status.Id  || status == null)).Select(item => iMapper.Map<TicketFile, TicketFileDomain>(item)).ToList();

        }


        public List<TicketFileDomain> ReadAll(List<int> statusLts)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFile, TicketFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

            List<TicketFile> source = new List<TicketFile>();

            if (statusLts == null)
            {
                 source = _context.TicketFile.ToList();
            }
            else 
            {
                 source = _context.TicketFile.Where(p => statusLts.Contains((int)p.Status)).ToList();
            }

            var result = source.Select(item => iMapper.Map<TicketFile, TicketFileDomain>(item)).ToList();

            return result;

        }


        public void Dispose()
        {
            if (_context != null) { _context.Dispose(); }
        }

        public List<TicketFileDomain> ReadAll()
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketFile, TicketFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

            List<TicketFile> source = _context.TicketFile.ToList();
           
            var result = source.Select(item => iMapper.Map<TicketFile, TicketFileDomain>(item)).ToList();

            return result;
        }

        public List<TicketFileDomain> ReadAll(string id)
        {
            throw new NotImplementedException();
        }
    }
}
