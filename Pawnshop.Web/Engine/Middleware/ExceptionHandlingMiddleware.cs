using System;
using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Serilog;

namespace Pawnshop.Web.Engine.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionHandlingMiddleware(ILogger logger, RequestDelegate next)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (PawnshopApplicationException ex)
            {
                _logger.Error(ex, ex.Message);
                await HandleApplicationExceptionAsync(context, ex);
            }
            catch (DbException ex)
            {
                _logger.Error(ex, ex.Message);
                await HandleDbExceptionAsync(context, ex);
            }
            catch (NotImplementedException ex)
            {
                _logger.Error(ex, ex.Message);
                await HandleNotImplementedExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleApplicationExceptionAsync(HttpContext context, PawnshopApplicationException exception)
        {
            await WriteExceptionAsync(context, HttpStatusCode.Locked,
                exception.GetType().Name, exception.Messages).ConfigureAwait(false);
        }
        private async Task HandleNotImplementedExceptionAsync(HttpContext context, NotImplementedException exception)
        {
            await WriteExceptionAsync(context, HttpStatusCode.NotImplemented,
                "Функционал не доступен", exception.Message).ConfigureAwait(false);
        }

        private async Task HandleDbExceptionAsync(HttpContext context, DbException exception)
        {
            await WriteExceptionAsync(context, HttpStatusCode.InsufficientStorage,
                "Ошибка в чтения/записи данных", exception.Message).ConfigureAwait(false);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            await WriteExceptionAsync(context, HttpStatusCode.InternalServerError,
                exception.GetType().Name, exception.Message).ConfigureAwait(false);
        }

        private async Task WriteExceptionAsync(HttpContext context, HttpStatusCode code, string type, params string[] messages)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int) code;
            await response.WriteAsync(JsonConvert.SerializeObject(new
            {
                messages,
                type,
            })).ConfigureAwait(false);
        }
    }
}