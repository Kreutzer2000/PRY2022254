using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data;

namespace CapaDatos
{
    public class CD_Respuesta
    {

        /* FUNCION PARA LLENAR EN LA BD's LA RESPUESTA DEL USUARIO CON RESPECTO A SU FUNCION */
        public int RegistrarRespuesta(string correo, int idusuario, int ID, int PR, int DE, int RS, int RC, out string Mensaje)
        {
            int idautogenerado = 0;

            Mensaje = string.Empty;
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarRespuestaCliente", conexion);
                    cmd.Parameters.AddWithValue("correo", correo);
                    cmd.Parameters.AddWithValue("idusuario", idusuario);
                    cmd.Parameters.AddWithValue("ID", ID);
                    cmd.Parameters.AddWithValue("PR", PR);
                    cmd.Parameters.AddWithValue("DE", DE);
                    cmd.Parameters.AddWithValue("RS", RS);
                    cmd.Parameters.AddWithValue("RC", RC);
                    cmd.Parameters.Add("resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    idautogenerado = Convert.ToInt32(cmd.Parameters["resultado"].Value);
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();

                }
            }
            catch (Exception e)
            {
                idautogenerado = 0;
                Mensaje = e.Message;
            }
            return idautogenerado;
        }

    }
}
