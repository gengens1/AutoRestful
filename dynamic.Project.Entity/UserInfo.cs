﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace dynamic.Project.Entity
{
    public class UserInfo : EntityBase
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string NickName { get; set; }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
