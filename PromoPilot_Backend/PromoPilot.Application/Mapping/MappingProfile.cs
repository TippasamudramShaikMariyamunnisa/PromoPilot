using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Campaign Mapping
            CreateMap<CampaignDto, Campaign>()
                .ForMember(dest => dest.CampaignId, opt => opt.Ignore());

            CreateMap<Campaign, CampaignDto>();

            // Other Entity Mappings
            CreateMap<Budget, BudgetDto>().ReverseMap();
            CreateMap<Sale, SaleDto>().ReverseMap();
            CreateMap<Engagement, EngagementDto>().ReverseMap();
            CreateMap<ExecutionStatus, ExecutionStatusDto>().ReverseMap();
            CreateMap<CampaignReport, CampaignReportDto>().ReverseMap();
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();

            // Detailed CampaignReport Mapping
            CreateMap<CampaignReport, CampaignReportDto>()
                .ForMember(dest => dest.ReportID, opt => opt.MapFrom(src => src.ReportId))
                .ForMember(dest => dest.CampaignID, opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.ROI, opt => opt.MapFrom(src => src.Roi))
                .ForMember(dest => dest.Reach, opt => opt.MapFrom(src => src.Reach))
                .ForMember(dest => dest.ConversionRate, opt => opt.MapFrom(src => src.ConversionRate))
                .ForMember(dest => dest.GeneratedDate, opt => opt.MapFrom(src => src.GeneratedDate))
                .ReverseMap();
        }
    }
}
