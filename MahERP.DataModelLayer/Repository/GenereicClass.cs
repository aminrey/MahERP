using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MahERP.DataModelLayer.Repository
{

    public class GenereicClass<Tentity> where Tentity : class
    {

        private readonly AppDbContext _Context;
        private DbSet<Tentity> _table;

        public GenereicClass(AppDbContext context)
        {
            _Context = context;
            _table = context.Set<Tentity>();
        }

        public virtual void Create(Tentity entity)
        {
            _table.Add(entity);
        }

        public virtual void DeleteAll(Tentity tentity)
        {
           

        }

        public virtual void Update(Tentity entity)
        {
            _table.Attach(entity);
            _Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual Tentity GetById(object id)
        {
            return _table.Find(id);
        }

        public virtual void Delete(Tentity entity)
        {
            if (_Context.Entry(entity).State == EntityState.Detached)
            {
                _table.Attach(entity);
            }
            _table.Remove(entity);


        }

        public virtual void DeleteById(object id)
        {
            var entity = GetById(id);
            Delete(entity);
        }

        public virtual void DeleteByRange(Expression<Func<Tentity, bool>> whereVariable = null)
        {
            IQueryable<Tentity> query = _table;

            if (whereVariable != null)
            {
                query = query.Where(whereVariable);
            }
            _table.RemoveRange(query);
        }

        public virtual IEnumerable<Tentity> Get(Expression<Func<Tentity, bool>> whereVariable = null,
           Func<IQueryable<Tentity>, IOrderedQueryable<Tentity>> orderbyVariable = null,
            string joinString = "")
        {
            IQueryable<Tentity> query = _table;

            if (whereVariable != null)
            {
                query = query.Where(whereVariable);
            }
            if (orderbyVariable != null)
            {
                query = orderbyVariable(query);
            }
            if (joinString != "")
            {
                foreach (string item in joinString.Split(','))
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();




        }
    }

}
