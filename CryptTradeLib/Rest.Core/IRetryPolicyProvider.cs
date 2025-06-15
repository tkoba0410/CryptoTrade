using Polly;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// ����̃��N�G�X�g�ɑ΂��ēK�p���� Polly ���g���C�|���V�[�𓮓I�ɒ񋟂���C���^�[�t�F�[�X�B
/// �ʐM�Ώۂ�p�����[�^�ɉ������J�X�^�����g���C������\�ɂ��A��������Ƃ̎d�l�����z������B
/// </summary>
public interface IRetryPolicyProvider
{
    /// <summary>
    /// �w�肳�ꂽ���N�G�X�g�ɑ΂��ēK�p���ׂ� <see cref="IAsyncPolicy{HttpResponseMessage}"/> ���\�z���ĕԂ��܂��B
    /// �����ɂ��AHTTP ���\�b�h�A�G���h�|�C���g�AAPI ��ʂȂǂɉ��������I���䂪�\�ł��B
    /// </summary>
    /// <param name="request">
    /// ���g���C�ΏۂƂȂ� HTTP ���N�G�X�g���B
    /// ���\�b�h�� URI�A�w�b�_�[�Ȃǂ̓��e����ɔ��f�\�ł��B
    /// </param>
    /// <returns>
    /// Polly �ɂ��񓯊����g���C�|���V�[�B
    /// ���g���C���Ȃ��|���V�[�iPolicy.NoOp�j��A�ő厎�s�񐔕t���̃|���V�[�Ȃǂ�Ԃ��������z�肳��܂��B
    /// </returns>
    IAsyncPolicy<HttpResponseMessage> CreatePolicy(Request request);
}
