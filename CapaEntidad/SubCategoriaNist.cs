using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class SubCategoriaNist
    {
        public int idSubCategoria { get; set; }
        public string codigo { get; set; }
        public string subCategoria { get; set; }
        public CategoriaNist oCategoria { get; set; }
    }
}
