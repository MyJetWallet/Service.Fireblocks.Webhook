using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using Newtonsoft.Json;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.Circle.Signer.Grpc;
using Service.Circle.Signer.Grpc.Models;

// ReSharper disable InconsistentLogPropertyNaming
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable UnusedMember.Global

namespace Service.Fireblocks.Webhook.Services
{
    public class WebhookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebhookMiddleware> _logger;

        /// <summary>
        /// Middleware that handles all unhandled exceptions and logs them as errors.
        /// </summary>
        public WebhookMiddleware(
            RequestDelegate next,
            ILogger<WebhookMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/fireblocks", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Receive call to {path}, method: {method}", context.Request.Path,
                    context.Request.Method);

                return;
            }

            var path = context.Request.Path;
            var method = context.Request.Method;

            var body = "--none--";

            if (method == "POST")
            {
                await using var buffer = new MemoryStream();

                await context.Request.Body.CopyToAsync(buffer);

                buffer.Position = 0L;

                using var reader = new StreamReader(buffer);

                body = await reader.ReadToEndAsync();
            }

            var query = context.Request.QueryString;

            _logger.LogInformation($"'{path}' | {query} | {method}\n{body}");


            if (method == "POST")
            {
                using var activity = MyTelemetry.StartActivity("Receive fireblocks webhook");

                foreach (var header in context.Request.Headers)
                {
                    activity.AddTag(header.Key, header.Value);
                }

                activity.AddTag("Body", body);

                _logger.LogInformation("Message from Fireblocks: {message}", body);
            }

            context.Response.StatusCode = 200;
        }
    }
}