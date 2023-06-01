using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace PRY2022254.PresentacionAdmin.Utils
{
    public class NotificacionesHub : Hub
    {
        // Esta lista almacenará la asociación entre los usuarios y sus IDs de conexión
        private static readonly Dictionary<string, string> UserConnectionIds = new Dictionary<string, string>();

        public override Task OnConnected()
        {
            // Obtener el correo del usuario a partir del ticket de autenticación
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null)
                {
                    string user = authTicket.Name;

                    if (!UserConnectionIds.ContainsKey(user))
                    {
                        UserConnectionIds.Add(user, Context.ConnectionId);
                    }
                }
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null)
                {
                    string user = authTicket.Name;

                    if (UserConnectionIds.ContainsKey(user))
                    {
                        UserConnectionIds.Remove(user);
                    }
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null)
                {
                    string user = authTicket.Name;

                    if (!UserConnectionIds.ContainsKey(user))
                    {
                        UserConnectionIds.Add(user, Context.ConnectionId);
                    }
                }
            }

            return base.OnReconnected();
        }

        public static void EnviarMensaje(string correo, string mensaje)
        {
            //var hubContext = GlobalHost.DependencyResolver.Resolve<IHubContext<NotificacionesHub>>();
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificacionesHub>();
            if (context == null)
            {
                return;
            }
            else
            {
                if (UserConnectionIds.ContainsKey(correo))
                {
                    var connectionId = UserConnectionIds[correo];
                    //hubContext.Clients.Client(connectionId).RecibirNotificacion(mensaje);
                    context.Clients.Client(connectionId).RecibirNotificacion(mensaje);
                }
            }
        }

        public Task RecibirNotificacion(string mensaje)
        {
            return Clients.Caller.SendAsync("RecibirNotificacion", mensaje);
        }
    }
}