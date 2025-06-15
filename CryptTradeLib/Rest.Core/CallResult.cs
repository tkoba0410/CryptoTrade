using System.Net;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST �ʐM�̌��ʁi�����^���s�^��O�j���ꌳ�I�ɕێ����郉�b�p�[�N���X�B
/// ���X�|���X�̓��e�ɉ����A�ʐM���ɔ���������O�⎞�������܂ށB
/// </summary>
public class CallResult
{
    /// <summary>
    /// ���ۂɑ��M���ꂽ���N�G�X�g���B
    /// �G���h�|�C���g�AHTTP ���\�b�h�A�w�b�_�[�Ȃǂ��܂܂��B
    /// </summary>
    public Request Request { get; }

    /// <summary>
    /// ���X�|���X�̃X�e�[�^�X�R�[�h��{�f�B�Ȃǂ̏��B
    /// �ʐM�Ɏ��s�����ꍇ�̓f�t�H���g�\�z���ꂽ Response ��Ԃ��B
    /// </summary>
    public Response Response { get; }

    /// <summary>
    /// ���N�G�X�g�̑��M�E��M�Ɋւ���^�C���X�^���v���B
    /// �ʐM���Ԃ�^�C���A�E�g���͂ɗ��p�ł���B
    /// </summary>
    public TimeStamp TimeStamp { get; }

    /// <summary>
    /// �ʐM���ɔ���������O�i��F�^�C���A�E�g�A�ڑ����s�A�L�����Z���Ȃǁj�B
    /// �������� null�B
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// �ʐM�������������ǂ����B
    /// HTTP �X�e�[�^�X�R�[�h�� 2xx �ł���A��O���������Ă��Ȃ��ꍇ�� true�B
    /// </summary>
    public bool IsSuccess => Exception == null && ((int)Response.HttpStatusCode is >= 200 and < 300);

    /// <summary>
    /// ���[�g���~�b�g�iHTTP 429�j�ɂ���ċ��ۂ��ꂽ���ǂ����B
    /// ���g���C�����A���[�g�̃g���K�[�Ƃ��ė��p�\�B
    /// </summary>
    public bool IsRateLimited => Response.HttpStatusCode == HttpStatusCode.TooManyRequests;

    /// <summary>
    /// �T�[�o�[���̃G���[�iHTTP 5xx�j�ł��邩�ǂ����B
    /// ��ʂɃ��g���C�\�ƌ��Ȃ����B
    /// </summary>
    public bool IsServerError => ((int)Response.HttpStatusCode is >= 500 and < 600);

    /// <summary>
    /// �N���C�A���g���̃G���[�iHTTP 4xx�j�ł��邩�ǂ����B
    /// �p�����[�^�~�X��F�؎��s�Ȃǂ��Y���B
    /// </summary>
    public bool IsClientError => ((int)Response.HttpStatusCode is >= 400 and < 500);

    /// <summary>
    /// �������Ɏg�p���������R���X�g���N�^�B
    /// </summary>
    internal CallResult(Request request, Response response, TimeStamp timeStamp)
    {
        Request = request;
        Response = response;
        TimeStamp = timeStamp;
    }

    /// <summary>
    /// ��O�����������ꍇ�Ɏg�p���������R���X�g���N�^�B
    /// Response �͋�̃C���X�^���X�ő�ւ����B
    /// </summary>
    internal CallResult(Request request, Exception exception, TimeStamp timeStamp)
    {
        Request = request;
        Exception = exception;
        Response = new Response();
        TimeStamp = timeStamp;
    }

    /// <summary>
    /// ��O�𔺂����s���ʂ𐶐����郆�[�e�B���e�B���\�b�h�B
    /// </summary>
    /// <param name="request">���M���ꂽ���N�G�X�g</param>
    /// <param name="exception">����������O</param>
    /// <param name="timeStamp">���M�`���s�܂ł̎��ԏ��</param>
    /// <returns>���s��\�� CallResult �C���X�^���X</returns>
    public static CallResult Failure(Request request, Exception exception, TimeStamp timeStamp)
        => new(request, exception, timeStamp);
}
