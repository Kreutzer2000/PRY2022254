﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Rol
    {
        private CD_Rol objRol = new CD_Rol();
        public List<Rol> ListarRol()
        {
            return objRol.ListarRol();
        }
    }
}
