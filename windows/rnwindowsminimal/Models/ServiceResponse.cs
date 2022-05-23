using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rnwindowsminimal.Models
{
    public class ServiceName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class ServiceResponse
    {
        public ServiceResponse()
        {
            this.Message = new List<string>();
            this.Valid = false;
        }

        public bool Valid { get; set; }

        public List<string> Message { get; set; }


    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public ServiceResponse()
        {
            if (typeof(T).IsValueType || typeof(T) == typeof(String))
            {
                this.Content = default(T);
            }
            else
            {
                this.Content = (T)Activator.CreateInstance(typeof(T));
            }
        }

        public T Content { get; set; }
    }
}
