using System.Diagnostics.Metrics;

namespace DistributedCache.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Audit : Attribute
    {
        
        private string _parentTable;
        public Audit()
        {
            //this._parentTable = parentTable;
        }

        public void SaveLog()
        {
            //new ApplicationContext().Configure(opt => opt.AddService("logger").EnableLogging());
        }
    } 
}
