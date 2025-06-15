using System.Net;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST 通信の結果（成功／失敗／例外）を一元的に保持するラッパークラス。
/// レスポンスの内容に加え、通信中に発生した例外や時刻情報も含む。
/// </summary>
public class CallResult
{
    /// <summary>
    /// 実際に送信されたリクエスト情報。
    /// エンドポイント、HTTP メソッド、ヘッダーなどが含まれる。
    /// </summary>
    public Request Request { get; }

    /// <summary>
    /// レスポンスのステータスコードやボディなどの情報。
    /// 通信に失敗した場合はデフォルト構築された Response を返す。
    /// </summary>
    public Response Response { get; }

    /// <summary>
    /// リクエストの送信・受信に関するタイムスタンプ情報。
    /// 通信時間やタイムアウト分析に利用できる。
    /// </summary>
    public TimeStamp TimeStamp { get; }

    /// <summary>
    /// 通信中に発生した例外（例：タイムアウト、接続失敗、キャンセルなど）。
    /// 成功時は null。
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// 通信が成功したかどうか。
    /// HTTP ステータスコードが 2xx であり、例外が発生していない場合に true。
    /// </summary>
    public bool IsSuccess => Exception == null && ((int)Response.HttpStatusCode is >= 200 and < 300);

    /// <summary>
    /// レートリミット（HTTP 429）によって拒否されたかどうか。
    /// リトライ制御やアラートのトリガーとして利用可能。
    /// </summary>
    public bool IsRateLimited => Response.HttpStatusCode == HttpStatusCode.TooManyRequests;

    /// <summary>
    /// サーバー側のエラー（HTTP 5xx）であるかどうか。
    /// 一般にリトライ可能と見なされる。
    /// </summary>
    public bool IsServerError => ((int)Response.HttpStatusCode is >= 500 and < 600);

    /// <summary>
    /// クライアント側のエラー（HTTP 4xx）であるかどうか。
    /// パラメータミスや認証失敗などが該当。
    /// </summary>
    public bool IsClientError => ((int)Response.HttpStatusCode is >= 400 and < 500);

    /// <summary>
    /// 成功時に使用される内部コンストラクタ。
    /// </summary>
    internal CallResult(Request request, Response response, TimeStamp timeStamp)
    {
        Request = request;
        Response = response;
        TimeStamp = timeStamp;
    }

    /// <summary>
    /// 例外が発生した場合に使用される内部コンストラクタ。
    /// Response は空のインスタンスで代替される。
    /// </summary>
    internal CallResult(Request request, Exception exception, TimeStamp timeStamp)
    {
        Request = request;
        Exception = exception;
        Response = new Response();
        TimeStamp = timeStamp;
    }

    /// <summary>
    /// 例外を伴う失敗結果を生成するユーティリティメソッド。
    /// </summary>
    /// <param name="request">送信されたリクエスト</param>
    /// <param name="exception">発生した例外</param>
    /// <param name="timeStamp">送信～失敗までの時間情報</param>
    /// <returns>失敗を表す CallResult インスタンス</returns>
    public static CallResult Failure(Request request, Exception exception, TimeStamp timeStamp)
        => new(request, exception, timeStamp);
}
