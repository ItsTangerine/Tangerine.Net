using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalApis.Mapper.Interfaces;

namespace MinimalApis.Mapper.Extensions;

public static class MinimalApiEx
{
    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpointMapper> endpoints = app.Services
            .GetRequiredService<IEnumerable<IEndpointMapper>>();
        
        IEndpointRouteBuilder builder =
            routeGroupBuilder is null ? app : routeGroupBuilder;
        
        foreach (IEndpointMapper endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
    
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpointMapper)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpointMapper), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }
}