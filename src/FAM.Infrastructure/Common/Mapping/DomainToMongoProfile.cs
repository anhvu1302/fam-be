using AutoMapper;
using FAM.Domain.Companies;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.PersistenceModels.Mongo;

namespace FAM.Infrastructure.Common.Mapping;

/// <summary>
/// Mapping profile for MongoDB persistence models
/// </summary>
public class DomainToMongoProfile : Profile
{
    public DomainToMongoProfile()
    {
        CreateMap<Company, CompanyMongo>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // MongoDB generates ObjectId
            .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.TaxCode, opt => opt.MapFrom(src => src.TaxCode))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedById))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.UpdatedById, opt => opt.MapFrom(src => src.UpdatedById))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
            .ForMember(dest => dest.DeletedById, opt => opt.MapFrom(src => src.DeletedById));

        CreateMap<CompanyMongo, Company>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DomainId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.TaxCode, opt => opt.MapFrom(src => src.TaxCode))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedById))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.UpdatedById, opt => opt.MapFrom(src => src.UpdatedById))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
            .ForMember(dest => dest.DeletedById, opt => opt.MapFrom(src => src.DeletedById));

        CreateMap<User, UserMongo>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // MongoDB generates ObjectId
            .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.Value))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

        CreateMap<UserMongo, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DomainId))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => Username.Create(src.Username)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => Email.Create(src.Email)))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));
    }
}