using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChecksumAPI
{
    class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CADbContext>
    {
        public CADbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CADbContext>();
            optionsBuilder.UseSqlite("Data Source=checksum.db", sqlOptions => sqlOptions.MigrationsAssembly(typeof(CADbContext).GetTypeInfo().Assembly.GetName().Name));
            return new CADbContext(optionsBuilder.Options);
        }
    }
}
