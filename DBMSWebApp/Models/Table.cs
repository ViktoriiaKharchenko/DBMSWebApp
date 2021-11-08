﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMSWebApp.Models
{
    public class Table
    {
        public Table()
        {
            Columns = new List<Column>();
            Rows = new List<Row>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int DatabaseId { get; set; }
        public Database Database { get; set; }
        public virtual ICollection<Column> Columns { get; set; }
        public virtual ICollection<Row> Rows { get; set; }
    }
}
