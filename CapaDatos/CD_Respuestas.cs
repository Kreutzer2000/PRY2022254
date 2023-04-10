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
    public class CD_Respuestas
    {
        /* LISTAR RESPUESTAS Y PREGUNTAS */
        public List<RptaPreguntas> ListarRespuesta(int idpregunta)
        {
            List<RptaPreguntas> lista = new List<RptaPreguntas>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    using (SqlCommand _sqlcmd = new SqlCommand("sp_Lista_respuestas_preguntas", oconexion))
                    {
                        oconexion.Open();
                        _sqlcmd.CommandTimeout = 0;
                        _sqlcmd.CommandType = CommandType.StoredProcedure;

                        _sqlcmd.Parameters.Add("@idpregunta", SqlDbType.Int).Value = idpregunta;

                        SqlDataReader Reader = _sqlcmd.ExecuteReader();
                        while (Reader.Read())
                        {
                            //Preguntas pregunta = new Preguntas();
                            RptaPreguntas objRptaPreguntas = new RptaPreguntas()
                            {
                                idRptaPregunta = int.Parse(Reader["idRptaPreguntas"].ToString()),
                                respuesta = Reader["respuesta"].ToString(),
                                oPregunta = new Preguntas() { 
                                    idPregunta = Convert.ToInt32(Reader["idPregunta"]), 
                                    pregunta = Reader["pregunta"].ToString(),
                                    oSubCategoria = new SubCategoriaNist()
                                    {
                                        idSubCategoria = Convert.ToInt32(Reader["idSubCategoria"]),
                                        subCategoria = Reader["subCategoria"].ToString(),
                                        oCategoria = new CategoriaNist()
                                        {
                                            idCategoria = Convert.ToInt32(Reader["idCategoria"]),
                                            categoria = Reader["categoria"].ToString(),
                                            oFuncion = new FuncionNist()
                                            {
                                                idFuncion = Convert.ToInt32(Reader["idFuncion"]),
                                                funcion = Reader["funcion"].ToString()
                                            }
                                        }
                                    }
                                },
                                oFuncionNist = new FuncionNist() { idFuncion = Convert.ToInt32(Reader["idFuncion"]), funcion = Reader["funcion"].ToString() }
                            };

                            lista.Add(objRptaPreguntas);
                        }
                        oconexion.Close();
                        return lista;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return lista;
            }
            
        }

        /* LISTAR PREGUNTAS */
        public List<RptaPreguntas> ListarPreguntas()
        {
            List<RptaPreguntas> lista = new List<RptaPreguntas>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.CadenaConexion))
                {
                    using (SqlCommand _sqlcmd = new SqlCommand("sp_Lista_preguntas", oconexion))
                    {
                        oconexion.Open();
                        _sqlcmd.CommandTimeout = 0;
                        _sqlcmd.CommandType = CommandType.StoredProcedure;

                        SqlDataReader Reader = _sqlcmd.ExecuteReader();
                        while (Reader.Read())
                        {
                            //Preguntas pregunta = new Preguntas();
                            RptaPreguntas objRptaPreguntas = new RptaPreguntas()
                            {
                                oPregunta = new Preguntas()
                                {
                                    idPregunta = Convert.ToInt32(Reader["idPregunta"]),
                                    pregunta = Reader["pregunta"].ToString(),
                                    oSubCategoria = new SubCategoriaNist()
                                    {
                                        idSubCategoria = Convert.ToInt32(Reader["idSubCategoria"]),
                                        subCategoria = Reader["subCategoria"].ToString(),
                                        oCategoria = new CategoriaNist()
                                        {
                                            idCategoria = Convert.ToInt32(Reader["idCategoria"]),
                                            categoria = Reader["categoria"].ToString(),
                                            oFuncion = new FuncionNist()
                                            {
                                                idFuncion = Convert.ToInt32(Reader["idFuncion"]),
                                                funcion = Reader["funcion"].ToString()
                                            }
                                        }
                                    }
                                },
                                oFuncionNist = new FuncionNist() { idFuncion = Convert.ToInt32(Reader["idFuncion"]), funcion = Reader["funcion"].ToString() }
                            };

                            lista.Add(objRptaPreguntas);
                        }
                        oconexion.Close();
                        return lista;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return lista;
            }

        }

    }
}
