using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class IXActionController
    {
        public abstract Task DoActionAsync(); //{  return Task.CompletedTask; }  

        public virtual bool ShouldBeVisible() { return true; } 
    }
}
