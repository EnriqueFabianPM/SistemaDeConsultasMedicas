﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SistemaDeConsultasMedicas.Models;

public partial class Types
{
    public int Id_Type { get; set; }

    public string Name { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}