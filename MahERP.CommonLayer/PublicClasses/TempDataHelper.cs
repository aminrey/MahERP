using MahERP.CommonLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace MahERP.CommonLayer.PublicClasses
{
    public static class TempDataHelper
    {
        public static void SetResponseMessages(this ITempDataDictionary tempData, List<WebResponseMessageViewModel> messages)
        {
            tempData["ResponseMessages"] = JsonSerializer.Serialize(messages);
        }
        
        public static List<WebResponseMessageViewModel> GetResponseMessages(this ITempDataDictionary tempData)
        {
            if (tempData["ResponseMessages"] != null)
            {
                try
                {
                    var jsonString = tempData["ResponseMessages"].ToString();
                    return JsonSerializer.Deserialize<List<WebResponseMessageViewModel>>(jsonString);
                }
                catch
                {
                    return new List<WebResponseMessageViewModel>();
                }
            }
            return new List<WebResponseMessageViewModel>();
        }
    }
}