using AutoMapper;
using BachataEvents.Application.Festivals;
using BachataEvents.Domain.Entities;

namespace BachataEvents.Application.Mapping;

public sealed class FestivalMappingProfile : Profile
{
    public FestivalMappingProfile()
    {
        CreateMap<FestivalEvent, FestivalListItemDto>();

        CreateMap<FestivalEvent, FestivalDetailsDto>();

        CreateMap<CreateFestivalRequest, FestivalEvent>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrganizerProfileId, o => o.Ignore())
            .ForMember(d => d.OrganizerProfile, o => o.Ignore())
            .ForMember(d => d.IsPublished, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());

        CreateMap<UpdateFestivalRequest, FestivalEvent>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrganizerProfileId, o => o.Ignore())
            .ForMember(d => d.OrganizerProfile, o => o.Ignore())
            .ForMember(d => d.IsPublished, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());
    }
}
