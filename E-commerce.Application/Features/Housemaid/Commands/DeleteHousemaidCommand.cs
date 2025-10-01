using CSharpFunctionalExtensions;
using E_commerce.Application.Interfaces;
using E_commerce.Domian.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.Housemaid.Commands
{
    public class DeleteHousemaidCommand : IRequest<Result>
    {
        public int Id { get; set; }

        public class DeleteHousemaidCommandHandler : IRequestHandler<DeleteHousemaidCommand, Result>
        {
            private readonly IMediaService _mediaService;
            private readonly IGetCleanerContext _context;
            public DeleteHousemaidCommandHandler(IMediaService mediaService, IGetCleanerContext context)
            {
                _mediaService = mediaService;
                _context = context;
            }
            public async Task<Result> Handle(DeleteHousemaidCommand request, CancellationToken cancellationToken)
            {
                var housemaid = await _context.Housemaids.FirstOrDefaultAsync(x => x.Id == request.Id);

                if (housemaid == null)
                    return Result.Failure("Housemaid not found!");


                _context.Housemaids.Remove(housemaid);
                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>("Save Error");

                return Result.Success();

            }
        }
    }
}
