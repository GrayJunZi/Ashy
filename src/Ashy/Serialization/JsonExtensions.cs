using System.Text;
using System.Text.Json;

namespace Ashy.Serialization;

/// <summary>
/// JSON 序列化 / 反序列化扩展
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// 将对象序列化为 JSON 字符串
    /// </summary>
    public static string ToJson<T>(this T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// 将对象序列化为 JSON 字符串（异步）
    /// </summary>
    public static async Task<string> ToJsonAsync<T>(this T value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为对象
    /// </summary>
    public static T? FromJson<T>(this string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为对象（异步）
    /// </summary>
    public static async Task<T?> FromJsonAsync<T>(this string json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);
        return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
    }
}