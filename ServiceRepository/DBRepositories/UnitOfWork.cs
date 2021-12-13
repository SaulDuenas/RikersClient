using Service.Repositories;
using Service.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketDataAccess.InternalDB;

namespace Service.DBRepositories
{
    public class UnitOfWork : IUnitofWork
    {
        private readonly TicketsEntities _context;

        public UnitOfWork() {

            this._context = new TicketsEntities();
            this.InboundFileRepository = new InBoundFileRepository(this._context);
        }

        public IInBoundFileRepository InboundFileRepository { get; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
