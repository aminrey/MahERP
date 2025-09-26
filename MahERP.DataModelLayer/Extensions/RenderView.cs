using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MahERP.Extentions
{
    public static class ViewRendererExtensions
    {
        /// <summary>
        /// Renders a view to string
        /// </summary>
        /// <param name="controller">The controller instance</param>
        /// <param name="viewName">The view name to render</param>
        /// <param name="model">The model for the view</param>
        /// <param name="viewBag">Optional viewBag data</param>
        /// <param name="appendMode">If true, the content will be appended instead of replaced</param>
        /// <returns>The rendered view as a string</returns>
        public static async Task<string> RenderViewToStringAsync<TModel>(this Controller controller, string viewName, TModel model, dynamic viewBag = null, bool appendMode = false)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;

            using var writer = new StringWriter();
            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"Could not find view '{viewName}'");
            }

            // Create a ViewData with the model
            var viewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary()
            )
            {
                Model = model
            };

            // اضافه کردن اطلاعات append mode به ViewData
            viewData["AppendMode"] = appendMode;

            // If viewbag is provided, add its properties to ViewData
            if (viewBag != null)
            {
                foreach (var property in viewBag.GetType().GetProperties())
                {
                    viewData[property.Name] = property.GetValue(viewBag);
                }
            }

            var tempDataProvider = controller.HttpContext.RequestServices.GetRequiredService<ITempDataProvider>();
            var tempData = new TempDataDictionary(controller.HttpContext, tempDataProvider);

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.ToString();
        }
    }
}
