namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST API �ɑ΂��� HTTP ���N�G�X�g�̍\����\���N���X�B
/// �G���h�|�C���g�A�p�X�A�N�G���A�{�f�B�A�w�b�_�[�Ȃǂ��ʂɕێ����A�g�ݗ��āE��͂�e�Ղɂ���B
/// </summary>
public class Request
{
    /// <summary>
    /// API �̃x�[�X URI�i��: https://api.exchange.com�j�B
    /// </summary>
    public string EndpointUri { get; set; } = string.Empty;

    /// <summary>
    /// HTTP ���\�b�h�iGET�APOST�APUT�ADELETE �Ȃǁj�B
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// �G���h�|�C���g�ȉ��̃��N�G�X�g�p�X�i��: /v1/orders�j�B
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// �N�G��������i��: symbol=BTC&limit=10�j�B�擪�́u?�v�͊܂܂Ȃ��B
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// ���N�G�X�g�{�f�B�iJSON ������Ȃǁj�B
    /// GET �Ȃǂł͋󕶎���̂܂܂ł悢�B
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// ���ۂɑ��M����銮�S�ȃ��N�G�X�g URI�B
    /// EndpointUri + Path + \"?\" + Query ��g�ݗ��Ă����ʂ��i�[�B
    /// </summary>
    public string RequestUri { get; set; } = string.Empty;

    /// <summary>
    /// HTTP ���N�G�X�g�w�b�_�[�̎����B�L�[�̓w�b�_�[���A�l�͂��̓��e�B
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// ���̃��N�G�X�g�ɑ΂��Čʂɐݒ肳�ꂽ�^�C���A�E�g�B
    /// null �̏ꍇ�� HttpClient ���̊���l���g����B
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// ���O��ǐՂ̂��߂̔C�ӂ̈�ӎ��ʎq�B�w�肪�Ȃ���Γ����Ŏ������������B
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// �O���C���X�^���X�����֎~���A�r���_�[��t�@�N�g�����\�b�h����̐����Ɍ��肷��B
    /// </summary>
    internal Request() { }

    /// <summary>
    /// �^�v���̃L�[�E�l����N�G����������\�z���郆�[�e�B���e�B�B
    /// null �܂��͋�̒l�͖��������B
    /// </summary>
    public static string ToQueryString(params (string, string?)[] query)
    {
        var nonEmptyPairs = query.Where(x => !string.IsNullOrEmpty(x.Item2));
        if (!nonEmptyPairs.Any()) return string.Empty;

        var queryString = "?" + string.Join("&", nonEmptyPairs
            .Select(pair => $"{Uri.EscapeDataString(pair.Item1)}={Uri.EscapeDataString(pair.Item2 ?? string.Empty)}"));
        return queryString;
    }

    /// <summary>
    /// �N�G��������� (key, value) �̔z��ɕϊ�����B
    /// �擪�Ɂu?�v���t���Ă��Ȃ��ꍇ�� null �̏ꍇ�͋�z��B
    /// </summary>
    public static (string, string?)[] ParseQueryString(string? queryString)
    {
        if (string.IsNullOrEmpty(queryString) || queryString[0] != '?') return [];

        queryString = queryString[1..];
        string[] keyValuePairs = queryString.Split('&');

        return [.. keyValuePairs
            .Select(pair =>
            {
                string[] parts = pair.Split('=');
                string key = Uri.UnescapeDataString(parts[0]);
                string? value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : null;
                return (key, value);
            })];
    }

    /// <summary>
    /// �N�G��������� SortedDictionary �ɕϊ�����i�L�[���ň���j�B
    /// </summary>
    public static SortedDictionary<string, string?> ParseQueryStringToDict(string? queryString)
    {
        if (string.IsNullOrEmpty(queryString) || queryString[0] != '?') return [];

        queryString = queryString[1..];
        string[] keyValuePairs = queryString.Split('&');

        var result = keyValuePairs
            .Select(pair =>
            {
                string[] parts = pair.Split('=');
                string key = Uri.UnescapeDataString(parts[0]);
                string? value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : null;
                return (key, value);
            }).ToDictionary(x => x.key, x => x.value);

        return new SortedDictionary<string, string?>(result);
    }
}
