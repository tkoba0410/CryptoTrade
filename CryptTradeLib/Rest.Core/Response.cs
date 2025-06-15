using System.Net;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST API のレスポンスから得られる基本的な HTTP 応答情報を保持するクラス。
/// ステータスコード、理由句、ボディ文字列など、上位層での処理判断やログ出力に活用される。
/// </summary>
public class Response
{
    /// <summary>
    /// HTTP ステータスコード（例：200 OK, 404 Not Found, 500 Internal Server Error）。
    /// 成否の判定やリトライ判定などに用いられる。
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; init; } = default;

    /// <summary>
    /// ステータスの理由句（例: \"OK\", \"Bad Request\", \"Internal Server Error\"）。
    /// API提供側が明示する文言で、ログ出力やデバッグに役立つ。
    /// </summary>
    public string HttpReasonPhrase { get; init; } = string.Empty;

    /// <summary>
    /// レスポンスボディ（文字列形式）。通常は JSON またはプレーンテキスト。
    /// 上位層で必要に応じてデシリアライズ・パースされる。
    /// </summary>
    public string ResponseString { get; init; } = string.Empty;

    /// <summary>
    /// フレームワーク外からの直接生成を防ぐための内部コンストラクタ。
    /// </summary>
    internal Response() { }

    /// <summary>
    /// フィールドを明示的に指定して初期化するための内部コンストラクタ。
    /// 送信処理内部で使用される。
    /// </summary>
    /// <param name="httpStatusCode">HTTP ステータスコード</param>
    /// <param name="httpReasonPhrase">理由句（ReasonPhrase）</param>
    /// <param name="responseString">レスポンスボディの文字列</param>
    internal Response(HttpStatusCode httpStatusCode, string httpReasonPhrase, string responseString)
    {
        HttpStatusCode = httpStatusCode;
        HttpReasonPhrase = httpReasonPhrase;
        ResponseString = responseString;
    }
}
