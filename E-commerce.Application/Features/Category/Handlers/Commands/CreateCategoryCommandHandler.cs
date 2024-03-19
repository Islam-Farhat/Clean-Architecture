using AutoMapper;
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
    internal class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand>
    {
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<Category> _logger;
        private readonly IValidator<CreateCategoryCommand> _validator;

        public CreateCategoryCommandHandler(IGenericRepository<Category> categoryRepo, IMapper mapper, ILogger<Category> logger, IValidator<CreateCategoryCommand> validator)
        {
            this._categoryRepo = categoryRepo;
            this._mapper = mapper;
            this._logger = logger;
            this._validator = validator;
        }

        public async Task<ResponseModel> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return ResponseModel.Failure(validationResult.Errors.Select(e => e.PropertyName + " : " + e.ErrorMessage));
               
                var category = _mapper.Map<Category>(request);

                await _categoryRepo.Add(category);
                var isSuccessfull = await _categoryRepo.Save();

                return ResponseModel.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }
            return ResponseModel.Failure("Error");
        }
    }
}
