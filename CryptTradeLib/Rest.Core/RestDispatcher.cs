using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Polly;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST API の HTTP リクエスト送信を担当する標準実装クラス。
/// リトライ制御、バリデーション、例外処理、レスポンスデシリアライズまでを統合的に処理する。
/// </summary>
public class RestDispatcher : IRestDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestDispatcher> _logger;
    private readonly IRetryPolicyProvider _retryPolicyProvider;

    /// <summary>
    /// RestDispatcher のコンストラクタ。
    /// 通信処理、ロギング、リトライ方針を構成要素として受け取る。
    /// </summary>
    /// <param name="httpClient">HTTP 通信の実体。</param>
    /// <param name="logger">ロギング機能。</param>
    /// <param name="retryPolicyProvider">リクエストに応じたリトライ方針を提供するプロバイダー。</param>
#pragma warning disable IDE0290 // プライマリ コンストラクターの使用
    public RestDispatcher(HttpClient httpClient, ILogger<RestDispatcher> logger, IRetryPolicyProvider retryPolicyProvider)
#pragma warning restore IDE0290 // プライマリ コンストラクターの使用
    {
        _httpClient = httpClient;
        _logger = logger;
        _retryPolicyProvider = retryPolicyProvider;
    }

    /// <summary>
    /// 非ジェネリックなリクエスト送信メソッド。
    /// レスポンスの本体は文字列としてそのまま保持され、主に検査やログ確認に用いる。
    /// </summary>
    /// <param name="request">送信するリクエスト。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    /// <returns>ステータス、レスポンス文字列、例外などを含む <see cref="CallResult"/>。</returns>
    public async Task<CallResult> SendAsync(Request request, CancellationToken cancellationToken = default)
    {
        var requestId = string.IsNullOrWhiteSpace(request.RequestId)
            ? Guid.NewGuid().ToString()
            : request.RequestId;

        using var cts = request.Timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (request.Timeout.HasValue)
            cts!.CancelAfter(request.Timeout.Value);

        var token = cts?.Token ?? cancellationToken;
        var timeStamp = new TimeStamp { SendTime = DateTimeOffset.UtcNow };

        try
        {
            using var message = new HttpRequestMessage(new HttpMethod(request.Method), request.RequestUri);

            // ヘッダー設定
            foreach (var header in request.Headers)
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // ボディ（任意）
            if (!string.IsNullOrEmpty(request.Body))
            {
                message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
            }

            _logger.LogInformation("Sending request [{RequestId}]: {Method} {Uri}", requestId, request.Method, request.RequestUri);

            // ポリシーに従って送信（リトライあり）
            var policy = _retryPolicyProvider.CreatePolicy(request);
            var response = await policy.ExecuteAsync(() => _httpClient.SendAsync(message, token));

            var responseString = await response.Content.ReadAsStringAsync(token);
            timeStamp.ReceiveTime = DateTimeOffset.UtcNow;

            _logger.LogInformation("Received response [{RequestId}]: {StatusCode}", requestId, response.StatusCode);

            return new CallResult(
                request,
                new Response
                {
                    HttpStatusCode = response.StatusCode,
                    HttpReasonPhrase = response.ReasonPhrase ?? string.Empty,
                    ResponseString = responseString
                },
                timeStamp
            );
        }
        catch (Exception ex)
        {
            timeStamp.ReceiveTime = DateTimeOffset.UtcNow;
            _logger.LogError(ex, "Request failed [{RequestId}] - {Message}", requestId, ex.Message);
            return CallResult.Failure(request, ex, timeStamp);
        }
    }


    public async Task<CallResult<T>> SendAsync<T>(
        Request request,
        IValidator<T>? validator = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var result = await SendAsync(request, cancellationToken);

        // 通信失敗時（ステータス異常 or 例外）
        if (!result.IsSuccess)
        {
            return CallResult<T>.Failure(result.Request, result.Exception!, result.TimeStamp, result.Response);
        }

        try
        {
            var model = JsonSerializer.Deserialize<T>(result.Response.ResponseString);
            if (model == null)
            {
                return CallResult<T>.DeserializationFailed(result.Request, result.TimeStamp, result.Response);
            }

            if (validator != null)
            {
                var validation = await validator.ValidateAsync(model, cancellationToken);
                if (!validation.IsValid)
                {
                    return CallResult<T>.ValidationFailed(
                        result.Request,
                        model,
                        result.TimeStamp,
                        result.Response,
                        validation.Errors.Select(e => e.ErrorMessage));
                }
            }

            return CallResult<T>.Success(result.Request, model, result.TimeStamp, result.Response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deserialization failed for response [{RequestId}]", request.RequestId);
            return CallResult<T>.Failure(result.Request, ex, result.TimeStamp, result.Response);
        }
    }
}
