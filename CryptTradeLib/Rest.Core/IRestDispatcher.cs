using FluentValidation;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// REST 通信処理の抽象インターフェース。
/// HTTP リクエストの送信およびレスポンスの取得・解析を標準化し、実装との依存を分離する。
/// </summary>
public interface IRestDispatcher
{
    /// <summary>
    /// 指定された HTTP リクエストを非同期で送信し、結果を <see cref="CallResult"/> として返します。
    /// 主にステータスコード、レスポンス文字列、例外発生の有無などを確認する用途で使用されます。
    /// </summary>
    /// <param name="request">送信する HTTP リクエスト情報。</param>
    /// <param name="cancellationToken">操作のキャンセルを通知するトークン（任意）。</param>
    /// <returns>
    /// 通信結果を含む <see cref="CallResult"/> インスタンス。
    /// 通信に失敗した場合でも例外ではなく失敗情報として返されます。
    /// </returns>
    Task<CallResult> SendAsync(Request request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 型付きのレスポンスを非同期で取得し、必要に応じて FluentValidation による検証も行います。
    /// 通信結果・デシリアライズ・バリデーションを統合的に処理できます。
    /// </summary>
    /// <typeparam name="T">レスポンス本文のデシリアライズ対象型。</typeparam>
    /// <param name="request">送信する HTTP リクエスト情報。</param>
    /// <param name="validator">
    /// 型 <typeparamref name="T"/> に対する FluentValidation 検証ロジック（省略可能）。
    /// null の場合はバリデーションなしで成功とみなされます。
    /// </param>
    /// <param name="cancellationToken">操作のキャンセルを通知するトークン（任意）。</param>
    /// <returns>
    /// <see cref="CallResult{T}"/> インスタンス。通信失敗、パース失敗、検証失敗などを区別して返します。
    /// </returns>
    Task<CallResult<T>> SendAsync<T>(
        Request request,
        IValidator<T>? validator = null,
        CancellationToken cancellationToken = default)
        where T : class;
}
