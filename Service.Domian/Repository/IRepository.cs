using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Repository
{
    interface IRepository<T,E>
    {
        int Create(T entity);
        int Modify(T entity);

        int Delete(T entity);

        List<T> ReadAll();

        List<T> ReadAll(E entity);

        List<T> ReadAll(string id);

        T Find(string id);

        T Find(int id);

    }
}
