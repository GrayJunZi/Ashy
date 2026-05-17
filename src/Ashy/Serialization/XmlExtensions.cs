using System.Xml;
using System.Xml.Serialization;

namespace Ashy.Serialization;

/// <summary>
/// XML 序列化 / 反序列化扩展
/// </summary>
public static class XmlExtensions
{
    /// <summary>
    /// 将对象序列化为 XML 字符串
    /// </summary>
    public static string ToXml<T>(this T value)
    {
        using var sw = new StringWriter();
        using var xw = XmlWriter.Create(sw);
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(xw, value);
        return sw.ToString();
    }

    /// <summary>
    /// 将对象序列化为 XML 字符串（异步）
    /// </summary>
    public static async Task<string> ToXmlAsync<T>(this T value)
    {
        await using var sw = new StringWriter();
        await using var xw = XmlWriter.Create(sw, new XmlWriterSettings { Async = true });
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(xw, value);
        return sw.ToString();
    }

    /// <summary>
    /// 将 XML 字符串反序列化为对象
    /// </summary>
    public static T FromXml<T>(this string xml)
    {
        using var sr = new StringReader(xml);
        using var xr = XmlReader.Create(sr);
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(xr)!;
    }

    /// <summary>
    /// 将 XML 字符串反序列化为对象（异步）
    /// </summary>
    public static async Task<T> FromXmlAsync<T>(this string xml)
    {
        using var sr = new StringReader(xml);
        using var xr = XmlReader.Create(sr, new XmlReaderSettings { Async = true });
        var serializer = new XmlSerializer(typeof(T));
        var result = (T)serializer.Deserialize(xr)!;
        return result;
    }
}