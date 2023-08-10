using ISC.Core.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF.ModelsConfigurations
{
	internal class CampConfiguration : IEntityTypeConfiguration<Camp>
	{
		public void Configure(EntityTypeBuilder<Camp> builder)
		{

		}
	}
}
