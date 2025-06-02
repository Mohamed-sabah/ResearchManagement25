//// إنشاء ملف جديد: Infrastructure/Middleware/GlobalExceptionMiddleware.cs
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using System.Net.Http;
//using System.Text.Json;

//namespace ResearchManagement.Infrastructure.Middleware
//{
//    public class GlobalExceptionMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<GlobalExceptionMiddleware> _logger;

//        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            try
//            {
//                await _next(context);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ غير متوقع حدث في الطلب: {RequestPath}", context.Request.Path);
//                await HandleExceptionAsync(context, ex);
//            }
//        }

//        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
//        {
//            context.Response.ContentType = "application/json";

//            var response = new
//            {
//                message = "حدث خطأ في النظام",
//                details = exception.Message,
//                timestamp = DateTime.UtcNow
//            };

//            context.Response.StatusCode = exception switch
//            {
//                ArgumentNullException => StatusCodes.Status400BadRequest,
//                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
//                FileNotFoundException => StatusCodes.Status404NotFound,
//                InvalidOperationException => StatusCodes.Status400BadRequest,
//                _ => StatusCodes.Status500InternalServerError
//            };

//            // للتطوير فقط - إظهار تفاصيل أكثر
//            //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
//            //{
//            //    response = new
//            //    {
//            //        message = "حدث خطأ في النظام",
//            //        details = exception.Message,
//            //        stackTrace = exception.StackTrace,
//            //        timestamp = DateTime.UtcNow
//            //    };
//            //}

//            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
//            {
//                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                WriteIndented = true
//            });

//            await context.Response.WriteAsync(jsonResponse);
//        }
//    }

//    // Extension method for easy registration
//    public static class GlobalExceptionMiddlewareExtensions
//    {
//        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
//        {
//            return builder.UseMiddleware<GlobalExceptionMiddleware>();
//        }
//    }
//}

//// تحديث Program.cs لإضافة الـ middleware
//// أضف هذا السطر بعد var app = builder.Build();
//// app.UseGlobalExceptionMiddleware();