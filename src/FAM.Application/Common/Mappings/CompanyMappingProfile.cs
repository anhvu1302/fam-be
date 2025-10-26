using AutoMapper;
using FAM.Application.Companies.DTOs;
using FAM.Domain.Companies;

namespace FAM.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for Company mappings
/// </summary>
public class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        CreateMap<Company, CompanyDto>();
        CreateMap<CompanyDto, Company>();
    }
}