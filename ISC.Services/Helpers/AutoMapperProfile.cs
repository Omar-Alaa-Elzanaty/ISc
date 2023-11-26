using AutoMapper;
using ISC.Core.APIDtos;
using ISC.Core.Dtos;
using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Services.Helpers
{
	public class AutoMapperProfile:Profile
	{
        public AutoMapperProfile()
        {
            CreateMap<NewRegistration, RegisterDto>();
            CreateMap<Camp, DisplayCampsDto>();
            CreateMap<SessionDto, Session>();
            CreateMap<SheetDto, Sheet>();
            CreateMap<MaterialDto,Material>();
        }
    }
}
