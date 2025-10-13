using System.Collections.Generic;
using System.Threading.Tasks;
using MahERP.DataModelLayer.Entities.Sms;
using MahERP.DataModelLayer.Services.SmsProviders;

namespace MahERP.DataModelLayer.Repository
{
    public interface ISmsProviderRepository
    {
        // ========== مدیریت Providers ==========
        
        List<SmsProvider> GetAllProviders();
        List<SmsProvider> GetActiveProviders();
        SmsProvider GetDefaultProvider();
        SmsProvider GetProviderByCode(string providerCode);
        int CreateProvider(SmsProvider provider);
        void UpdateProvider(SmsProvider provider);
        void DeleteProvider(int providerId);
        void SetAsDefaultProvider(int providerId);

        // ========== کار با Instance واقعی ==========
        
        ISmsProvider CreateProviderInstance(SmsProvider providerEntity);
        ISmsProvider GetDefaultProviderInstance();

        // ========== بروزرسانی اعتبار ==========
        
        Task UpdateProviderCredit(int providerId);
        Task UpdateAllActiveProvidersCredit();
    }
}