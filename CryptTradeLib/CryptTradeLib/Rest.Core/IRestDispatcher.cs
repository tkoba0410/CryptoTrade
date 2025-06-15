using FluentValidation;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST �ʐM�����̒��ۃC���^�[�t�F�[�X�B
/// HTTP ���N�G�X�g�̑��M����у��X�|���X�̎擾�E��͂�W�������A�����Ƃ̈ˑ��𕪗�����B
/// </summary>
public interface IRestDispatcher
{
    /// <summary>
    /// �w�肳�ꂽ HTTP ���N�G�X�g��񓯊��ő��M���A���ʂ� <see cref="CallResult"/> �Ƃ��ĕԂ��܂��B
    /// ��ɃX�e�[�^�X�R�[�h�A���X�|���X������A��O�����̗L���Ȃǂ��m�F����p�r�Ŏg�p����܂��B
    /// </summary>
    /// <param name="request">���M���� HTTP ���N�G�X�g���B</param>
    /// <param name="cancellationToken">����̃L�����Z����ʒm����g�[�N���i�C�Ӂj�B</param>
    /// <returns>
    /// �ʐM���ʂ��܂� <see cref="CallResult"/> �C���X�^���X�B
    /// �ʐM�Ɏ��s�����ꍇ�ł���O�ł͂Ȃ����s���Ƃ��ĕԂ���܂��B
    /// </returns>
    Task<CallResult> SendAsync(Request request, CancellationToken cancellationToken = default);

    /// <summary>
    /// �^�t���̃��X�|���X��񓯊��Ŏ擾���A�K�v�ɉ����� FluentValidation �ɂ�錟�؂��s���܂��B
    /// �ʐM���ʁE�f�V���A���C�Y�E�o���f�[�V�����𓝍��I�ɏ����ł��܂��B
    /// </summary>
    /// <typeparam name="T">���X�|���X�{���̃f�V���A���C�Y�Ώی^�B</typeparam>
    /// <param name="request">���M���� HTTP ���N�G�X�g���B</param>
    /// <param name="validator">
    /// �^ <typeparamref name="T"/> �ɑ΂��� FluentValidation ���؃��W�b�N�i�ȗ��\�j�B
    /// null �̏ꍇ�̓o���f�[�V�����Ȃ��Ő����Ƃ݂Ȃ���܂��B
    /// </param>
    /// <param name="cancellationToken">����̃L�����Z����ʒm����g�[�N���i�C�Ӂj�B</param>
    /// <returns>
    /// <see cref="CallResult{T}"/> �C���X�^���X�B�ʐM���s�A�p�[�X���s�A���؎��s�Ȃǂ���ʂ��ĕԂ��܂��B
    /// </returns>
    Task<CallResult<T>> SendAsync<T>(
        Request request,
        IValidator<T>? validator = null,
        CancellationToken cancellationToken = default)
        where T : class;
}
