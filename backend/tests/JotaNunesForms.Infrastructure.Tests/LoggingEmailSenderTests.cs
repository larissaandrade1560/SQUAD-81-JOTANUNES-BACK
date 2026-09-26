using JotaNunesForms.Infrastructure.Email;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace JotaNunesForms.Infrastructure.Tests;

public sealed class LoggingEmailSenderTests
{
    [Fact]
    public async Task SendAsync_InDevelopment_LogsInviteLinkInTextBody()
    {
        var sender = new LoggingEmailSender(
            NullLogger<LoggingEmailSender>.Instance,
            new FakeHostEnvironment { EnvironmentName = Environments.Development });

        await sender.SendAsync(
            "to@example.com",
            "subject",
            "text with http://localhost/definir-senha?token=secret",
            "<p>html</p>");
    }

    [Fact]
    public async Task SendAsync_OutsideDevelopment_DoesNotRequireTokenInImplementation()
    {
        var sender = new LoggingEmailSender(
            NullLogger<LoggingEmailSender>.Instance,
            new FakeHostEnvironment { EnvironmentName = Environments.Production });

        await sender.SendAsync("to@example.com", "subject", "body", null);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = ".";

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
