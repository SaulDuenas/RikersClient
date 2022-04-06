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
    public class StatusFileRepository : IRepository<StatusFileDomain, StatusFileDomain>, IDisposable
    {
        private DataServiceEntities _context = null;

        public StatusFileRepository()
        {
            _context = new DataServiceEntities();
        }

        public int Create(StatusFileDomain entity)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<StatusFileDomain, StatusFile>(); });
            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<StatusFileDomain, StatusFile>(entity);

            _context.StatusFile.Add(destination);

            return _context.SaveChanges();
        }

       
        public StatusFileDomain Find(int id)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<StatusFile,StatusFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

           var source = _context.StatusFile.Where(p => p.Id == id).FirstOrDefault();

           return iMapper.Map<StatusFile, StatusFileDomain>(source);

        }


        public StatusFileDomain Find(string id)
        {
            throw new NotImplementedException();
        }


        public int Modify(StatusFileDomain entity)
        {
            throw new NotImplementedException();
        }

        public List<StatusFileDomain> ReadAll(StatusFileDomain status)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<StatusFile, StatusFileDomain>(); });
            IMapper iMapper = config.CreateMapper();

            var source = _context.StatusFile.Select(c => iMapper.Map<StatusFile, StatusFileDomain>(c)).ToList();

            return source;
        }

        public int Delete(StatusFileDomain entity)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_context != null) { _context.Dispose(); }
        }

        public List<StatusFileDomain> ReadAll()
        {
            throw new NotImplementedException();
        }

        public List<StatusFileDomain> ReadAll(string id)
        {
            throw new NotImplementedException();
        }
    }
}
