using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace MahERP.DataModelLayer.Services
{
    /// <summary>
    /// مدیریت تراکنش‌ها برای عملیات پیچیده
    /// </summary>
    public class TransactionManager : IDisposable
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        public TransactionManager(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// شروع تراکنش
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("تراکنش قبلاً شروع شده است");

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// تأیید تراکنش
        /// </summary>
        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("هیچ تراکنش فعالی وجود ندارد");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// لغو تراکنش
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// پاکسازی منابع
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}