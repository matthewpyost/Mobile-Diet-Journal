using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.ServiceModel
{
    public static class ICommunicationObjectExtensions
    {
        public static TResult SafeExecute<T,TResult>(this T client, Func<T, TResult> function)
            where T : ICommunicationObject
        {
            TResult result = default(TResult);

            try
            {
                result = function.Invoke(client);
            }
            finally
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    try
                    {
                        client.Close();
                    }
                    catch (CommunicationObjectFaultedException)
                    {
                        client.Abort();
                    }
                    catch (TimeoutException)
                    {
                        client.Abort();
                    }
                }
            }

            return result;

        }

        public static void SafeExecute<T>(this T client, Action<T> work)
            where T : ICommunicationObject
        {
            try
            {
                work(client);
                client.Close();
            }
            finally
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    try
                    {
                        client.Close();
                    }
                    catch (CommunicationObjectFaultedException)
                    {
                        client.Abort();
                    }
                    catch (TimeoutException)
                    {
                        client.Abort();
                    }
                }
            }
        }


    }
}