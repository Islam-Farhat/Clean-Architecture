using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Features.DataEntry.Dtos
{
    public class GetDataEntryDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
    }
}
