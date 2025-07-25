﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CarritoC.Models
{
    public class Rol : IdentityRole<int>
    {
            //public int Id { get; set; }

            #region Constructores
            public Rol() : base() { }

            public Rol(string name) : base(name) { }
        #endregion

        #region Propiedades

        [Display(Name = "Role Name")]
            public string Name
            {
                get { return base.Name; }
                set { base.Name = value; }
            }

            public override string NormalizedName
            {
                get => base.NormalizedName;
                set => base.NormalizedName = value;
            }

            #endregion
        }
    }
