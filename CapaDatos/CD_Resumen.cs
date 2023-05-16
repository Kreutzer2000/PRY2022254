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
    public class CD_Resumen
    {
        /* FUNCION PARA GUARDAR EL RESUMEN DEL CLIENTE */
        public int RegistrarResumen(string codigo, int idResultado, int idPuntaje_Actual, int idPregunta, int idRptaPreguntas, int idPuntaje_Deseado, int idUrgencia, out string Mensaje)
        {
            int idautogenerado = 0;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarResumen_Cliente", con);
                    cmd.Parameters.AddWithValue("codigo", codigo);
                    cmd.Parameters.AddWithValue("idResultado", idResultado);
                    cmd.Parameters.AddWithValue("idPuntaje_Actual", idPuntaje_Actual);
                    cmd.Parameters.AddWithValue("idPregunta", idPregunta);
                    cmd.Parameters.AddWithValue("idRptaPreguntas", idRptaPreguntas);
                    cmd.Parameters.AddWithValue("idPuntaje_Deseado", idPuntaje_Deseado);
                    cmd.Parameters.AddWithValue("idUrgencia", idUrgencia);
                    cmd.Parameters.Add("resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
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

        /* FUNCION PARA LISTAR RESUMENES AGRUPADOS */
        public List<Resumen> ListarResumen()
        {
            List<Resumen> resumenes = new List<Resumen>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_ListarResumenes", oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resumenes.Add(
                                new Resumen()
                                {
                                    //idResumen = Convert.ToInt32(reader["idResumen"]),
                                    codigo = reader["codigo"].ToString(),
                                    fechaResumen = Convert.ToDateTime(reader["fechaResultado"]).ToString("dd/MM/yyyy"),
                                    oResultado = new Resultado()
                                    {
                                        idResultado = Convert.ToInt32(reader["idResultado"]),
                                        oUsuario = new Usuario()
                                        {
                                            idUsuario = Convert.ToInt32(reader["idUsuario"]),
                                            nombres = reader["nombres"].ToString(),
                                            apellidos = reader["apellidos"].ToString(),
                                            email = reader["email"].ToString()
                                        }
                                    },
                                });
                        }
                    }
                }
            }
            catch
            {
                resumenes = new List<Resumen>();
            }

            return resumenes;
        }

        /* FUNCION PARA LISTAR RESUMENES AGRUPADOS Y FILTRADOS POR EL CORREO DEL CLIENTE PARA LA VISTA DEL CLIENTE */
        public List<Resumen> ListarResumen_Cliente(string correo)
        {
            List<Resumen> resumenes = new List<Resumen>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_ListarResumenes_Correo", oconexion);
                    cmd.Parameters.AddWithValue("correo", correo);
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resumenes.Add(
                                new Resumen()
                                {
                                    //idResumen = Convert.ToInt32(reader["idResumen"]),
                                    codigo = reader["codigo"].ToString(),
                                    fechaResumen = Convert.ToDateTime(reader["fechaResultado"]).ToString("dd/MM/yyyy"),
                                    oResultado = new Resultado()
                                    {
                                        idResultado = Convert.ToInt32(reader["idResultado"]),
                                        oUsuario = new Usuario()
                                        {
                                            idUsuario = Convert.ToInt32(reader["idUsuario"]),
                                            nombres = reader["nombres"].ToString(),
                                            apellidos = reader["apellidos"].ToString(),
                                            email = reader["email"].ToString()
                                        }
                                    },
                                });
                        }
                    }
                }
            }
            catch
            {
                resumenes = new List<Resumen>();
            }

            return resumenes;
        }

        /* FUNCION PARA LISTAR TODA LA TABLA RESUMEN PERO FILTRADO POR EL CODIGO */
        public List<Resumen> ListarResumenPorCodigo(string codigo)
        {
            List<Resumen> resumenes = new List<Resumen>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_ListarResumenesPorCodigo", oconexion);
                    cmd.Parameters.AddWithValue("codigo", codigo);
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resumenes.Add(
                                new Resumen()
                                {
                                    idResumen = Convert.ToInt32(reader["idResumen"]),
                                    codigo = reader["codigo"].ToString(),
                                    fechaResumen = Convert.ToDateTime(reader["fechaResultado"]).ToString("dd/MM/yyyy"),
                                    oResultado = new Resultado()
                                    {
                                        idResultado = Convert.ToInt32(reader["idResultado"]),
                                        oUsuario = new Usuario()
                                        {
                                            idUsuario = Convert.ToInt32(reader["idUsuario"]),
                                            nombres = reader["nombres"].ToString(),
                                            apellidos = reader["apellidos"].ToString(),
                                            email = reader["email"].ToString()
                                        }
                                    },
                                    oPuntajeActual = new Puntaje() { idPuntaje = Convert.ToInt32(reader["idPuntaje_Actual"]) },
                                    oPregunta = new Preguntas()
                                    {
                                        idPregunta = Convert.ToInt32(reader["idPregunta"]),
                                        oSubCategoria = new SubCategoriaNist()
                                        {
                                            idSubCategoria = Convert.ToInt32(reader["idSubCategoria"]),
                                            subCategoria = reader["subCategoria"].ToString(),
                                            oCategoria = new CategoriaNist()
                                            {
                                                idCategoria = Convert.ToInt32(reader["idCategoria"]),
                                                categoria = reader["categoria"].ToString(),
                                                oFuncion = new FuncionNist()
                                                {
                                                    idFuncion = Convert.ToInt32(reader["idFuncion"]),
                                                    codigo = reader["codigoFuncion"].ToString(),
                                                    funcion = reader["funcion"].ToString()
                                                }
                                            }
                                        }
                                    },
                                    oRptaPreguntas = new RptaPreguntas() { idRptaPregunta = Convert.ToInt32(reader["idRptaPreguntas"]), 
                                        respuesta = reader["respuesta"].ToString() },
                                    oPuntajeDeseado = new Puntaje() { idPuntaje = Convert.ToInt32(reader["idPuntaje_Deseado"]) },
                                    oNivelUrgencia = new NivelUrgencia { idUrgencia = Convert.ToInt32(reader["idUrgencia"]), 
                                        nivel = (float)Convert.ToDouble(reader["nivel"]) }
                                });
                        }
                    }
                }
            }
            catch
            {
                resumenes = new List<Resumen>();
            }

            return resumenes;
        }
    }
}
