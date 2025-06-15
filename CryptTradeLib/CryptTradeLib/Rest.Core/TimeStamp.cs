namespace CryptTradeLib.Rest.Core;

/// <summary>
/// ���N�G�X�g�̑��M��������у��X�|���X�̎�M�������L�^����N���X�B
/// ��ɒʐM�����̃p�t�H�[�}���X�����g���u���V���[�g�̂��߂Ɏg�p�����B
/// </summary>
public class TimeStamp
{
    /// <summary>
    /// ���N�G�X�g�𑗐M�������_�� UTC �����B
    /// �ʏ�AHTTP ���N�G�X�g�� HttpClient �ɂ�著�M���ꂽ����ɋL�^�����B
    /// </summary>
    public DateTimeOffset SendTime { get; init; }

    /// <summary>
    /// ���X�|���X����M�������_�� UTC �����B
    /// �ʐM���^�C���A�E�g���O�ɂ���Ď��s�����ꍇ�� null �ƂȂ�B
    /// </summary>
    public DateTimeOffset? ReceiveTime { get; set; }

    /// <summary>
    /// ���N�G�X�g���M���烌�X�|���X��M�܂łɂ����������ԁB
    /// ����Ƀ��X�|���X����M���ꂽ�ꍇ�̂ݗL���Ȓl�������A����ȊO�� null�B
    /// �ʐM�x���␫�\�{�g���l�b�N�̕��͂ɗ��p�\�B
    /// </summary>
    public TimeSpan? Elapsed => ReceiveTime.HasValue ? ReceiveTime.Value - SendTime : null;
}
