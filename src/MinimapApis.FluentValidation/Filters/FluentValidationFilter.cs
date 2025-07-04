using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace MinimapApis.FluentValidation.Filters;

public class FluentValidationFilter(IServiceProvider serviceProvider) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var contextArgument in context.Arguments)
        {
            if(contextArgument == null)
                continue;
            
            if(IsValidModel(contextArgument) == false)
                continue;

            var type = contextArgument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(type);
            var validator = serviceProvider.GetService(validatorType) as IValidator;
            if (validator == null) 
                continue;

            var method = validatorType.GetMethod("ValidateAsync", new[] { contextArgument.GetType(), typeof(CancellationToken) });
            if (method == null) 
                continue;

            var task = (Task)method.Invoke(validator, new [] { contextArgument, CancellationToken.None })!;
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            var validationResult = (ValidationResult?)resultProperty?.GetValue(task);

            if (validationResult?.IsValid ?? true)
                continue;
            
            return Results.ValidationProblem(validationResult.ToDictionary(), $"A validation error occurred to model: {type.Name}.");
        }
        
        return await next(context);
    }

    protected virtual bool IsValidModel(object model)
    {
        var type = model.GetType();
        return IsRecordType(type) && type.Name.EndsWith("request", StringComparison.CurrentCultureIgnoreCase);
    }
    
    private static bool IsRecordType(Type type)
    {
        return type.IsClass &&
               type.GetMethod("<Clone>$") != null;
    }
}