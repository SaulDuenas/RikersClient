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
    class CommentFileRepository : IRepository<FeedBackFileDomain, StatusFileDomain>, IDisposable
    {

        private DataServiceEntities _context = null;

        public CommentFileRepository()
        {
            _context = new DataServiceEntities();
        }


        public int Create(FeedBackFileDomain entity)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<FeedBackFileDomain, CommentFile>(); });
            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<FeedBackFileDomain, CommentFile>(entity);

            _context.CommentFile.Add(destination);

            return _context.SaveChanges();
        }


        public bool exists(string id)
        {

            var source = _context.CommentFile.Where(p => p.FileName == id).FirstOrDefault();

            return (source != null);
        }


        public int Delete(FeedBackFileDomain entity)
        {
            throw new NotImplementedException();
        }


        public FeedBackFileDomain Find(string id)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<CommentFile, FeedBackFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

            var source = _context.CommentFile.Where(p => p.FileName == id).FirstOrDefault();

            return iMapper.Map<CommentFile, FeedBackFileDomain>(source);
        }

        public FeedBackFileDomain Find(int id)
        {
            throw new NotImplementedException();
        }

        public int Modify(FeedBackFileDomain entity)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<FeedBackFileDomain, CommentFile>(); });
            IMapper iMapper = config.CreateMapper();

            var destiny = _context.CommentFile.First(p => p.FileName == entity.FileName);

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



        public List<FeedBackFileDomain> ReadAll(List<int> statusLts)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<CommentFile, FeedBackFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

            List<CommentFile> source = new List<CommentFile>();

            if (statusLts == null)
            {
                source = _context.CommentFile.ToList();
            }
            else
            {
                source = _context.CommentFile.Where(p => statusLts.Contains((int)p.Status)).ToList();
            }

            var result = source.Select(item => iMapper.Map<CommentFile, FeedBackFileDomain>(item)).ToList();

            return result;

        }






        public List<FeedBackFileDomain> ReadAll(StatusFileDomain status)
        {
            throw new NotImplementedException();
        }



        public void Dispose()
        {
            if (_context != null) { _context.Dispose(); }
        }

        public List<FeedBackFileDomain> ReadAll()
        {
            throw new NotImplementedException();
        }

        public List<FeedBackFileDomain> ReadAll(string id)
        {
            throw new NotImplementedException();
        }
    }
}
