﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BooruDatasetTagManager
{
    public class TranslationManager
    {
        private string _language;
        private string _workDir;
        public List<TransItem> Translations { get; set; }
        private AbstractTranslator translator;
        private string translationFilePath;

        public TranslationManager(string toLang, TranslationService service, string workDir)
        {
            _language = toLang;
            _workDir = workDir;
            Translations = new List<TransItem>();
            translator = AbstractTranslator.Create(service);
            translationFilePath = Path.Combine(_workDir, _language + ".txt");
        }

        public void LoadTranslations()
        {
            if (!File.Exists(translationFilePath))
            {
                using (var sw = File.CreateText(translationFilePath))
                {
                    sw.WriteLine("//Translation format: <original>=<translation>");
                }
                return;
            }
            string[] lines = File.ReadAllLines(translationFilePath);
            foreach (var item in lines)
            {
                if (item.Trim().StartsWith("//"))
                    continue;
                var transItem = TransItem.Create(item);
                if (transItem != null && !Contains(transItem.OrigHash))
                {
                    Translations.Add(transItem);
                }
            }
        }

        public bool Contains(string orig)
        {
            return Translations.Exists(a => a.OrigHash == orig.ToLower().GetHash());
        }

        public bool Contains(long hash)
        {
            return Translations.Exists(a => a.OrigHash == hash);
        }

        public string GetTranslation(string text)
        {
            return GetTranslation(text.ToLower().Trim().GetHash());
        }


        public List<string> GetTranslation(IEnumerable<string> textList)
        {
            return GetTranslation(textList.Select(o => o.ToLower().Trim().GetHash()));
        }

        public string GetTranslation(long hash)
        {
            var res = Translations.FirstOrDefault(a => a.OrigHash == hash);
            if (res == null)
                return null;
            return res.Trans;
        }


        public List<string> GetTranslation(IEnumerable<long> hashList)
        {
            List<string> result = new List<string>(hashList.Count());
            bool hasBreak = false;
            hashList.ToList().ForEach(hash =>
            {
                if (hasBreak)
                {
                    return;
                }

                var res = Translations.FirstOrDefault(a => a.OrigHash == hash);
                if (res == null)
                {
                    hasBreak = true;
                    return;
                }

                result.Add(res.Trans);
            });

            if (hasBreak)
            {
                return new List<string>();
            }

            return result;
        }

        public string GetTranslation(long hash, bool onlyManual)
        {
            if (onlyManual)
            {
                var res = Translations.FirstOrDefault(a => a.OrigHash == hash && a.IsManual == onlyManual);
                if (res == null)
                    return null;
                return res.Trans;
            }
            else
                return GetTranslation(hash);

        }

        public async Task<string> GetTranslationAsync(string text)
        {
            return await Task.Run(() =>
            {
                return GetTranslation(text);
            });
        }


        public async Task<List<string>> GetTranslationAsync(IEnumerable<string> textList)
        {
            return await Task.Run(() =>
            {
                return GetTranslation(textList);
            });
        }

        public void AddTranslation(string orig, string trans, bool isManual)
        {
            File.AppendAllText(translationFilePath, $"{orig}={trans}\r\n", Encoding.UTF8);
            Translations.Add(new TransItem(orig, trans, isManual));
        }

        public async Task AddTranslationAsync(string orig, string trans, bool isManual)
        {
            using (StreamWriter sw = new StreamWriter(translationFilePath, true, Encoding.UTF8))
            {
                await sw.WriteLineAsync($"{(isManual ? "*" : "")}{orig}={trans}");
            }

            Translations.Add(new TransItem(orig, trans, isManual));
        }

        public async Task<string> TranslateAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            string result = await GetTranslationAsync(text);
            if (result != null)
                return result;
            result = await translator.TranslateAsync(text, "en", _language);
            await AddTranslationAsync(text, result, false);
            return result;
        }

        public async Task<List<string>> TranslateAsync(CancellationToken cancellationToken, IEnumerable<string> textList)
        {
            if (textList == null || textList.Count() == 0)
            {
                return new List<string>();
            }

            if (textList.Count() == 1)
            {
                return new List<string> { await TranslateAsync(textList.First()) };
            }

            List<string> resultList = await GetTranslationAsync(textList);
            if (resultList != null && resultList.Count > 0)
                return resultList;

            var dic = await translator.TranslateAsync(cancellationToken, textList, "en", _language);

            if (dic == null || dic.Count == 0)
            {
                return new List<string>();
            }
            resultList = new List<string>();
            foreach (var text in textList)
            {
                string trans = string.Empty;
                if (dic.TryGetValue(text, out trans))
                {
                    resultList.Add(trans);

                    if (text != trans)
                    {
                        await AddTranslationAsync(text, trans, false);//add local cache
                    }
                }
                else
                {
                    resultList.Add("");
                }
            }

            return resultList;
        }

        public class TransItem
        {
            public string Orig { get; private set; }
            public string Trans { get; set; }
            public long OrigHash { get; private set; }
            public bool IsManual { get; private set; }

            public TransItem(string orig, string trans, bool isManual)
            {
                Orig = orig;
                Trans = trans;
                OrigHash = orig.ToLower().GetHash();
                IsManual = isManual;
            }

            public static TransItem Create(string text)
            {
                bool manual = false;
                if (text.StartsWith("*"))
                {
                    text = text.Substring(1);
                    manual = true;
                }
                int index = text.LastIndexOf('=');
                if (index == -1)
                    return null;
                string orig = text.Substring(0, index).Trim();
                string trans = text.Substring(index + 1).Trim();
                return new TransItem(orig, trans, manual);
            }


            public TransItem() { }
        }
    }
}
