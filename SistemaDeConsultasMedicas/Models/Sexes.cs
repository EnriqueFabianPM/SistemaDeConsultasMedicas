﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SistemaDeConsultasMedicas.Models;

public partial class Sexes
{
    public int Id_Sex { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}