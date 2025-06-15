namespace CryptTradeLib.Rest.Core;

/// <summary>
/// リクエストの送信時刻およびレスポンスの受信時刻を記録するクラス。
/// 主に通信処理のパフォーマンス測定やトラブルシュートのために使用される。
/// </summary>
public class TimeStamp
{
    /// <summary>
    /// リクエストを送信した時点の UTC 時刻。
    /// 通常、HTTP リクエストが HttpClient により送信された直後に記録される。
    /// </summary>
    public DateTimeOffset SendTime { get; init; }

    /// <summary>
    /// レスポンスを受信した時点の UTC 時刻。
    /// 通信がタイムアウトや例外によって失敗した場合は null となる。
    /// </summary>
    public DateTimeOffset? ReceiveTime { get; set; }

    /// <summary>
    /// リクエスト送信からレスポンス受信までにかかった時間。
    /// 正常にレスポンスが受信された場合のみ有効な値を持ち、それ以外は null。
    /// 通信遅延や性能ボトルネックの分析に利用可能。
    /// </summary>
    public TimeSpan? Elapsed => ReceiveTime.HasValue ? ReceiveTime.Value - SendTime : null;
}
