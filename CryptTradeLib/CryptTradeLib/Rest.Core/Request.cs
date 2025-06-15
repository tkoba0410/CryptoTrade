namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST API に対する HTTP リクエストの構造を表すクラス。
/// エンドポイント、パス、クエリ、ボディ、ヘッダーなどを個別に保持し、組み立て・解析を容易にする。
/// </summary>
public class Request
{
    /// <summary>
    /// API のベース URI（例: https://api.exchange.com）。
    /// </summary>
    public string EndpointUri { get; set; } = string.Empty;

    /// <summary>
    /// HTTP メソッド（GET、POST、PUT、DELETE など）。
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// エンドポイント以下のリクエストパス（例: /v1/orders）。
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// クエリ文字列（例: symbol=BTC&limit=10）。先頭の「?」は含まない。
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// リクエストボディ（JSON 文字列など）。
    /// GET などでは空文字列のままでよい。
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 実際に送信される完全なリクエスト URI。
    /// EndpointUri + Path + \"?\" + Query を組み立てた結果を格納。
    /// </summary>
    public string RequestUri { get; set; } = string.Empty;

    /// <summary>
    /// HTTP リクエストヘッダーの辞書。キーはヘッダー名、値はその内容。
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// このリクエストに対して個別に設定されたタイムアウト。
    /// null の場合は HttpClient 側の既定値が使われる。
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// ログや追跡のための任意の一意識別子。指定がなければ内部で自動生成される。
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// 外部インスタンス化を禁止し、ビルダーやファクトリメソッドからの生成に限定する。
    /// </summary>
    internal Request() { }

    /// <summary>
    /// タプルのキー・値からクエリ文字列を構築するユーティリティ。
    /// null または空の値は無視される。
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
    /// クエリ文字列を (key, value) の配列に変換する。
    /// 先頭に「?」が付いていない場合や null の場合は空配列。
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
    /// クエリ文字列を SortedDictionary に変換する（キー順で安定）。
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
