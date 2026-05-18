using Ashy.Exceptions;

namespace Ashy.UnitTests.Exceptions;

public class DefaultExceptionMapperTests
{
    private readonly DefaultExceptionMapper _mapper = new();

    [Fact]
    public void Map_ArgumentException_Returns_400()
    {
        var problem = _mapper.Map(new ArgumentException("test"));
        Assert.Equal(400, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("test", problem.Detail);
        Assert.Contains("rfc7231#section-6.5.1", problem.Type);
    }

    [Fact]
    public void Map_UnauthorizedAccessException_Returns_401()
    {
        var problem = _mapper.Map(new UnauthorizedAccessException("denied"));
        Assert.Equal(401, problem.Status);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Contains("rfc7235#section-3.1", problem.Type);
    }

    [Fact]
    public void Map_InvalidOperationException_Returns_409()
    {
        var problem = _mapper.Map(new InvalidOperationException("conflict"));
        Assert.Equal(409, problem.Status);
        Assert.Equal("Conflict", problem.Title);
    }

    [Fact]
    public void Map_NotImplementedException_Returns_501()
    {
        var problem = _mapper.Map(new NotImplementedException("not impl"));
        Assert.Equal(501, problem.Status);
        Assert.Equal("Not Implemented", problem.Title);
        Assert.Contains("rfc7231#section-6.6.1", problem.Type);
    }

    [Fact]
    public void Map_UnknownException_Returns_500()
    {
        var problem = _mapper.Map(new Exception("unknown"));
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.Contains("rfc7231#section-6.6.1", problem.Type);
    }

    [Fact]
    public void Map_All_Responses_Have_NonEmpty_Type()
    {
        var exceptions = new Exception[]
        {
            new ArgumentException(),
            new UnauthorizedAccessException(),
            new InvalidOperationException(),
            new NotImplementedException(),
            new Exception()
        };

        foreach (var ex in exceptions)
        {
            var problem = _mapper.Map(ex);
            Assert.False(string.IsNullOrEmpty(problem.Type));
        }
    }
}