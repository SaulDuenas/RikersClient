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
    class CommentFileRepository : IRepository<CommentFileDomain, StatusFileDomain>, IDisposable
    {

        private DataServiceEntities _context = null;

        public CommentFileRepository()
        {
            _context = new DataServiceEntities();
        }


        public int Create(CommentFileDomain entity)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<CommentFileDomain, CommentFile>(); });
            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<CommentFileDomain, CommentFile>(entity);

            _context.CommentFile.Add(destination);

            return _context.SaveChanges();
        }


        public bool exists(string id)
        {

            var source = _context.CommentFile.Where(p => p.FileName == id).FirstOrDefault();

            return (source != null);
        }


        public int Delete(CommentFileDomain entity)
        {
            throw new NotImplementedException();
        }


        public CommentFileDomain Find(string id)
        {
            throw new NotImplementedException();
        }

        public CommentFileDomain Find(int id)
        {
            throw new NotImplementedException();
        }

        public int Modify(CommentFileDomain entity)
        {
            throw new NotImplementedException();
        }

        public List<CommentFileDomain> ReadAll(StatusFileDomain status)
        {
            throw new NotImplementedException();
        }



        public void Dispose()
        {
            if (_context != null) { _context.Dispose(); }
        }

        public List<CommentFileDomain> ReadAll()
        {
            throw new NotImplementedException();
        }

        public List<CommentFileDomain> ReadAll(string id)
        {
            throw new NotImplementedException();
        }
    }
}
