using BachataEvents.Application.Common;
using FluentValidation;

namespace BachataEvents.Application.Validation;

public static class ValidationExtensions
{
    public static async Task ValidateOrThrowAsync<T>(this IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid) return;

        var dict = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).Distinct().ToArray()
            );

        throw new ValidationFailedException(dict);
    }
}
