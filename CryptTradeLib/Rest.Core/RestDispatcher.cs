using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Polly;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST API �� HTTP ���N�G�X�g���M��S������W�������N���X�B
/// ���g���C����A�o���f�[�V�����A��O�����A���X�|���X�f�V���A���C�Y�܂ł𓝍��I�ɏ�������B
/// </summary>
public class RestDispatcher : IRestDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestDispatcher> _logger;
    private readonly IRetryPolicyProvider _retryPolicyProvider;

    /// <summary>
    /// RestDispatcher �̃R���X�g���N�^�B
    /// �ʐM�����A���M���O�A���g���C���j���\���v�f�Ƃ��Ď󂯎��B
    /// </summary>
    /// <param name="httpClient">HTTP �ʐM�̎��́B</param>
    /// <param name="logger">���M���O�@�\�B</param>
    /// <param name="retryPolicyProvider">���N�G�X�g�ɉ��������g���C���j��񋟂���v���o�C�_�[�B</param>
#pragma warning disable IDE0290 // �v���C�}�� �R���X�g���N�^�[�̎g�p
    public RestDispatcher(HttpClient httpClient, ILogger<RestDispatcher> logger, IRetryPolicyProvider retryPolicyProvider)
#pragma warning restore IDE0290 // �v���C�}�� �R���X�g���N�^�[�̎g�p
    {
        _httpClient = httpClient;
        _logger = logger;
        _retryPolicyProvider = retryPolicyProvider;
    }

    /// <summary>
    /// ��W�F�l���b�N�ȃ��N�G�X�g���M���\�b�h�B
    /// ���X�|���X�̖{�͕̂�����Ƃ��Ă��̂܂ܕێ�����A��Ɍ����⃍�O�m�F�ɗp����B
    /// </summary>
    /// <param name="request">���M���郊�N�G�X�g�B</param>
    /// <param name="cancellationToken">�L�����Z���p�g�[�N���B</param>
    /// <returns>�X�e�[�^�X�A���X�|���X������A��O�Ȃǂ��܂� <see cref="CallResult"/>�B</returns>
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

            // �w�b�_�[�ݒ�
            foreach (var header in request.Headers)
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // �{�f�B�i�C�Ӂj
            if (!string.IsNullOrEmpty(request.Body))
            {
                message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
            }

            _logger.LogInformation("Sending request [{RequestId}]: {Method} {Uri}", requestId, request.Method, request.RequestUri);

            // �|���V�[�ɏ]���đ��M�i���g���C����j
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

        // �ʐM���s���i�X�e�[�^�X�ُ� or ��O�j
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
