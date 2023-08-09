using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace TagManager
{
    public class CustomTranslator : AbstractTranslator
    {

        private HttpClient client = new HttpClient();
        public CustomTranslator() : base(TranslationService.CustomTranslate)
        {
            client = new HttpClient();
        }

        public override async Task<string> TranslateAsync(string text, string fromLang, string toLang)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var result = await TranslateAsync(new CancellationToken(), new List<string>() { text }, fromLang, toLang);
            if (result.ContainsKey(text))
            {
                return result[text];
            }
            return null;
        }

        public override async Task<Dictionary<string, string>> TranslateAsync(CancellationToken cancellationToken, IEnumerable<string> contentList, string fromLang = "en", string toLang = "zh-CN")
        {
            var list = contentList.ToList();
            if (list == null || list.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            int emptyCount = 0;
            var emptyIndexList = new List<int>();
            var resultList = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(list[i]) || (list[i].Length == 1 && !IsChinese(list[0])))
                {
                    emptyCount++;
                    emptyIndexList.Add(i);
                }
                else
                {
                    resultList.Add(list[i]);
                    emptyIndexList.Add(-1);
                }
            }

            if (emptyCount == list.Count)
            {
                return new Dictionary<string, string>();
            }

            string url = $"http://aitool.mm.babybus.com/Transalte/Index?content={HttpUtility.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(resultList))}&translateType={HttpUtility.UrlEncode(toLang)}";

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            string contentResult;

            try
            {
                //await Task.Delay(3000);
                var contentResultTask = client.PostAsync(url, null);
                contentResult = (await (await contentResultTask).Content.ReadAsStringAsync());
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(Program.LangManager.GetString("tip.requestTimeOut"));
                return new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Translate Exception:" + ex.Message);
                return new Dictionary<string, string>();
            }

            if (cancellationToken != CancellationToken.None)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            JObject obj = JObject.Parse(contentResult);
            JToken translatesToken = obj.SelectToken("Data.Translates");
            if (translatesToken == null)
            {

                return new Dictionary<string, string>();
            }

            var result = translatesToken.ToObject<Dictionary<string, string>>();
            if (result == null || result.Count == 0)
            {
                return result;
            }

            if (emptyCount == 0)
            {
                if (toLang == "en")
                {
                    result = FixDic(result);
                }

                return result;
            }
            var newDic = new Dictionary<string, string>();

            int t = 0;
            for (int j = 0; j < list.Count; j++)
            {
                if (emptyIndexList[j] == -1)
                {
                    var ele = result.ElementAt(t);
                    newDic.Add(ele.Key, ele.Value);
                    t++;
                    continue;
                }

                newDic.Add("", "");
            }

            if (toLang == "en")
            {
                newDic = FixDic(newDic);
            }
            return newDic;
        }

        private static bool IsChinese(string Text)
        {
            for (int i = 0; i < Text.Length; i++)
            {
                if (Regex.IsMatch(Text.ToString(), @"[\u4E00-\u9FA5]+$"))
                {
                    return true;
                }
            }
            return false;

        }

        private static Dictionary<string, string> FixDic(Dictionary<string, string> dic)
        {
            var newDic = new Dictionary<string, string>();
            foreach (var kv in dic)
            {
                newDic.Add(kv.Key, dic[kv.Key].Replace(" ", "_").ToLower());
            }

            return newDic;
        }


        public override void Dispose()
        {
            client.Dispose();
        }
    }
}
