using System.Net;
using JotaNunesForms.Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JotaNunesForms.Infrastructure.Tests;

public sealed class ResendHttpEmailSenderTests
{
    [Fact]
    public async Task SendAsync_posts_to_resend_api_with_bearer_token()
    {
        HttpRequestMessage? captured = null;
        var handler = new StubHandler(
            (request, _) =>
            {
                captured = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":\"abc\"}"),
                });
            });

        var factory = new StubHttpClientFactory(handler);
        var sender = new ResendHttpEmailSender(
            Options.Create(new EmailOptions
            {
                UseHttpApi = true,
                Password = "re_test_key",
                From = "portal@example.dev",
            }),
            factory,
            NullLogger<ResendHttpEmailSender>.Instance);

        await sender.SendAsync(
            "user@example.com",
            "Assunto",
            "texto",
            "<p>html</p>");

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Equal("https://api.resend.com/emails", captured.RequestUri?.ToString());
        Assert.Equal("Bearer", captured.Headers.Authorization?.Scheme);
        Assert.Equal("re_test_key", captured.Headers.Authorization?.Parameter);
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

        public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) =>
            _send = send;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            _send(request, cancellationToken);
    }
}
