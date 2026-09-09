using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public delegate TEventResult EventHandler<TEventArgs, TEventResult>(object sender, TEventArgs e);
    public delegate Task EventHandlerAsync<TEventArgs>(object sender, TEventArgs e);
    public delegate Task<TEventResult> EventHandlerAsync<TEventArgs, TEventResult>(object sender, TEventArgs e);
}
