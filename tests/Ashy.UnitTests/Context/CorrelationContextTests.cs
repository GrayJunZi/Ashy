using Ashy.Context;

namespace Ashy.UnitTests.Context;

public class CorrelationContextTests
{
    [Fact]
    public void Default_Constructor_Generates_TraceId_32Hex()
    {
        var ctx = new CorrelationContext();
        Assert.NotNull(ctx.TraceId);
        Assert.Equal(32, ctx.TraceId.Length);
        Assert.All(ctx.TraceId, c => Assert.True(IsHex(c)));
    }

    [Fact]
    public void Default_Constructor_Generates_SpanId_16Hex()
    {
        var ctx = new CorrelationContext();
        Assert.NotNull(ctx.SpanId);
        Assert.Equal(16, ctx.SpanId.Length);
        Assert.All(ctx.SpanId, c => Assert.True(IsHex(c)));
    }

    [Fact]
    public void TraceParent_Returns_W3C_Format()
    {
        var ctx = new CorrelationContext();
        var parent = ctx.TraceParent;
        var parts = parent.Split('-');
        Assert.Equal(4, parts.Length);
        Assert.Equal("00", parts[0]);
        Assert.Equal(ctx.TraceId, parts[1]);
        Assert.Equal(ctx.SpanId, parts[2]);
        Assert.Equal(ctx.TraceFlags, parts[3]);
    }

    [Fact]
    public void FromTraceParent_Parses_Valid_String()
    {
        var parent = "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01";
        var ctx = CorrelationContext.FromTraceParent(parent);
        Assert.Equal("0af7651916cd43dd8448eb211c80319c", ctx.TraceId);
        Assert.Equal("b9c7c989f97918e1", ctx.SpanId);
        Assert.Equal("01", ctx.TraceFlags);
    }

    [Fact]
    public void FromTraceParent_Throws_On_Invalid_Format()
    {
        Assert.Throws<FormatException>(() => CorrelationContext.FromTraceParent("invalid"));
    }

    [Fact]
    public void Create_Sets_TenantId_And_UserId()
    {
        var ctx = CorrelationContext.Create(tenantId: "t1", userId: "u1");
        Assert.Equal("t1", ctx.TenantId);
        Assert.Equal("u1", ctx.UserId);
    }

    [Fact]
    public void Items_Dictionary_Is_Initially_Empty()
    {
        var ctx = new CorrelationContext();
        Assert.Empty(ctx.Items);
    }

    [Fact]
    public void Items_Dictionary_Can_Be_ReadAndWritten()
    {
        var ctx = new CorrelationContext();
        ctx.Items["key"] = "value";
        Assert.Equal("value", ctx.Items["key"]);
    }

    [Fact]
    public void Default_Constructor_TraceFlags_Is_00_When_No_Activity()
    {
        var ctx = new CorrelationContext();
        Assert.Equal("00", ctx.TraceFlags);
    }

    private static bool IsHex(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
}