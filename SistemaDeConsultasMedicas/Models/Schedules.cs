﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SistemaDeConsultasMedicas.Models;

public partial class Schedules
{
    public int Id_Schedule { get; set; }

    public string Schedule_Name { get; set; }

    public TimeOnly From { get; set; }

    public TimeOnly To { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<Medical_Appointments> Medical_Appointments { get; set; } = new List<Medical_Appointments>();

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}