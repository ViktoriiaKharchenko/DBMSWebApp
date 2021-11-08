﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DBMSWebApp.Models
{
    public class Database
    {
        public Database()
        {
            Tables = new List<Table>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Table> Tables { get; set; }
    }
}
