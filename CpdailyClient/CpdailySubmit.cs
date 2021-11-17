using Cpdaily;
using Cpdaily.CpdailyModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily
{
    public partial class CpdailyClient
    {
        /// <summary>
        /// 获取未完成的表单项目
        /// </summary>
        /// <param name="cookie">用于访问校内应用的 Cookie</param>
        /// <returns>FormItem[]</returns>
        public async Task<FormItem[]> GetFormItemsAsync(string ampUrlPrefix, string cookie)
        {
            string url = $"{ampUrlPrefix}/wec-counselor-collector-apps/stu/collector/queryCollectorProcessingList";
            RestClient client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", cookie);
            request.AddHeader("User-Agent", WebUserAgent);
            request.AddJsonBody(new { pageNumber = 1, pageSize = 20 });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var json = JObject.Parse(response.Content);
            if (json["code"].Value<int>() != 0)
            {
                throw new Exception($"出现错误: {json["message"].Value<string>()}, 错误代码: {json["code"].Value<int>()}。");
            }
            var list = JArray.FromObject(json["datas"]["rows"]);
            return list.ToObject<FormItem[]>();
        }

        /// <summary>
        /// 获取历史的表单项目
        /// </summary>
        /// <param name="cookie">用于访问校内应用的 Cookie</param>
        /// <returns>FormItem[]</returns>
        public async Task<FormItem[]> GetFormItemsHistoryAsync(string ampUrlPrefix, string cookie)
        {
            string url = $"{ampUrlPrefix}/wec-counselor-collector-apps/stu/collector/queryCollectorHistoryList";
            RestClient client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", cookie);
            request.AddHeader("User-Agent", WebUserAgent);
            request.AddJsonBody(new { pageNumber = 1, pageSize = 5 });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var json = JObject.Parse(response.Content);
            if (json["code"].Value<int>() != 0)
            {
                throw new Exception($"出现错误: {json["message"].Value<string>()}, 错误代码: {json["code"].Value<int>()}。");
            }
            var list = JArray.FromObject(json["datas"]["rows"]);
            return list.ToObject<FormItem[]>();
        }

        /// <summary>
        /// 获取表单的字段
        /// </summary>
        /// <param name="cookie">用于访问校内应用的 Cookie</param>
        /// <param name="wid">FormItem.Wid</param>
        /// <param name="formWid">FormItem.FormWid</param>
        /// <returns>FormField[]</returns>
        public async Task<FormField[]> GetFormFieldsAsync(string ampUrlPrefix, string cookie, string wid, string formWid)
        {
            string url = $"{ampUrlPrefix}/wec-counselor-collector-apps/stu/collector/getFormFields";
            RestClient client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", cookie);
            request.AddHeader("User-Agent", WebUserAgent);
            request.AddJsonBody(new
            {
                pageNumber = 1,
                pageSize = 20,
                formWid = formWid,
                collectorWid = wid
            });
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var json = JObject.Parse(response.Content);
            if (json["code"].Value<int>() != 0)
            {
                throw new Exception($"出现错误: {json["message"].Value<string>()}, 错误代码: {json["code"].Value<int>()}。");
            }
            var list = JArray.FromObject(json["datas"]["rows"]);
            return list.ToObject<FormField[]>();
        }

        public static FormField MergeToFormField(FormField origin, FormFieldChange change)
        {
            var originTitle = origin.Title.Trim();
            var changeTitle = change.Title.Trim();
            if (originTitle != changeTitle)
            {
                throw new Exception($@"无法合并修改: ""{originTitle}"" 和 ""{changeTitle}"" 不匹配!");
            }
            var result = (FormField)origin.Clone();
            if (origin.FieldType == 1)         // 填空只需要修改文本值
            {
                result.Value = change.Value;
            }
            else if (origin.FieldType == 2)     // 单选需要修改 Value 为选项Id，且去除不用选项
            {
                result.FieldItems.Clear();
                foreach (var item in origin.FieldItems)
                {
                    if (item.Content == change.Value)
                    {
                        result.Value = item.ItemWid;
                        item.IsSelected = 1;
                        result.FieldItems.Add(item);
                        break;
                    }
                }
            }
            else if (origin.FieldType == 3)      // 多选需要分割默认选项值，且去除不用选项
            {
                result.FieldItems.Clear();
                var values = change.Value.Split(",");
                foreach (var item in origin.FieldItems)
                {
                    foreach (var value in values)
                    {
                        if (item.Content == value)
                        {
                            result.Value += $"{item.Content} ";
                            item.IsSelected = 1;
                            result.FieldItems.Add(item);
                        }
                    }
                }
            }
            else if (origin.FieldType == 4)      // 图片类型
            {
                throw new Exception("暂时不支持图片类型，请在GitHub提出issues请求支持!");
            }
            else
            {
                throw new Exception("未知的FieldType!请检查配置文件是否正确!");
            }
            return result;
        }

        /// <summary>
        /// 提交修改后的表单
        /// </summary>
        /// <param name="cookie">用于访问校内应用的 Cookie</param>
        /// <param name="formItem">通过GetFormItemsAsync()获取</param>
        /// <param name="formFieldsToSubmit">被修改过的FormField</param>
        /// <param name="address">地址</param>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <returns></returns>
        public async Task SubmitForm(string ampUrlPrefix, string cookie, FormItem formItem, FormField[] formFieldsToSubmit, string address, double latitude, double longitude)
        {
            var cpdaily_extension = CpdailyCrypto.DESEncrypt(JsonConvert.SerializeObject(DeviceInfo), "b3L26XNL", CpdailyCrypto.IV);
            var obj = new
            {
                formWid = formItem.FormWId,
                collectWid = formItem.WId,
                schoolTaskWid = (string?)null,
                address = address,
                uaIsCpadaily = true,
                latitude = latitude,
                longitude = longitude,
                form = formFieldsToSubmit
            };
            string objJson = JsonConvert.SerializeObject(obj);
            string url = $"{ampUrlPrefix}/wec-counselor-collector-apps/stu/collector/submitForm";
            RestClient client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", cookie);
            request.AddHeader("User-Agent", WebUserAgent);
            request.AddHeader("extension", "1");
            request.AddHeader("CpdailyStandAlone", "0");
            request.AddHeader("Cpdaily-Extension", cpdaily_extension);
            request.AddParameter("application/json", objJson, ParameterType.RequestBody);
            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("非200状态响应");
            var json = JObject.Parse(response.Content);
            if (json["code"].Value<int>() != 0)
            {
                throw new Exception(json["message"].Value<string>());
            }
        }

    }
}
