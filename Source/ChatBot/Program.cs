using System;
using System.IO;
using System.Windows.Forms;

namespace ChatBot
{
    static class Program
    {
        /// <summary>
        /// В БД загружены 1 сезон ЗВ-1, 2 сезон по 10 серию
        /// Картинки 1 полностью и2 сезон по 10 серию, ЗВ фильм, но надо поменять
        /// 
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new BotForm());
        }
            catch (Exception ex)
            {
                File.WriteAllText(DateTime.Now.ToString("dd.MM.yy hh.mm.ss") + " errorlog.txt", DateTime.Now + " -- " + ex.Message + "\n"
                    + ex.TargetSite.Name + "\n" + ex.StackTrace + "\n" + ex.HelpLink);
                Application.Exit();
            }
}
    }
}
