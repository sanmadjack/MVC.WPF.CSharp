using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace MVC.WPF {
    public class ThreadBridge: IThreadBridge {
        public SynchronizationContext context {
            get;
            protected set;
        }

        public ThreadBridge(SynchronizationContext context) {
            this.context = context;
        }

        public void Send(CommunicationDelegate send_me) {
            context.Send((delegate(object obj) {
                send_me();
            }),null);
        }
        public void Post(CommunicationDelegate send_me) {
            context.Post((delegate(object obj) {
                send_me();
            }), null);
        }
    }
}
