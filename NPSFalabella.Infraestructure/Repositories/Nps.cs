using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NPSFalabella.Core.Entities;
using NPSFalabella.Core.Interfaces;
using NPSFalabella.Infraestructure.Data;
using Serilog;
using ServiceNPS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace NPSFalabella.Infraestructure.Repositories
{
    public class Nps : INps
    {
        public string conn = "Server=OCC3BDP1401;Database=B2208_BCO_FALABELLA;Persist Security Info=True;User ID=usraccmw;Password=inc2001";
        private readonly DbContextApp _context;
        private readonly ILogger<Nps> _logger;
        private readonly IConfiguration _configuration;
        public Nps(DbContextApp context ,ILogger<Nps> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<Registro>> ListarTramas()
        {
            try
            {
                var sql = "spGSS_Ap_ListarTramasNPS";
                var parameters = new DynamicParameters();
                using (IDbConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    var result = await connection.QueryAsync<Registro>(sql, parameters, commandType: CommandType.StoredProcedure);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed Listar Trama", new Exception(ex.Message));

                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> UpdateTrama(string ContactId, int Procesado, string Resultado)
        {
            try
            {
                var sql = "spGSS_Ap_UpdateTramasNPS";
                var parameters = new DynamicParameters();
                parameters.Add("@ContactId", ContactId, DbType.String);
                parameters.Add("@Procesado", Procesado, DbType.Int32);
                parameters.Add("@Resultado", Resultado, DbType.String);
                using (IDbConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    var result = await connection.ExecuteAsync(sql, parameters, commandType: CommandType.StoredProcedure);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed Update Trama ContactId [" + ContactId + "]");
                throw new Exception(ex.Message);
            }
        }
       

        public async Task<bool> ProcesarTrama()
        {
            try
            {
                var asas = await ListarTramas();
                CallClient client = new CallClient();
                //client.ClientCredentials.UserName.UserName = _configuration["AppKeys:ApiUser"];
                //client.ClientCredentials.UserName.Password = _configuration["AppKeys:ApiPass"];
                client.ClientCredentials.UserName.UserName = "user_falabella";
                client.ClientCredentials.UserName.Password = "$Netvox321x$_2022";
                registrarResponse response = new registrarResponse();
                foreach (var item in asas)
                {
                    try
                    {
                        using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
                        {
                            var httpRequestProperty = new HttpRequestMessageProperty();
                            httpRequestProperty.Headers[System.Net.HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(client.ClientCredentials.UserName.UserName + ":" + client.ClientCredentials.UserName.Password));
                            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                            item.ges_id = item.ges_id.Substring(0, 10); 
                            response = await client.registrarAsync(item.ges_id, item.ges_campana, item.ges_tipo_documento, item.ges_numero_documento, item.ges_fec_hor, item.pr_nombres_cliente, item.pr_apellidos_cliente, item.desc_operador, item.attributeconnid, item.mensaje,item.base_pto_contacto,item.ges_tipo_llamada,item.ges_tipo_llamada_desc,item.id_res_cod,item.res_desc,item.codigo_del_servicio,item.descripcion_corta_servicio,item.cod_detalle_servicio,item.descripcion_corta_detalle_serv,item.telefono,item.campana_ivrvox,item.identificador_empresa,item.identificador_agente,item.nombre_coordinador,item.cod_personal);
                            if (response.Body.@return == "Datos Registrados")
                            {
                                await UpdateTrama(item.ContactId,1,response.Body.@return);
                            }
                            else
                            {
                             
                                await UpdateTrama(item.ContactId, 0, response.Body.@return);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed Call Api ContactId [" + item.ContactId + "]");
                    }
                }
                await client.CloseAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
