using System;
using System.Collections.Generic;

namespace Mud_Q_Sass.API.Models;

public partial class UserRole
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public int? CompanyId { get; set; }

    public int? BranchId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
