using Service.Repositories;
using Service.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TicketDataAccess.InternalDB;

namespace Service.DBRepositories
{
    public class InBoundFileRepository : IInBoundFileRepository
    {
        private readonly TicketsEntities _context;

        public InBoundFileRepository(TicketsEntities context)
        {
            _context = context;
        }


        public void Add(InBoundFileDomain entity)
        {
            _context.InBoundFile.Add(new InBoundFile { FileName = entity.FileName, FullPath = entity.FullPath, DateReg = entity.DateReg, TimeReg = entity.TimeReg, DateCreated = entity.DateCreated });
        }

        public void AddRange(IEnumerable<InBoundFileDomain> entities)
        {
            var range = entities.ToList().Select(r => new InBoundFile { FileName=r.FileName, FullPath = r.FullPath, DateReg = r.DateReg, TimeReg = r.TimeReg, DateCreated = r.DateCreated });
            _context.InBoundFile.AddRange(range);

        }

        public IEnumerable<InBoundFileDomain> Find(Expression<Func<InBoundFileDomain, bool>> predicate)
        {

           // var r = _context.InBoundFile.Where(predicate);

           // return _context.InBoundFile.Where(predicate);

            throw new NotImplementedException();
        }


        public IEnumerable<InBoundFileDomain> GetAll()
        {
            
            return _context.InBoundFile.ToList().Select(r => new InBoundFileDomain { FileName = r.FileName, 
                                                                                     FullPath = r.FullPath,
                                                                                     Id = r.Id,
                                                                                     DateReg = r.DateReg,
                                                                                     TimeReg = r.TimeReg,
                                                                                     DateCreated = r.DateCreated,
                                                                                     Processed = r.Processed
                                                                                   }  
                                                        );

        }

        public InBoundFileDomain GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<InBoundFileDomain> getNotProcessed()
        {
            return _context.InBoundFile.Where(p => p.Processed == 0).ToList().Select(r => new InBoundFileDomain
                                                                                        {
                                                                                            FileName = r.FileName,
                                                                                            FullPath = r.FullPath,
                                                                                            Id = r.Id,
                                                                                            DateReg = r.DateReg,
                                                                                            TimeReg = r.TimeReg,
                                                                                            DateCreated = r.DateCreated,
                                                                                            Processed = r.Processed
                                                                                        });
        }

        public void Update(InBoundFileDomain entity)
        {
            var inBoundFile = _context.InBoundFile.Where(P => P.Id == entity.Id).FirstOrDefault();

            inBoundFile.FileName = entity.FileName;
            inBoundFile.FullPath = entity.FullPath;
            inBoundFile.Processed = entity.Processed;
            inBoundFile.DateProcessed = entity.DateProcessed;
        }

        public void UpdateRange(IEnumerable<InBoundFileDomain> entities)
        {

            entities.ToList().ForEach((item) => {

                var inBoundFile = _context.InBoundFile.Where(P => P.Id == item.Id).FirstOrDefault();

                inBoundFile.FileName = item.FileName;
                inBoundFile.FullPath = item.FullPath;
                inBoundFile.Processed = item.Processed;
                inBoundFile.DateProcessed = item.DateProcessed;

            } );

        }

        public void Remove(InBoundFileDomain entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<InBoundFileDomain> entities)
        {
            throw new NotImplementedException();
        }

        public InBoundFileDomain getByFullName(string fullName)
        {
            return _context.InBoundFile.Where(p => p.FullPath.Equals(fullName)).Select(r => new InBoundFileDomain
            {
                Id = r.Id,
                FileName = r.FileName,
                FullPath = r.FullPath,
                DateCreated = r.DateCreated,
                DateReg = r.DateReg,
                TimeReg = r.TimeReg
            }).FirstOrDefault();
        }

        public bool FileProcessed(string fullName)
        {
            var result = getByFullName(fullName);

            return (result != null  && result.Processed==0 );
        }
    }
}
