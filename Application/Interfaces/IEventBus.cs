using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    
        public interface IEventBus
        {
            Task Publish<T>(string queue, T message);
            void Subscribe<T>(string queue, Func<T, Task> handler);
        }

    
}
