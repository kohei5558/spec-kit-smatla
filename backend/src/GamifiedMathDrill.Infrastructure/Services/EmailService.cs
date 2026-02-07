using System.Net;
using System.Net.Mail;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// メール送信サービス実装（SMTP）
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        try
        {
            // 開発環境ではメール送信をスキップ（ログのみ）
            var enableSending = _configuration.GetValue<bool>("Email:EnableSending", false);
            if (!enableSending)
            {
                _logger.LogInformation("Email sending disabled. Password reset link for {Email}: {Link}", toEmail, resetLink);
                return true;
            }

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:FromAddress"];

            using var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress ?? "noreply@gamifiedmathdrill.com"),
                Subject = "パスワードリセットのご案内",
                Body = $@"
                    <html>
                    <body style='font-family: sans-serif;'>
                        <h2>パスワードリセットのご案内</h2>
                        <p>パスワードリセットのリクエストを受け付けました。</p>
                        <p>以下のリンクをクリックして、新しいパスワードを設定してください：</p>
                        <p><a href='{resetLink}' style='display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px;'>パスワードをリセット</a></p>
                        <p>または、以下のURLをコピーしてブラウザに貼り付けてください：</p>
                        <p style='word-break: break-all;'>{resetLink}</p>
                        <p>このリンクは24時間有効です。</p>
                        <p>このメールに心当たりがない場合は、無視してください。</p>
                        <hr>
                        <p style='color: #666; font-size: 12px;'>このメールは自動送信されています。返信しないでください。</p>
                    </body>
                    </html>
                ",
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string displayName)
    {
        try
        {
            // 開発環境ではメール送信をスキップ（ログのみ）
            var enableSending = _configuration.GetValue<bool>("Email:EnableSending", false);
            if (!enableSending)
            {
                _logger.LogInformation("Email sending disabled. Welcome email would be sent to {Email}", toEmail);
                return true;
            }

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:FromAddress"];

            using var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress ?? "noreply@gamifiedmathdrill.com"),
                Subject = "ようこそ！アカウントが作成されました",
                Body = $@"
                    <html>
                    <body style='font-family: sans-serif;'>
                        <h2>ようこそ、{displayName}様！</h2>
                        <p>Gamified Math Drillへようこそ！</p>
                        <p>アカウントが正常に作成されました。</p>
                        <p>お子様の算数学習をサポートする準備が整いました。</p>
                        <p>早速ログインして、お子様のプロフィールを作成しましょう！</p>
                        <hr>
                        <p style='color: #666; font-size: 12px;'>このメールは自動送信されています。返信しないでください。</p>
                    </body>
                    </html>
                ",
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Welcome email sent to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
            return false;
        }
    }
}
