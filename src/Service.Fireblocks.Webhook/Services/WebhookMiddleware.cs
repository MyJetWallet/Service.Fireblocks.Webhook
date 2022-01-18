using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyJetWallet.Fireblocks.Client;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.Blockchain.Wallets.Grpc;
using Service.Blockchain.Wallets.MyNoSql.AssetsMappings;
using Service.Fireblocks.Webhook.Domain.Models;
using Service.Fireblocks.Webhook.Domain.Models.Deposits;
using Service.Fireblocks.Webhook.Events;
using Service.Fireblocks.Webhook.ServiceBus;
using Service.Fireblocks.Webhook.ServiceBus.Deposits;

// ReSharper disable InconsistentLogPropertyNaming
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable UnusedMember.Global

namespace Service.Fireblocks.Webhook.Services
{
    public class WebhookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebhookMiddleware> _logger;
        private readonly IServiceBusPublisher<WebhookQueueItem> _serviceBusPublisher;


        /// <summary>
        /// Middleware that handles all unhandled exceptions and logs them as errors.
        /// </summary>
        public WebhookMiddleware(
            RequestDelegate next,
            ILogger<WebhookMiddleware> logger,
            IServiceBusPublisher<WebhookQueueItem> serviceBusPublisher)
        {
            _next = next;
            _logger = logger;
            _serviceBusPublisher = serviceBusPublisher;

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

            using var activity = MyTelemetry.StartActivity("Receive fireblocks webhook");

            var path = context.Request.Path;
            var method = context.Request.Method;

            var body = "--none--";
            var query = context.Request.QueryString;
            var signature = context.Request.Headers["Fireblocks-Signature"].FirstOrDefault();
            byte[] bodyArray;

            if (method != "POST")
            {
                _logger.LogInformation($"'{path}' | {query} | {method}\nNO-BODY\n{signature}");
                context.Response.StatusCode = 200;
                _logger.LogWarning("Message from Fireblocks: @{context} method is not POST", body);
                return;
            }

            await using var buffer = new MemoryStream();

            await context.Request.Body.CopyToAsync(buffer);

            buffer.Position = 0L;
            using var reader = new StreamReader(buffer);
            body = await reader.ReadToEndAsync();
            buffer.Position = 0L;
            bodyArray = buffer.GetBuffer();
            var bAStr = Convert.ToBase64String(bodyArray);

            _logger.LogInformation($"'{path}' | {query} | {method}\n{body}\n{signature}");

            //Fireblocks - Signature = Base64(RSA512(WEBHOOK_PRIVATE_KEY, SHA512(eventBody)))

            if (!CryptoProvider.VerifySignature(bodyArray, Convert.FromBase64String(signature)))
            {
                context.Response.StatusCode = 401;
                _logger.LogWarning("Message from Fireblocks: {context} webhook can't be verified", new { 
                    Body = body,
                    Signature = signature, 
                    BodyBase64 = bAStr });

                return;
            } else
            {
                _logger.LogInformation("Body Array: {context} webhook is verified", new { 
                    Body = body, 
                    Signature = signature, 
                    BodyBase64 = bAStr });
            }

            foreach (var header in context.Request.Headers)
            {
                activity.AddTag(header.Key, header.Value);
            }

            activity.AddTag("Body", body);

            _logger.LogInformation("Message from Fireblocks: @{context}", body);

            var webhook = Newtonsoft.Json.JsonConvert.DeserializeObject<WebhookBase>(body);

            if (webhook == null)
            {
                context.Response.StatusCode = 400;
                _logger.LogWarning("Message from Fireblocks: @{context} can't be parsed", body);

                return;
            }

            switch (webhook.Type)
            {
                case WebhookType.TRANSACTION_CREATED:
                case WebhookType.TRANSACTION_APPROVAL_STATUS_UPDATED:
                case WebhookType.TRANSACTION_STATUS_UPDATED:
                    {
                        await _serviceBusPublisher.PublishAsync(new WebhookQueueItem
                        {
                            Data = body,
                            Type = webhook.Type,
                        });
                        break;
                    }

                case WebhookType.VAULT_ACCOUNT_ADDED:
                case WebhookType.NETWORK_CONNECTION_ADDED:
                case WebhookType.EXCHANGE_ACCOUNT_ADDED:
                case WebhookType.FIAT_ACCOUNT_ADDED:
                case WebhookType.VAULT_ACCOUNT_ASSET_ADDED:
                case WebhookType.INTERNAL_WALLET_ASSET_ADDED:
                case WebhookType.EXTERNAL_WALLET_ASSET_ADDED:
                    {
                        //SKIP... FOR NOW!
                        break;
                    }

                default:
                    {
                        context.Response.StatusCode = 400;
                        _logger.LogWarning("Message from Fireblocks: @{context} webhook can't be reckognised", body);

                        return;
                    }
            }

            context.Response.StatusCode = 200;
        }
    }
}