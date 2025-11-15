using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Shared.DTOs
{
    public class UpdateCompanyDto
    {
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public IFormFile? Logo { get; set; }
    }
}
