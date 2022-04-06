using AutoMapper;
using Service.AtmCat;
using Service.Domian.Model;
using Service.Domian.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Implementation
{
    public class AtmCatRepository : IRepository<AtmCatDomain, StatusFileDomain>, IDisposable
    {

        private AtmCatEntities _context = null;

        public AtmCatRepository()
        {
            _context = new AtmCatEntities();
        }

      
        public AtmCatDomain Find(string id)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<Atm, AtmCatDomain>(); });
            IMapper iMapper = config.CreateMapper();

            var source = _context.Atm.Where(p => (p.IbmSerial == id || p.CitiBanamexId == id)).FirstOrDefault();

            return iMapper.Map<Atm, AtmCatDomain>(source);
        }


        public bool exists(string id)
        {

            var source = _context.Atm.ToList();

            var retval = source.Where(p => (p.IbmSerial == id || p.CitiBanamexId == id )).FirstOrDefault();

            return (retval != null);
        }

        public int Create(AtmCatDomain entity)
        {
            throw new NotImplementedException();
        }

        public int Delete(AtmCatDomain entity)
        {
            throw new NotImplementedException();
        }

        public AtmCatDomain Find(int id)
        {
            throw new NotImplementedException();
        }

        public int Modify(AtmCatDomain entity)
        {
            throw new NotImplementedException();
        }

        public List<AtmCatDomain> ReadAll()
        {
            throw new NotImplementedException();
        }

        public List<AtmCatDomain> ReadAll(StatusFileDomain entity)
        {
            throw new NotImplementedException();
        }

        public List<AtmCatDomain> ReadAll(string id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_context != null) { _context.Dispose(); }
        }

    }
}
