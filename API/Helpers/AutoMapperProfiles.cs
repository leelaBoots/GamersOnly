using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        // maps properties from one object to another. it saves time from having to manually type out the mappings yourself.
        // some mappings that are not clear need to be specified here
        public AutoMapperProfiles() {
            // this maps AppUser to MemberDto, it also specifically maps the url of the main photo to a property called PhotoUrl
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>(); // used when updating profile
            CreateMap<RegisterDto, AppUser>(); // this is so we dont have to manually map the proerties we are receiving from our accountcontroller
            CreateMap<Message, MessageDto>()
              .ForMember(d => d.SenderPhotoUrl, o => o.MapFrom(s => s.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
              .ForMember(d => d.RecipientPhotoUrl, o => o.MapFrom(s => s.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
        }
        
    }
}