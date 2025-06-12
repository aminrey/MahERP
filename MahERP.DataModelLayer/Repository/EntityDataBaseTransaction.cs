using MahERP.DataModelLayer.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MahERP.DataModelLayer.Repository
{
    class EntityDataBaseTransaction : IEntityDataBaseTransaction
    {

        private readonly IDbContextTransaction _transaction;

        public EntityDataBaseTransaction(AppDbContext context)
        {
            _transaction = context.Database.BeginTransaction();
        }

        public void Commit()
        {
            //وقتی همه دستورات با موفقیت انجام شد
            _transaction.Commit();
        }

        public void RollBack()
        {
            //وقتی خطایی پیش آمد
            _transaction.Rollback();
        }

        public void Dispose()
        {
            //برای از بین بردن دیتابیس
            _transaction.Dispose();
        }
    }

}
