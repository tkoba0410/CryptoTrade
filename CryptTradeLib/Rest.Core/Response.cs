using System.Net;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST API �̃��X�|���X���瓾�����{�I�� HTTP ��������ێ�����N���X�B
/// �X�e�[�^�X�R�[�h�A���R��A�{�f�B������ȂǁA��ʑw�ł̏������f�⃍�O�o�͂Ɋ��p�����B
/// </summary>
public class Response
{
    /// <summary>
    /// HTTP �X�e�[�^�X�R�[�h�i��F200 OK, 404 Not Found, 500 Internal Server Error�j�B
    /// ���ۂ̔���⃊�g���C����Ȃǂɗp������B
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; init; } = default;

    /// <summary>
    /// �X�e�[�^�X�̗��R��i��: \"OK\", \"Bad Request\", \"Internal Server Error\"�j�B
    /// API�񋟑����������镶���ŁA���O�o�͂�f�o�b�O�ɖ𗧂B
    /// </summary>
    public string HttpReasonPhrase { get; init; } = string.Empty;

    /// <summary>
    /// ���X�|���X�{�f�B�i������`���j�B�ʏ�� JSON �܂��̓v���[���e�L�X�g�B
    /// ��ʑw�ŕK�v�ɉ����ăf�V���A���C�Y�E�p�[�X�����B
    /// </summary>
    public string ResponseString { get; init; } = string.Empty;

    /// <summary>
    /// �t���[�����[�N�O����̒��ڐ�����h�����߂̓����R���X�g���N�^�B
    /// </summary>
    internal Response() { }

    /// <summary>
    /// �t�B�[���h�𖾎��I�Ɏw�肵�ď��������邽�߂̓����R���X�g���N�^�B
    /// ���M���������Ŏg�p�����B
    /// </summary>
    /// <param name="httpStatusCode">HTTP �X�e�[�^�X�R�[�h</param>
    /// <param name="httpReasonPhrase">���R��iReasonPhrase�j</param>
    /// <param name="responseString">���X�|���X�{�f�B�̕�����</param>
    internal Response(HttpStatusCode httpStatusCode, string httpReasonPhrase, string responseString)
    {
        HttpStatusCode = httpStatusCode;
        HttpReasonPhrase = httpReasonPhrase;
        ResponseString = responseString;
    }
}
