using AutoMapper;
using E_commerce.Application;
using E_commerce.Application.Dto.Category;
using E_commerce.Application.Validation;
using E_commerce.Domian;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<Category> _logger;
        private readonly IValidator<CreateCategoryDto> _validator;

        public CategoryService(IGenericRepository<Category> categoryRepo, IMapper mapper, ILogger<Category> logger, IValidator<CreateCategoryDto> validator)
        {
            this._categoryRepo = categoryRepo;
            this._mapper = mapper;
            this._logger = logger;
            this._validator = validator;
        }

        public async Task<ResponseModel<GetCategoryDto>> GetObj(GetCategoryDto categoryDto)
        {
            try
            {
                var category = await _categoryRepo.GetObj(c => c.Name == categoryDto.Name);
                if (category == null)
                    return new ResponseModel<GetCategoryDto>(false, errorMessage: "Not Exist!");

                var result = _mapper.Map<GetCategoryDto>(category);

                return new ResponseModel<GetCategoryDto>(isSuccess: true, data: result);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return new ResponseModel<GetCategoryDto>(false);

        }

        public async Task<ResponseModel<CreateCategoryDto>> Add(CreateCategoryDto value)
        {

            try
            {
                var validationResult = await _validator.ValidateAsync(value);
                if (!validationResult.IsValid)
                     return new ResponseModel<CreateCategoryDto>(false,errorMessage: validationResult.Errors.Select(e => e.PropertyName + " : " + e.ErrorMessage));

                var category = _mapper.Map<Category>(value);

                await _categoryRepo.Add(category);
                var isSuccessfull = await _categoryRepo.Save();

                return new ResponseModel<CreateCategoryDto>(isSuccessfull);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }
            return new ResponseModel<CreateCategoryDto>(false);
        }

        public async Task<ResponseModel<List<GetCategoryDto>>> GetAll()
        {

            try
            {
                var categories = await _categoryRepo.FindAllAsync(c => c.Id > 0);
                var result = _mapper.Map<IEnumerable<GetCategoryDto>>(categories);

                return new ResponseModel<List<GetCategoryDto>>(isSuccess: true, data: result.ToList());
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return new ResponseModel<List<GetCategoryDto>>(false);

        }
    }
}
