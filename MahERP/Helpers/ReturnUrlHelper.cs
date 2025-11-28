using Microsoft.AspNetCore.Mvc;

namespace MahERP.Helpers
{
    /// <summary>
    /// Helper برای مدیریت Return URL در کنترلرها
    /// </summary>
    public static class ReturnUrlHelper
    {
        /// <summary>
        /// ساخت URL بازگشت امن و معتبر
        /// </summary>
        /// <param name="controller">کنترلر فعلی</param>
        /// <param name="returnUrl">URL دریافتی از کاربر</param>
        /// <param name="defaultAction">Action پیش‌فرض</param>
        /// <param name="defaultController">Controller پیش‌فرض</param>
        /// <param name="defaultArea">Area پیش‌فرض</param>
        /// <returns>URL معتبر بازگشت</returns>
        public static string GetSafeReturnUrl(
            this Controller controller,
            string? returnUrl,
            string defaultAction = "Index",
            string? defaultController = null,
            string? defaultArea = null)
        {
            // اگر returnUrl معتبر نیست، از URL پیش‌فرض استفاده کن
            if (string.IsNullOrWhiteSpace(returnUrl) || !IsLocalUrl(returnUrl))
            {
                return controller.Url.Action(
                    defaultAction,
                    defaultController ?? controller.ControllerContext.ActionDescriptor.ControllerName,
                    new { area = defaultArea }) ?? "/";
            }

            return returnUrl;
        }

        /// <summary>
        /// بررسی اینکه URL محلی است و امن است
        /// </summary>
        private static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // باید با / یا ~/ شروع شود
            if (!url.StartsWith("/") && !url.StartsWith("~/"))
                return false;

            // نباید // داشته باشد (پیشگیری از Open Redirect)
            if (url.StartsWith("//") || url.StartsWith("/\\"))
                return false;

            return true;
        }

        /// <summary>
        /// ساخت JSON Response با Return URL
        /// </summary>
        public static object CreateReturnUrlResponse(
            this Controller controller,
            bool success,
            string message,
            string? returnUrl = null,
            string defaultAction = "Index",
            string? defaultController = null,
            string? defaultArea = null)
        {
            var finalReturnUrl = controller.GetSafeReturnUrl(
                returnUrl,
                defaultAction,
                defaultController,
                defaultArea);

            return new
            {
                status = success ? "redirect" : "error",
                redirectUrl = success ? finalReturnUrl : null,
                message = new[] { new { status = success ? "success" : "error", text = message } }
            };
        }

        /// <summary>
        /// افزودن ReturnUrl به RouteValues
        /// </summary>
        public static object AddReturnUrl(
            this object routeValues,
            string? returnUrl,
            string? sourcePage = null)
        {
            var dict = new Dictionary<string, object?>();

            // کپی کردن مقادیر قبلی
            if (routeValues != null)
            {
                foreach (var prop in routeValues.GetType().GetProperties())
                {
                    dict[prop.Name] = prop.GetValue(routeValues);
                }
            }

            // اضافه کردن ReturnUrl
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                dict["returnUrl"] = returnUrl;
            }

            if (!string.IsNullOrWhiteSpace(sourcePage))
            {
                dict["sourcePage"] = sourcePage;
            }

            return dict;
        }

        /// <summary>
        /// دریافت Return URL از Request
        /// </summary>
        public static string? GetReturnUrlFromRequest(this Controller controller)
        {
            // بررسی Query String
            if (controller.Request.Query.ContainsKey("returnUrl"))
            {
                return controller.Request.Query["returnUrl"].ToString();
            }

            // بررسی Form Data
            if (controller.Request.HasFormContentType &&
                controller.Request.Form.ContainsKey("returnUrl"))
            {
                return controller.Request.Form["returnUrl"].ToString();
            }

            // بررسی Referer
            var referer = controller.Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer) && IsLocalUrl(referer))
            {
                return referer;
            }

            return null;
        }

        /// <summary>
        /// دریافت Source Page از Request
        /// </summary>
        public static string? GetSourcePageFromRequest(this Controller controller)
        {
            if (controller.Request.Query.ContainsKey("sourcePage"))
            {
                return controller.Request.Query["sourcePage"].ToString();
            }

            if (controller.Request.HasFormContentType &&
                controller.Request.Form.ContainsKey("sourcePage"))
            {
                return controller.Request.Form["sourcePage"].ToString();
            }

            return null;
        }
    }
}
