﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace WebServices.Data;

public partial class Consultories
{
    public int Id_Consultory { get; set; }

    public string Name { get; set; }

    public string Latitude { get; set; }

    public string Length { get; set; }

    public string Email { get; set; }

    public int? fk_Municipality { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();

    public virtual Municipalities fk_MunicipalityNavigation { get; set; }
}