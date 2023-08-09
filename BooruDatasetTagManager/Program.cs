using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooruDatasetTagManager
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();


#if NET5_0_OR_GREATER
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
            Application.SetCompatibleTextRenderingDefault(false);
            Settings = new AppSettings(Application.StartupPath);

            LangManager = new LangManager(Settings.AppLanguage);

            #region waitForm
            Form f_wait = new Form();
            f_wait.AutoScaleMode = AutoScaleMode.Dpi;
            f_wait.Width = 300;
            f_wait.Height = 100;
            f_wait.FormBorderStyle = FormBorderStyle.FixedDialog;
            f_wait.ControlBox = false;
            f_wait.StartPosition = FormStartPosition.CenterScreen;
            Label mes = new Label();
            mes.Text = LangManager.GetString("startUpLoadingMsg"); ;
            mes.Location = new System.Drawing.Point(10, 10);
            mes.AutoSize = true;

            f_wait.Controls.Add(mes);

            f_wait.Shown += async (o, i) =>
            {
                await Task.Run(() =>
                {
                    string translationsDir = Path.Combine(Application.StartupPath, "Translations");
                    if (!Directory.Exists(translationsDir))
                        Directory.CreateDirectory(translationsDir);
                    TransManager = new TranslationManager(Program.Settings.TranslationLanguage, Program.Settings.TransService, translationsDir);
                    TransManager.LoadTranslations();
                    string tagsDir = Path.Combine(Application.StartupPath, "Tags");
                    if (!Directory.Exists(tagsDir))
                        Directory.CreateDirectory(tagsDir);
                    string tagFile = Path.Combine(tagsDir, "List.tdb");
                    TagsList = TagsDB.LoadFromTagFile(tagFile);
                    if (TagsList == null)
                        TagsList = new TagsDB();
                    if (TagsList.IsNeedUpdate(tagsDir))
                    {
                        TagsList.ClearDb();
                        TagsList.ClearLoadedFiles();
                        TagsList.ResetVersion();
                        TagsList.LoadCSVFromDir(tagsDir);
                        TagsList.LoadTxtFromDir(tagsDir);
                        TagsList.SortTags();
                        TagsList.SaveTags(tagFile);
                    }
                    TagsList.LoadTranslation(TransManager);
                });
                f_wait.Close();
            };
            f_wait.ShowDialog();
            #endregion

            Application.Run(new Form1());
        }
        public static TranslationManager TransManager;

        public static DatasetManager DataManager;

        public static AppSettings Settings;

        public static TagsDB TagsList;

        public static LangManager LangManager;
    }
}
