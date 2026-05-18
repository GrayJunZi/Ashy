using Ashy.Context;

namespace Ashy.UnitTests.Context;

public class CorrelationContextAccessorTests
{
    [Fact]
    public void Default_Current_Is_Null()
    {
        var accessor = new CorrelationContextAccessor();
        Assert.Null(accessor.Current);
    }

    [Fact]
    public void Set_And_Get_Current()
    {
        var accessor = new CorrelationContextAccessor();
        var ctx = new CorrelationContext();
        accessor.Current = ctx;
        Assert.Same(ctx, accessor.Current);
    }

    [Fact]
    public void Set_Null_Clears_Current()
    {
        var accessor = new CorrelationContextAccessor();
        accessor.Current = new CorrelationContext();
        accessor.Current = null;
        Assert.Null(accessor.Current);
    }
}