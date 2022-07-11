namespace Dorsavi.Win.Bluetooth.Models
{
    //public class ServiceResponse
    //{
    //    public ServiceResponse()
    //    {
    //        this.Message = new List<string>();
    //        this.Valid = false;
    //    }

    //    public bool Valid { get; set; }

    //    public List<string> Message { get; set; }
    //}

    //public class ServiceResponse<T> : ServiceResponse
    //{
    //    public ServiceResponse()
    //    {
    //        if (typeof(T).IsValueType || typeof(T) == typeof(string))
    //        {
    //            this.Content = default(T);
    //        }
    //        else
    //        {
    //            this.Content = (T)Activator.CreateInstance(typeof(T));
    //        }
    //    }

    //    public T Content { get; set; }
    //}

    //public static class ServiceResponseExtensions
    //{
    //    public static T ToInvalidRequest<T>(this T response, string message) where T : ServiceResponse
    //    {
    //        //var response = (T)Activator.CreateInstance(typeof(T));
    //        response.Message.Add(message);
    //        response.Valid = false;
    //        return response;
    //    }

    //    public static T CopyFrom<T>(this T response, ServiceResponse source) where T : ServiceResponse
    //    {
    //        response.Message = source.Message;
    //        response.Valid = source.Valid;
    //        return response;
    //    }

    //    public static TResponse ValidateModel<TResponse, TRequest>(this TRequest obj)
    //        where TRequest : class
    //        where TResponse : ServiceResponse
    //    {
    //        var response = (TResponse)Activator.CreateInstance(typeof(TResponse));

    //        foreach (var prop in obj.GetType().GetProperties()
    //            .Where(prop => prop.IsDefined(typeof(RequiredAttribute), true)).ToList())
    //        {
    //            var value = prop.GetValue(obj, null);
    //            if (value == null || value.ToString().Length == 0)
    //            {
    //                response.Message.Add($"{prop.Name} is required");
    //                response.Valid = false;
    //            }
    //        }
    //        foreach (var prop in obj.GetType().GetProperties()
    //            .Where(prop => prop.IsDefined(typeof(EmailAddressAttribute), true)).ToList())
    //        {
    //            if (!new EmailAddressAttribute().IsValid(prop.GetValue(obj, null)))
    //            {
    //                response.Message.Add($"Invalid email format");
    //                response.Valid = false;
    //            }
    //        }

    //        return response;
    //    }

    //    public static ServiceResponse<List<TSource>> CombineResponseContent<TSource>(this List<ServiceResponse<TSource>> responses)
    //    {
    //        var payloads = responses.ToList();
    //        var baseResponse = CombineResponse(payloads);

    //        return new ServiceResponse<List<TSource>>
    //        {
    //            Content = payloads.Select(x => x.Content).ToList(),
    //            Message = baseResponse.Message,
    //            Valid = baseResponse.Valid,
    //        };
    //    }

    //    public static ServiceResponse<List<TResult>> CombineResponseContent<TSource, TResult>(
    //        this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
    //        where TSource : ServiceResponse
    //    {
    //        var payloads = source.ToList();
    //        var baseResponse = CombineResponse(payloads);

    //        var response = new ServiceResponse<List<TResult>>()
    //        {
    //            //Content = payloads.Select(x => x.Content).ToList(),
    //            Message = baseResponse.Message,
    //            Valid = baseResponse.Valid,
    //        };
    //        foreach (TSource element in source)
    //        {
    //            foreach (TResult subElement in selector(element))
    //            {
    //                response.Content.Add(subElement);
    //            }
    //        }

    //        return response;
    //    }

    //    /// <summary>
    //    /// Combine multiple responses into a single base response excluding the content if any
    //    /// </summary>
    //    /// <param name="responses"></param>
    //    /// <returns></returns>
    //    public static ServiceResponse CombineResponse(this IEnumerable<ServiceResponse> responses)
    //    {
    //        var payload = responses.ToList();
    //        if (!payload.Any())
    //        {
    //            throw new ArgumentNullException();
    //        }

    //        var messages = payload.SelectMany(x => x.Message).ToList();
    //        var resp = new ServiceResponse()
    //        {
    //            Message = messages.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()
    //        };

    //        return resp;
    //    }
    //}
}