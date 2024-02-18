using E_commerce.Application.Dto.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    public interface ICategoryService
    {
        Task<ResponseModel<GetCategoryDto>> GetObj(GetCategoryDto categoryDto);
        Task<ResponseModel<CreateCategoryDto>> Add(CreateCategoryDto value);
        Task<ResponseModel<List<GetCategoryDto>>> GetAll();
    }
}
