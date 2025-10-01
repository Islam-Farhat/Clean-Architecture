using E_commerce.Domian.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Configurations
{
    public class HousemaidMapping : IEntityTypeConfiguration<Housemaid>
    {
        public void Configure(EntityTypeBuilder<Housemaid> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
