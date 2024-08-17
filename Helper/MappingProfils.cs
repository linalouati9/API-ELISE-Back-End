using api_elise.Dto;
using api_elise.Models;
using AutoMapper;

namespace api_elise.Helper
{
    public class MappingProfils : Profile
    {
        public MappingProfils()
        {

            CreateMap<ModelDto, Model>().ReverseMap();
            CreateMap<QRCodeDto, QRCode>().ReverseMap();

        }
    }
}
