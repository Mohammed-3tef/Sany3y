using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.ViewModels;

namespace Sany3y.Infrastructure.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Category> Categories { get; set; } = new();
    }
}
