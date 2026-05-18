using Ashy.Models;

namespace Ashy.UnitTests.Models;

public class ApiResultTests
{
    [Fact]
    public void Ok_Returns_Success_With_Data()
    {
        var result = ApiResult<string>.Ok("hello");
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.Equal("hello", result.Data);
    }

    [Fact]
    public void Fail_Returns_Error_With_Message()
    {
        var result = ApiResult<int>.Fail("error occurred");
        Assert.False(result.Success);
        Assert.Equal("error occurred", result.Message);
        Assert.Equal(default, result.Data);
    }

    [Fact]
    public void Ok_With_Null_Data()
    {
        var result = ApiResult<object?>.Ok(null);
        Assert.True(result.Success);
        Assert.Null(result.Data);
    }
}

public class PagedResultTests
{
    [Fact]
    public void HasNextPage_True_When_More_Items()
    {
        var result = new PagedResult<string>(1, 10, 25, new List<string> { "a", "b" });
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_False_When_No_More_Items()
    {
        var result = new PagedResult<string>(2, 10, 20, new List<string>());
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void HasPrevPage_True_When_Page_GT_1()
    {
        var result = new PagedResult<string>(2, 10, 100, new List<string>());
        Assert.True(result.HasPrevPage);
    }

    [Fact]
    public void HasPrevPage_False_When_Page_Is_1()
    {
        var result = new PagedResult<string>(1, 10, 100, new List<string>());
        Assert.False(result.HasPrevPage);
    }

    [Fact]
    public void TotalPages_Ceiling_Division()
    {
        var result = new PagedResult<string>(1, 10, 25, new List<string>());
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_Exact_Division()
    {
        var result = new PagedResult<string>(1, 10, 30, new List<string>());
        Assert.Equal(3, result.TotalPages);
    }
}