using E_commerce.Domian;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Validation
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator(IGenericRepository<Category> categoryRepo)
        {
            RuleFor(x => x.Name).MaximumLength(3).WithMessage("Name must be less than 3 characters!")
                                .MustAsync(async (Name, _) =>
                                {
                                    var category = await categoryRepo.GetObj(x => x.Name == Name);
                                    if (category != null)
                                        return false;

                                    return true;
                                }).WithMessage("A category must have an Name that is unique in the database");
        }
    }

}

