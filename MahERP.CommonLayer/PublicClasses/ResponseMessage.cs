using MahERP.CommonLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahERP.CommonLayer.PublicClasses
{
    public static class ResponseMessage
    {
        public static List<WebResponseMessageViewModel> CreateSuccessResponse(params string[] messages)
        {
            List<WebResponseMessageViewModel> responseList = new List<WebResponseMessageViewModel>();

            foreach (var message in messages)
            {
                WebResponseMessageViewModel ResponseMessage = new()
                {
                    status = "success",
                    text = message
                };
                responseList.Add(ResponseMessage);

            }
            // Construct the response object


            return responseList;
        }

        public static List<WebResponseMessageViewModel> CreateWarningResponse(params string[] messages)
        {
            List<WebResponseMessageViewModel> responseList = new List<WebResponseMessageViewModel>();

            foreach (var message in messages)
            {
                WebResponseMessageViewModel ResponseMessage = new()
                {
                    status = "warning",
                    text = message
                };
                responseList.Add(ResponseMessage);

            }
            // Construct the response object


            return responseList;
        }
          public static List<WebResponseMessageViewModel> CreateInfoResponse( params string[] messages)
        {
            List<WebResponseMessageViewModel> responseList = new List<WebResponseMessageViewModel>();

            foreach (var message in messages)
            {
                WebResponseMessageViewModel ResponseMessage = new()
                {
                    status = "info",
                    text = message
                };
                responseList.Add(ResponseMessage);

            }
            // Construct the response object


            return responseList;
        }

          public static List<WebResponseMessageViewModel> CreateErrorResponse( params string[] messages)
        {
            List<WebResponseMessageViewModel> responseList = new List<WebResponseMessageViewModel>();

            foreach (var message in messages)
            {
                WebResponseMessageViewModel ResponseMessage = new()
                {
                    status = "error",
                    text = message
                };
                responseList.Add(ResponseMessage);

            }
            // Construct the response object


            return responseList;
        }



    }
}
