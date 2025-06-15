using Polly;

namespace CryptTradeLib.Rest.Core;

/// <summary>
/// 特定のリクエストに対して適用する Polly リトライポリシーを動的に提供するインターフェース。
/// 通信対象やパラメータに応じたカスタムリトライ制御を可能にし、取引所ごとの仕様差を吸収する。
/// </summary>
public interface IRetryPolicyProvider
{
    /// <summary>
    /// 指定されたリクエストに対して適用すべき <see cref="IAsyncPolicy{HttpResponseMessage}"/> を構築して返します。
    /// 実装により、HTTP メソッド、エンドポイント、API 種別などに応じた動的制御が可能です。
    /// </summary>
    /// <param name="request">
    /// リトライ対象となる HTTP リクエスト情報。
    /// メソッドや URI、ヘッダーなどの内容を基に判断可能です。
    /// </param>
    /// <returns>
    /// Polly による非同期リトライポリシー。
    /// リトライしないポリシー（Policy.NoOp）や、最大試行回数付きのポリシーなどを返す実装が想定されます。
    /// </returns>
    IAsyncPolicy<HttpResponseMessage> CreatePolicy(Request request);
}
