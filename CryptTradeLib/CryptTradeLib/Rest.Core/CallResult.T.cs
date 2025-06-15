using System.Net;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// 型付き REST API 通信の結果を表すクラス。
/// 通信ステータス、デシリアライズ結果、例外、バリデーション情報などを統合して保持する。
/// </summary>
public class CallResult<T> where T : class
{
    /// <summary>
    /// 実際に送信されたリクエストオブジェクト。
    /// エンドポイント、ヘッダー、メソッドなどを含む。
    /// </summary>
    public Request Request { get; }

    /// <summary>
    /// レスポンスに関する HTTP ステータスやボディ情報。
    /// 通信成功／失敗を問わず必ず保持される。
    /// </summary>
    public Response Response { get; }

    /// <summary>
    /// 通信の送受信時刻や経過時間を示す情報。
    /// パフォーマンス分析やロギングに利用可能。
    /// </summary>
    public TimeStamp TimeStamp { get; }

    /// <summary>
    /// 通信中に発生した例外（タイムアウト、接続エラーなど）。
    /// 成功時は null。
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// レスポンスボディをデシリアライズして得られたデータ。
    /// JSON パース失敗時や例外時には null。
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// データに対して実行されたバリデーションエラーの一覧。
    /// 成功時は空。
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// 通信とデシリアライズが成功し、かつバリデーションエラーもない場合に true。
    /// </summary>
    public bool IsSuccess =>
        Exception == null && ValidationErrors.Count == 0 && Data != null;

    /// <summary>
    /// レート制限（HTTP 429）による失敗かどうか。
    /// </summary>
    public bool IsRateLimited =>
        Response.HttpStatusCode == HttpStatusCode.TooManyRequests;

    /// <summary>
    /// サーバーエラー（HTTP 5xx）かどうか。リトライの判断材料となる。
    /// </summary>
    public bool IsServerError =>
        (int)Response.HttpStatusCode is >= 500 and < 600;

    /// <summary>
    /// クライアントエラー（HTTP 4xx）かどうか。
    /// パラメータミスや認証エラーなどの特定に使われる。
    /// </summary>
    public bool IsClientError =>
        (int)Response.HttpStatusCode is >= 400 and < 500;

    /// <summary>
    /// 通信は成功したが JSON などのデシリアライズに失敗した場合に true。
    /// </summary>
    public bool IsDeserializationFailed =>
        Exception == null && Data == null && !string.IsNullOrWhiteSpace(Response.ResponseString);

    /// <summary>
    /// 通信およびデシリアライズが成功した場合の内部用コンストラクタ。
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
    /// 通信中に例外が発生した場合の内部用コンストラクタ。
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
    /// バリデーションエラーがあった場合の内部用コンストラクタ。
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
    /// 成功時の CallResult を生成するファクトリメソッド。
    /// </summary>
    public static CallResult<T> Success(Request request, T data, TimeStamp timeStamp, Response response)
        => new(request, data, timeStamp, response, []);

    /// <summary>
    /// 例外を伴う失敗時の CallResult を生成するファクトリメソッド。
    /// </summary>
    public static CallResult<T> Failure(Request request, Exception exception, TimeStamp timeStamp, Response response)
        => new(request, exception, timeStamp, response);

    /// <summary>
    /// バリデーション失敗時の CallResult を生成するファクトリメソッド。
    /// </summary>
    public static CallResult<T> ValidationFailed(Request request, T data, TimeStamp timeStamp, Response response, IEnumerable<string> errors)
        => new(request, data, timeStamp, response, errors);

    /// <summary>
    /// デシリアライズに失敗した場合の CallResult を生成するファクトリメソッド。
    /// </summary>
    public static CallResult<T> DeserializationFailed(Request request, TimeStamp timeStamp, Response response)
        => new(request, null!, timeStamp, response, []);
}
