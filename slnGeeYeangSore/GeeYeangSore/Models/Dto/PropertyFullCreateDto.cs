using System.Collections.Generic;
using GeeYeangSore.Models;

namespace GeeYeangSore.Models.Dto
{
    public class PropertyFullCreateDto
    {
        public HProperty Property { get; set; }
        public HPropertyFeature PropertyFeature { get; set; }
        public List<HPropertyImage> PropertyImages { get; set; }
        public HAd Ad { get; set; }
    }
} 