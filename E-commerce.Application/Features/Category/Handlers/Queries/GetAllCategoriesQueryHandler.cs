using AutoMapper;
using E_commerce.Application;
using E_commerce.Application.Dto.Category;
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
    public class GetAllCategoriesQueryHandler : IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>>
    {
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<Category> _logger;

        public GetAllCategoriesQueryHandler(IGenericRepository<Category> categoryRepo, IMapper mapper, ILogger<Category> logger)
        {
            this._categoryRepo = categoryRepo;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<ResponseModel<List<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _categoryRepo.FindAllAsync(c => c.Id > 0);
                var result = _mapper.Map<IEnumerable<CategoryResponse>>(categories);

                return new ResponseModel<List<CategoryResponse>>(isSuccess: true, data: result.ToList());
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return new ResponseModel<List<CategoryResponse>>(false);

        }
    }
}
