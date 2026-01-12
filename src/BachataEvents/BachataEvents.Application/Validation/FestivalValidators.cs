using BachataEvents.Application.Festivals;
using FluentValidation;

namespace BachataEvents.Application.Validation;

public sealed class CreateFestivalRequestValidator : AbstractValidator<CreateFestivalRequest>
{
    public CreateFestivalRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);

        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.VenueName).MaximumLength(200);

        RuleFor(x => x.WebsiteUrl).MaximumLength(1000);
        RuleFor(x => x.TicketUrl).MaximumLength(1000);

        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x).Must(x => x.EndDate >= x.StartDate)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public sealed class UpdateFestivalRequestValidator : AbstractValidator<UpdateFestivalRequest>
{
    public UpdateFestivalRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);

        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.VenueName).MaximumLength(200);

        RuleFor(x => x.WebsiteUrl).MaximumLength(1000);
        RuleFor(x => x.TicketUrl).MaximumLength(1000);

        RuleFor(x => x).Must(x => x.EndDate >= x.StartDate)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public sealed class FestivalQueryValidator : AbstractValidator<FestivalQuery>
{
    public FestivalQueryValidator()
    {
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.Q).MaximumLength(200);

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x).Must(x => x.EndDate!.Value >= x.StartDate!.Value)
                .WithMessage("endDate must be on or after startDate.");
        });
    }
}

public sealed class PublishFestivalRequestValidator : AbstractValidator<PublishFestivalRequest>
{
    public PublishFestivalRequestValidator()
    {
        RuleFor(x => x.IsPublished).NotNull();
    }
}
