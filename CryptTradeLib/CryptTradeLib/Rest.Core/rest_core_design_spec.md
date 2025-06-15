# RestDispatcher クラスの設計仕様書

## 概要

`RestDispatcher` は、HTTP ベースの REST API 通信において、以下の機能を統合的に提供する実装クラスです：

- HTTP リクエスト送信
- リトライポリシーの適用（Polly）
- レスポンス解析とログ出力
- デシリアライズ処理（Json）
- FluentValidation によるレスポンス検証
- 結果の型安全な集約（CallResult, CallResult<T>）

本クラスは `IRestDispatcher` インターフェースの標準実装です。

---

## コンストラクタ

```
public RestDispatcher(HttpClient httpClient, ILogger<RestDispatcher> logger, IRetryPolicyProvider retryPolicyProvider)
```

- `HttpClient`: 実際の HTTP 通信を担当
- `ILogger<RestDispatcher>`: ログ出力用（リクエスト送信・応答・エラー）
- `IRetryPolicyProvider`: リクエストに対して適用される Polly リトライ戦略を提供

※ C# 12 のプライマリコンストラクタ未使用（明示性・互換性を優先）

---

## メソッド: `SendAsync(Request)`

```
public async Task<CallResult> SendAsync(Request request, CancellationToken cancellationToken = default)
```

### 処理概要：
1. `Request.RequestId` を初期化（未指定なら自動生成）
2. 任意の `Request.Timeout` に応じて `CancellationTokenSource` を構築
3. `HttpRequestMessage` を生成（Method, Uri, Headers, Body）
4. リトライポリシーを適用して `_httpClient.SendAsync()` を実行
5. レスポンスボディを読み取り、`CallResult` にまとめて返却

### エラー処理：
- 通信エラー発生時は `CallResult.Failure()` に例外情報とタイムスタンプを格納

---

## メソッド: `SendAsync<T>(Request, IValidator<T>?, CancellationToken)`

```
public async Task<CallResult<T>> SendAsync<T>(Request request, IValidator<T>? validator = null, CancellationToken cancellationToken = default)
    where T : class
```

### 処理概要：
1. 上記の `SendAsync(Request)` を呼び出し、通信とレスポンス取得
2. レスポンス本文を `JsonSerializer.Deserialize<T>` により T 型へ変換
3. `validator` が指定されている場合は FluentValidation による検証を実施
4. 以下のいずれかに応じた `CallResult<T>` を返却：
   - 成功：`CallResult<T>.Success()`
   - デシリアライズ失敗：`CallResult<T>.DeserializationFailed()`
   - バリデーション失敗：`CallResult<T>.ValidationFailed()`
   - 通信失敗／例外：`CallResult<T>.Failure()`

### 補足：
- `T : class` 制約を付加しているため、`null!` 使用や失敗時の `Data == null` 判定が安全
- バリデーションエラーは `ValidationErrors` プロパティに一覧で格納される
- エラー／成功は `IsSuccess`, `IsDeserializationFailed` などの補助プロパティで判定可能

---

## ログ出力

- 送信前：`LogInformation("Sending request [{RequestId}]: {Method} {Uri}")`
- 応答後：`LogInformation("Received response [{RequestId}]: {StatusCode}")`
- 失敗時：`LogError(ex, "Request failed [{RequestId}] - {Message}")`
- デシリアライズ失敗時：`LogError(ex, "Deserialization failed for response [{RequestId}]")`

---

## 補足設計方針

- 例外はスローせず `CallResult` に格納し、呼び出し側が明示的に処理できる設計
- 低レイヤ（通信、解析）と高レイヤ（モデル検証）の責務を同居させつつ一貫した API 提供
- DI、テストの柔軟性を考慮し、依存オブジェクト（HttpClient / Logger / RetryPolicy）を注入

---

## 今後の拡張余地（設計メモ）

- Content-Type に応じた `HttpContent` 切り替え（JSON以外も対応）
- リトライ判定戦略の強化（ステータスコード/レスポンス内容ベース）
- `CallResult<T>` にメタ情報（ヘッダー、レスポンス時間など）の追加
- 複数 API サーバーとの統合利用時のルーティング対応

