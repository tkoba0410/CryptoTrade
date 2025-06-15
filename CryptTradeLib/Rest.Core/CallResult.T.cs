using System.Net;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// �^�t�� REST API �ʐM�̌��ʂ�\���N���X�B
/// �ʐM�X�e�[�^�X�A�f�V���A���C�Y���ʁA��O�A�o���f�[�V�������Ȃǂ𓝍����ĕێ�����B
/// </summary>
public class CallResult<T> where T : class
{
    /// <summary>
    /// ���ۂɑ��M���ꂽ���N�G�X�g�I�u�W�F�N�g�B
    /// �G���h�|�C���g�A�w�b�_�[�A���\�b�h�Ȃǂ��܂ށB
    /// </summary>
    public Request Request { get; }

    /// <summary>
    /// ���X�|���X�Ɋւ��� HTTP �X�e�[�^�X��{�f�B���B
    /// �ʐM�����^���s���킸�K���ێ������B
    /// </summary>
    public Response Response { get; }

    /// <summary>
    /// �ʐM�̑���M������o�ߎ��Ԃ��������B
    /// �p�t�H�[�}���X���͂⃍�M���O�ɗ��p�\�B
    /// </summary>
    public TimeStamp TimeStamp { get; }

    /// <summary>
    /// �ʐM���ɔ���������O�i�^�C���A�E�g�A�ڑ��G���[�Ȃǁj�B
    /// �������� null�B
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// ���X�|���X�{�f�B���f�V���A���C�Y���ē���ꂽ�f�[�^�B
    /// JSON �p�[�X���s�����O���ɂ� null�B
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// �f�[�^�ɑ΂��Ď��s���ꂽ�o���f�[�V�����G���[�̈ꗗ�B
    /// �������͋�B
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// �ʐM�ƃf�V���A���C�Y���������A���o���f�[�V�����G���[���Ȃ��ꍇ�� true�B
    /// </summary>
    public bool IsSuccess =>
        Exception == null && ValidationErrors.Count == 0 && Data != null;

    /// <summary>
    /// ���[�g�����iHTTP 429�j�ɂ�鎸�s���ǂ����B
    /// </summary>
    public bool IsRateLimited =>
        Response.HttpStatusCode == HttpStatusCode.TooManyRequests;

    /// <summary>
    /// �T�[�o�[�G���[�iHTTP 5xx�j���ǂ����B���g���C�̔��f�ޗ��ƂȂ�B
    /// </summary>
    public bool IsServerError =>
        (int)Response.HttpStatusCode is >= 500 and < 600;

    /// <summary>
    /// �N���C�A���g�G���[�iHTTP 4xx�j���ǂ����B
    /// �p�����[�^�~�X��F�؃G���[�Ȃǂ̓���Ɏg����B
    /// </summary>
    public bool IsClientError =>
        (int)Response.HttpStatusCode is >= 400 and < 500;

    /// <summary>
    /// �ʐM�͐��������� JSON �Ȃǂ̃f�V���A���C�Y�Ɏ��s�����ꍇ�� true�B
    /// </summary>
    public bool IsDeserializationFailed =>
        Exception == null && Data == null && !string.IsNullOrWhiteSpace(Response.ResponseString);

    /// <summary>
    /// �ʐM����уf�V���A���C�Y�����������ꍇ�̓����p�R���X�g���N�^�B
    /// </summary>
    internal CallResult(Request request, Response response, TimeStamp timeStamp, T? data)
    {
        Request = request;
        Response = response;
        TimeStamp = timeStamp;
        Data = data;
        ValidationErrors = [];
    }

    /// <summary>
    /// �ʐM���ɗ�O�����������ꍇ�̓����p�R���X�g���N�^�B
    /// </summary>
    internal CallResult(Request request, Exception exception, TimeStamp timeStamp, Response response)
    {
        Request = request;
        Exception = exception;
        Response = response;
        TimeStamp = timeStamp;
        ValidationErrors = [];
    }

    /// <summary>
    /// �o���f�[�V�����G���[���������ꍇ�̓����p�R���X�g���N�^�B
    /// </summary>
    internal CallResult(Request request, T data, TimeStamp timeStamp, Response response, IEnumerable<string> validationErrors)
    {
        Request = request;
        Data = data;
        TimeStamp = timeStamp;
        Response = response;
        ValidationErrors = [.. validationErrors];
    }

    /// <summary>
    /// �������� CallResult �𐶐�����t�@�N�g�����\�b�h�B
    /// </summary>
    public static CallResult<T> Success(Request request, T data, TimeStamp timeStamp, Response response)
        => new(request, data, timeStamp, response, []);

    /// <summary>
    /// ��O�𔺂����s���� CallResult �𐶐�����t�@�N�g�����\�b�h�B
    /// </summary>
    public static CallResult<T> Failure(Request request, Exception exception, TimeStamp timeStamp, Response response)
        => new(request, exception, timeStamp, response);

    /// <summary>
    /// �o���f�[�V�������s���� CallResult �𐶐�����t�@�N�g�����\�b�h�B
    /// </summary>
    public static CallResult<T> ValidationFailed(Request request, T data, TimeStamp timeStamp, Response response, IEnumerable<string> errors)
        => new(request, data, timeStamp, response, errors);

    /// <summary>
    /// �f�V���A���C�Y�Ɏ��s�����ꍇ�� CallResult �𐶐�����t�@�N�g�����\�b�h�B
    /// </summary>
    public static CallResult<T> DeserializationFailed(Request request, TimeStamp timeStamp, Response response)
        => new(request, null!, timeStamp, response, []);
}
