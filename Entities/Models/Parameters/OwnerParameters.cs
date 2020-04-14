using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models.Parameters
{
    public class OwnerParameters : QueryStringParameters 
    {
        public OwnerParameters()
        {
            OrderBy = "Name";
        }

        //Since the default uint value is 0, we don’t need to explicitly define MinYearOfBirth,
        public uint MinYearOfBirth { get; set; }
        public uint MaxYearOfBirth { get; set; } = (uint)DateTime.Now.Year;
        public bool ValidYearRange => MaxYearOfBirth > MinYearOfBirth;
        public string Name { get; set; }
    }

}
