using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;

namespace MenStyle.Web.Services;

public interface IPasswordResetOtpService
{
    Task<OtpSendResult> CreateOrResendAsync(
        string userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default);

    Task<OtpSendResult> ResendAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<OtpRequestInfo?> GetInfoAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<OtpVerifyResult> VerifyAsync(
        Guid requestId,
        string otpCode,
        CancellationToken cancellationToken = default);

    void InvalidateForUser(string userId);
}

public sealed record OtpSendResult(
    bool Succeeded,
    Guid RequestId,
    string Message,
    int RetryAfterSeconds = 0);

public sealed record OtpRequestInfo(
    Guid RequestId,
    string MaskedEmail,
    int RemainingSeconds,
    int RemainingAttempts,
    bool IsExpired);

public enum OtpVerifyStatus
{
    Succeeded,
    NotFound,
    Expired,
    InvalidCode,
    TooManyAttempts,
    AlreadyUsed
}

public sealed record OtpVerifyResult(
    OtpVerifyStatus Status,
    string UserId = "",
    int RemainingAttempts = 0);

public sealed class PasswordResetOtpService : IPasswordResetOtpService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan SendHistoryWindow = TimeSpan.FromHours(1);

    private const int MaxFailedAttempts = 5;
    private const int MaxSendsPerRequest = 5;
    private const int MaxSendsPerHour = 5;

    private readonly IEmailSender _emailSender;
    private readonly ILogger<PasswordResetOtpService> _logger;
    private readonly IPasswordHasher<OtpRequestState> _passwordHasher;

    private readonly ConcurrentDictionary<Guid, OtpRequestState> _requests = new();
    private readonly ConcurrentDictionary<string, Guid> _activeRequestsByUser = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTimeOffset>> _sendHistory = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _userGates = new();

    public PasswordResetOtpService(
        IEmailSender emailSender,
        ILogger<PasswordResetOtpService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
        _passwordHasher = new PasswordHasher<OtpRequestState>();
    }

    public async Task<OtpSendResult> CreateOrResendAsync(
        string userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        CleanupOldRequests();

        var userGate = _userGates.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await userGate.WaitAsync(cancellationToken);

        try
        {
            if (_activeRequestsByUser.TryGetValue(userId, out var activeRequestId)
                && _requests.TryGetValue(activeRequestId, out var activeRequest))
            {
                await activeRequest.Gate.WaitAsync(cancellationToken);

                try
                {
                    if (!activeRequest.IsVerified)
                    {
                        return await SendNewCodeLockedAsync(activeRequest, cancellationToken);
                    }
                }
                finally
                {
                    activeRequest.Gate.Release();
                }
            }

            var hourlyRetry = GetHourlyRetryAfterSeconds(userId);

            if (hourlyRetry > 0)
            {
                return new OtpSendResult(
                    false,
                    Guid.Empty,
                    "Bạn đã yêu cầu quá nhiều mã OTP. Vui lòng thử lại sau.",
                    hourlyRetry);
            }

            var request = new OtpRequestState
            {
                RequestId = Guid.NewGuid(),
                UserId = userId,
                Email = email.Trim(),
                DisplayName = displayName.Trim(),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var sendResult = await SendNewCodeLockedAsync(request, cancellationToken);

            if (!sendResult.Succeeded)
            {
                return sendResult with { RequestId = Guid.Empty };
            }

            _requests[request.RequestId] = request;
            _activeRequestsByUser[userId] = request.RequestId;

            return sendResult;
        }
        finally
        {
            userGate.Release();
        }
    }

    public async Task<OtpSendResult> ResendAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (!_requests.TryGetValue(requestId, out var request))
        {
            return new OtpSendResult(
                false,
                requestId,
                "Yêu cầu OTP không tồn tại hoặc đã hết hiệu lực.");
        }

        var userGate = _userGates.GetOrAdd(request.UserId, _ => new SemaphoreSlim(1, 1));
        await userGate.WaitAsync(cancellationToken);

        try
        {
            await request.Gate.WaitAsync(cancellationToken);

            try
            {
                if (request.IsVerified)
                {
                    return new OtpSendResult(
                        false,
                        requestId,
                        "Mã OTP này đã được xác minh.");
                }

                return await SendNewCodeLockedAsync(request, cancellationToken);
            }
            finally
            {
                request.Gate.Release();
            }
        }
        finally
        {
            userGate.Release();
        }
    }

    public async Task<OtpRequestInfo?> GetInfoAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (!_requests.TryGetValue(requestId, out var request))
        {
            return null;
        }

        await request.Gate.WaitAsync(cancellationToken);

        try
        {
            var now = DateTimeOffset.UtcNow;
            var remainingSeconds = Math.Max(
                0,
                (int)Math.Ceiling((request.ExpiresAtUtc - now).TotalSeconds));

            return new OtpRequestInfo(
                request.RequestId,
                MaskEmail(request.Email),
                remainingSeconds,
                Math.Max(0, MaxFailedAttempts - request.FailedAttempts),
                request.ExpiresAtUtc <= now);
        }
        finally
        {
            request.Gate.Release();
        }
    }

    public async Task<OtpVerifyResult> VerifyAsync(
        Guid requestId,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        if (!_requests.TryGetValue(requestId, out var request))
        {
            return new OtpVerifyResult(OtpVerifyStatus.NotFound);
        }

        await request.Gate.WaitAsync(cancellationToken);

        try
        {
            if (request.IsVerified)
            {
                return new OtpVerifyResult(OtpVerifyStatus.AlreadyUsed);
            }

            if (request.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                return new OtpVerifyResult(OtpVerifyStatus.Expired);
            }

            if (request.FailedAttempts >= MaxFailedAttempts)
            {
                return new OtpVerifyResult(OtpVerifyStatus.TooManyAttempts);
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                request,
                request.CodeHash,
                otpCode.Trim());

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                request.FailedAttempts++;
                var remainingAttempts = Math.Max(0, MaxFailedAttempts - request.FailedAttempts);

                return new OtpVerifyResult(
                    remainingAttempts == 0
                        ? OtpVerifyStatus.TooManyAttempts
                        : OtpVerifyStatus.InvalidCode,
                    RemainingAttempts: remainingAttempts);
            }

            request.IsVerified = true;
            request.VerifiedAtUtc = DateTimeOffset.UtcNow;

            if (_activeRequestsByUser.TryGetValue(request.UserId, out var activeRequestId)
                && activeRequestId == request.RequestId)
            {
                _activeRequestsByUser.TryRemove(request.UserId, out _);
            }

            return new OtpVerifyResult(
                OtpVerifyStatus.Succeeded,
                request.UserId,
                MaxFailedAttempts - request.FailedAttempts);
        }
        finally
        {
            request.Gate.Release();
        }
    }

    public void InvalidateForUser(string userId)
    {
        foreach (var item in _requests.Where(x => x.Value.UserId == userId).ToList())
        {
            _requests.TryRemove(item.Key, out _);
        }

        _activeRequestsByUser.TryRemove(userId, out _);
    }

    private async Task<OtpSendResult> SendNewCodeLockedAsync(
        OtpRequestState request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (request.SendCount >= MaxSendsPerRequest)
        {
            return new OtpSendResult(
                false,
                request.RequestId,
                "Bạn đã gửi lại OTP quá số lần cho phép. Vui lòng thử lại sau.");
        }

        if (request.LastSentAtUtc != default)
        {
            var cooldownRemaining = ResendCooldown - (now - request.LastSentAtUtc);

            if (cooldownRemaining > TimeSpan.Zero)
            {
                var retryAfterSeconds = (int)Math.Ceiling(cooldownRemaining.TotalSeconds);

                return new OtpSendResult(
                    false,
                    request.RequestId,
                    $"Vui lòng chờ {retryAfterSeconds} giây trước khi gửi lại OTP.",
                    retryAfterSeconds);
            }
        }

        var hourlyRetry = GetHourlyRetryAfterSeconds(request.UserId);

        if (hourlyRetry > 0)
        {
            return new OtpSendResult(
                false,
                request.RequestId,
                "Bạn đã yêu cầu quá nhiều mã OTP. Vui lòng thử lại sau.",
                hourlyRetry);
        }

        var otpCode = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        var candidateHash = _passwordHasher.HashPassword(request, otpCode);

        try
        {
            await _emailSender.SendHtmlAsync(
                request.Email,
                "Mã OTP đặt lại mật khẩu MENSTYLE",
                BuildOtpEmailBody(request.DisplayName, otpCode),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Không thể gửi email OTP đặt lại mật khẩu cho user {UserId}.",
                request.UserId);

            return new OtpSendResult(
                false,
                request.RequestId,
                "Hệ thống chưa thể gửi email OTP. Vui lòng thử lại hoặc kiểm tra cấu hình email.");
        }

        request.CodeHash = candidateHash;
        request.LastSentAtUtc = now;
        request.ExpiresAtUtc = now.Add(OtpLifetime);
        request.FailedAttempts = 0;
        request.SendCount++;

        RecordSuccessfulSend(request.UserId, now);

        return new OtpSendResult(
            true,
            request.RequestId,
            "Mã OTP đã được gửi tới email đăng ký của bạn.");
    }

    private int GetHourlyRetryAfterSeconds(string userId)
    {
        var now = DateTimeOffset.UtcNow;
        var queue = _sendHistory.GetOrAdd(
            userId,
            _ => new ConcurrentQueue<DateTimeOffset>());

        while (queue.TryPeek(out var sentAt)
               && now - sentAt >= SendHistoryWindow)
        {
            queue.TryDequeue(out _);
        }

        if (queue.Count < MaxSendsPerHour || !queue.TryPeek(out var oldestSend))
        {
            return 0;
        }

        return Math.Max(
            1,
            (int)Math.Ceiling((oldestSend.Add(SendHistoryWindow) - now).TotalSeconds));
    }

    private void RecordSuccessfulSend(string userId, DateTimeOffset sentAt)
    {
        var queue = _sendHistory.GetOrAdd(
            userId,
            _ => new ConcurrentQueue<DateTimeOffset>());

        queue.Enqueue(sentAt);
    }

    private void CleanupOldRequests()
    {
        var removeBefore = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(30));

        foreach (var item in _requests)
        {
            var request = item.Value;
            var referenceTime = request.VerifiedAtUtc ?? request.ExpiresAtUtc;

            if (referenceTime < removeBefore)
            {
                _requests.TryRemove(item.Key, out _);

                if (_activeRequestsByUser.TryGetValue(request.UserId, out var activeRequestId)
                    && activeRequestId == request.RequestId)
                {
                    _activeRequestsByUser.TryRemove(request.UserId, out _);
                }
            }
        }
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@', 2);

        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
        {
            return "email đã đăng ký";
        }

        var localPart = parts[0];
        var visiblePrefix = localPart.Length <= 2
            ? localPart[..1]
            : localPart[..2];

        return $"{visiblePrefix}***@{parts[1]}";
    }

    private static string BuildOtpEmailBody(string displayName, string otpCode)
    {
        var safeDisplayName = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(displayName) ? "bạn" : displayName);

        var safeOtpCode = WebUtility.HtmlEncode(otpCode);

        return $$"""
            <!DOCTYPE html>
            <html lang="vi">
            <body style="margin:0;background:#f6f4ef;font-family:Arial,sans-serif;color:#171717;">
              <div style="max-width:560px;margin:32px auto;background:#ffffff;border:1px solid #e8e1d7;border-radius:18px;overflow:hidden;">
                <div style="padding:24px 28px;background:#111318;color:#ffffff;">
                  <strong style="font-size:22px;letter-spacing:1px;">MENSTYLE</strong>
                </div>
                <div style="padding:30px 28px;">
                  <h1 style="margin:0 0 16px;font-size:24px;">Xác minh đặt lại mật khẩu</h1>
                  <p>Xin chào {{safeDisplayName}},</p>
                  <p>Mã OTP dùng để đặt lại mật khẩu tài khoản MENSTYLE của bạn là:</p>
                  <div style="margin:24px 0;padding:18px;text-align:center;background:#fff7e8;border:1px solid #d9ae67;border-radius:14px;font-size:34px;font-weight:800;letter-spacing:8px;">
                    {{safeOtpCode}}
                  </div>
                  <p>Mã có hiệu lực trong <strong>5 phút</strong> và chỉ được sử dụng một lần.</p>
                  <p style="color:#6f6f6f;">Nếu bạn không yêu cầu đổi mật khẩu, hãy bỏ qua email này và không cung cấp mã OTP cho bất kỳ ai.</p>
                </div>
              </div>
            </body>
            </html>
            """;
    }

    private sealed class OtpRequestState
    {
        public Guid RequestId { get; init; }

        public string UserId { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string CodeHash { get; set; } = string.Empty;

        public DateTimeOffset CreatedAtUtc { get; init; }

        public DateTimeOffset ExpiresAtUtc { get; set; }

        public DateTimeOffset LastSentAtUtc { get; set; }

        public DateTimeOffset? VerifiedAtUtc { get; set; }

        public int FailedAttempts { get; set; }

        public int SendCount { get; set; }

        public bool IsVerified { get; set; }

        public SemaphoreSlim Gate { get; } = new(1, 1);
    }
}
