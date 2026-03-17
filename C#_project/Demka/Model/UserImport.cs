using System;
using System.Collections.Generic;

namespace Demka.Model;

public partial class UserImport
{
    public int IdUserImport { get; set; }

    public string RoleUserImport { get; set; } = null!;

    public string SurnameUserImport { get; set; } = null!;

    public string NameUserImport { get; set; } = null!;

    public string LastnameUserImport { get; set; } = null!;

    public string LoginUserImport { get; set; } = null!;

    public string PasswordUserImport { get; set; } = null!;

    public string userRole
    {
        get { return $"Ваша роль: {RoleUserImport}"; }
    }
    public virtual ICollection<OrderImport> OrderImports { get; set; } = new List<OrderImport>();
}
