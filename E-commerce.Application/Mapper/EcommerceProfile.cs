using AutoMapper;
using E_commerce.Application.Dto.Category;
using E_commerce.Domian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Mapper
{
    public class EcommerceProfile : Profile
    {
        public EcommerceProfile()
        {
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Category, GetCategoryDto>().ReverseMap();
        }
    }
}
