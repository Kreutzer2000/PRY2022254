using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PRY2022254.PresentacionAdmin.Utils
{
    public class NotificacionesHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
        public static void EnviarMensaje(string mensaje)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificacionesHub>();
            hubContext.Clients.All.mostrarMensaje(mensaje);
        }
    }
}