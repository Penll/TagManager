using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TagManager
{
    public abstract class AbstractTranslator : IDisposable
    {
        public TranslationService Service { get; set; }
        public AbstractTranslator(TranslationService service)
        {
            Service = service;
        }

        public static AbstractTranslator Create(TranslationService service)
        {
            switch (service)
            {
                case TranslationService.GoogleTranslate:
                    {
                        return new GoogleTranslator();
                    }
                case TranslationService.ChineseTranslate:
                    {
                        return new ChineseTranslator();
                    }
                case TranslationService.CustomTranslate:
                    {
                        return new CustomTranslator();
                    }
                default:
                    throw new NotImplementedException("Translation service not implemented");
            }
        }

        public virtual async Task<string> TranslateAsync(string text, string fromLang, string toLang)
        {
            return null;
        }

        public virtual async Task<Dictionary<string, string>> TranslateAsync(CancellationToken cancellationToken, IEnumerable<string> contentList, string fromLang, string toLang)
        {
            return null;
        }

        public abstract void Dispose();
    }
}
