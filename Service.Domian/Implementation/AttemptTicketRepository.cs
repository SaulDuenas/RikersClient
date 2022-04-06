using AutoMapper;
using Service.DataAccess.ORM;
using Service.Domian.Model;
using Service.Domian.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Implementation
{
    public class AttemptTicketRepository : IRepository<AttemptTicketDomain, StatusFileDomain>, IDisposable
    {

        private DataServiceEntities _context = null;

        public AttemptTicketRepository()
        {
            _context = new DataServiceEntities();
        }

        public int Create(AttemptTicketDomain entity)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<AttemptTicketDomain, TicketAttempt>(); });
            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<AttemptTicketDomain, TicketAttempt>(entity);

            _context.TicketAttempt.Add(destination);

            return _context.SaveChanges();
        }

        public AttemptTicketDomain Find(string id)
        {
            throw new NotImplementedException();
        }

        public AttemptTicketDomain Find(int id)
        {
            throw new NotImplementedException();
        }

        public int Modify(AttemptTicketDomain entity)
        {
            throw new NotImplementedException();
        }

        public int Delete(AttemptTicketDomain entity)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            if (_context != null) { _context.Dispose(); }
        }

        public List<AttemptTicketDomain> ReadAll()
        {
            throw new NotImplementedException();
        }

        public List<AttemptTicketDomain> ReadAll(StatusFileDomain entity)
        {
            throw new NotImplementedException();
        }

        public List<AttemptTicketDomain> ReadAll(string id)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TicketAttempt, AttemptTicketDomain>(); });
            IMapper iMapper = config.CreateMapper();

            return _context.TicketAttempt.Where(p => p.NoTicket == id).Select(item => iMapper.Map<TicketAttempt, AttemptTicketDomain>(item)).ToList();

        }
    }
}
