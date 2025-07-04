using Microsoft.AspNetCore.Routing;

namespace MinimalApis.Mapper.Interfaces;

public interface IEndpointMapper
{
    void MapEndpoint(IEndpointRouteBuilder app);
}