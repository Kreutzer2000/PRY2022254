using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data;
using System.Collections;

namespace CapaDatos
{
    public class CD_Resultado
    {

        /* FUNCION PARA REGISTRAR EL RESULTADO DE MI CUESTIONARIO */
        public int RegistrarResultado(Usuario usuario, out string Mensaje, out string codigo)
        {
            int idautogenerado = 0;
            codigo = string.Empty;
            Mensaje = string.Empty;

            try
            {
                using(SqlConnection con = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarResultado_Cliente", con);
                    cmd.Parameters.AddWithValue("idUsuario", usuario.idUsuario);
                    cmd.Parameters.Add("resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("codigo", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    idautogenerado = Convert.ToInt32(cmd.Parameters["resultado"].Value);
                    codigo = cmd.Parameters["codigo"].Value.ToString();
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();
                }
            }
            catch (Exception e)
            {
                codigo = "";
                idautogenerado = 0;
                Mensaje = e.Message;
            }


            return idautogenerado;
        }

        /* LISTAR RESULTADOS PARA UN FILTRO POSTERIOR CON LA TABLA RESUMEN*/
        public List<Resultado> ListarResultados(int idUsuario)
        {
            List<Resultado> resultados = new List<Resultado>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    string query = "select idResultado, idUsuario, porcentajeTotal, fechaResultado, codigo from Resultado where idUsuario = @id";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(
                                new Resultado()
                                {
                                    idResultado = Convert.ToInt32(reader["idResultado"]),
                                    oUsuario = new Usuario() { idUsuario = Convert.ToInt32(reader["idUsuario"]) },
                                    porcentajeTotal = (float)Convert.ToDouble(reader["porcentajeTotal"]),
                                    fechaResultado = Convert.ToDateTime(reader["fechaResultado"]),
                                    codigo = reader["codigo"].ToString(),
                                });
                        }
                    }
                }
            }
            catch
            {
                resultados = new List<Resultado>();
            }
            return resultados;
        }

    }
}
