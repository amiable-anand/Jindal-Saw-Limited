using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Jindal.Models
{
    public class Location
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }
}
