using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using TrainTracker.Web.Models;
using Newtonsoft.Json;
using TrainTracker.Helpers;

namespace TrainTracker.Web.Helpers
{
    public abstract class PagedApiControler<TRequest, TResponce> : ApiController
        where TRequest : PagedApiControler<TRequest, TResponce>.RequestData, new()
    {
        protected abstract string TokenKey { get; }

        protected virtual int MaxResults { get; } = 100;
        [HttpGet]
        public PagedList<TResponce> Get([FromUri] string nextPageToken)
        {
            TRequest data;
            if (!CheckToken(nextPageToken, out data)) return null; // Token Check Failed, return null;
            
            return GetData(data);
        }
        [HttpGet]
        public PagedList<TResponce> Get([FromUri] TRequest data)
        {
            return GetData(data);
        }

        [HttpPost]
        public PagedList<TResponce> Post([FromBody] TRequest data)
        {
            return GetData(data);
        }

        protected PagedList<TResponce> PageData(TRequest requestData, IEnumerable<TResponce> data)
        {
            var dataList = ProcessPagedData(data
                .Skip((requestData.PageNumber - 1)*requestData.MaxItems)
                .Take(Math.Min(requestData.MaxItems, 100)))
                .ToList();
            return new PagedList<TResponce>
            {
                Data = dataList,
                Returned = dataList.Count,
                NextPageToken = CreateToken(requestData),
                PageNumber = requestData.PageNumber
            };
        }

        protected virtual IEnumerable<TResponce> ProcessPagedData(IEnumerable<TResponce> enumerable)
        {
            return enumerable;
        }

        protected abstract PagedList<TResponce> GetData(TRequest data); 


        protected string CreateToken(TRequest requestData)
        {
            requestData.PageNumber++;
            var tokenData = JsonConvert.SerializeObject(requestData);
            var tokenDataList = new []
            {
                DateTime.Now.ToString("s"),
                tokenData
            };
            var data = string.Join("|", tokenDataList.ToArray());
            var cryptedText = StringCipher.Encrypt(data, TokenKey); 
            return cryptedText.Replace('/', '_').Replace(" ", "*");
        }

        protected bool CheckToken(string nextPageToken, out TRequest data)
        {
            try
            {
                var dataString = StringCipher.Decrypt(nextPageToken.Replace('_', '/').Replace(" ", "+").Replace("*", " "), TokenKey);
                if (dataString.Contains("|"))
                {
                    var values = dataString.Split('|');
                    if (values.Length == 2)
                    {
                        if (DateTime.Now.AddDays(1) > DateTime.Parse(values[0]))
                        {
                            //Data is Valid and has not expired
                            data = JsonConvert.DeserializeObject<TRequest>(values[1]);
                            return true;
                        }
                    }
                }
                data = null;
                return false;
            }
            catch (Exception)
            {
                data = null;
                return false;
            }
            
        }

        public abstract class RequestData
        {
            protected RequestData()
            {
                PageNumber = 1;
                MaxItems = 25;
            }
            public int MaxItems { get; set; }

            [JsonProperty]
            internal int PageNumber { get; set; }

        }

    }
}