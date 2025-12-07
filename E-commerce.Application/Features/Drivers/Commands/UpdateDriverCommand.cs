using CSharpFunctionalExtensions;
using E_commerce.Domian;
using E_commerce.Domian.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Drivers.Commands
{
    public class UpdateDriverCommand : IRequest<Result>
    {
        public int UserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class UpdateDriverCommandHandler : IRequestHandler<UpdateDriverCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateDriverCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId.ToString()))
                return Result.Failure("UserId is required.");

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return Result.Failure("Driver not found.");

            if (!await _userManager.IsInRoleAsync(user, RoleSystem.Driver.ToString()))
                return Result.Failure("User is not a driver.");

            if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(request.Username);
                if (existingUser != null && existingUser.Id != user.Id)
                    return Result.Failure("Username is already taken.");

                user.UserName = request.Username;
                user.Email = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = request.PhoneNumber;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to update driver: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                await _userManager.RemovePasswordAsync(user);
                var result = await _userManager.AddPasswordAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to update password: {errors}");
                }
            }

            return Result.Success();
        }
    }
}