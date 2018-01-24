using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace ChatBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool autostart = false;
        string constr = "";
        BackgroundWorker bw;
        decimal timer_time = 0;
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerOnTick;
            try
            {
                string alltext = File.ReadAllText("settings.txt");
                string[] keyAndSql = alltext.Split('|');
                KeyText.Text = keyAndSql[0];
                constr = keyAndSql[1];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings file not found or file is corrupted", "Alert", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                File.WriteAllText(DateTime.Now.ToString("dd.MM.yy hh.mm.ss") + " errorlog.txt", DateTime.Now + " -- " + "Settings file not found or file is corrupted");
                Application.Current.Shutdown();
            }
            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            if (KeyText.Text != "")
            {
                var text = @KeyText.Text;
                if (text != "" && bw.IsBusy != true)
                {
                    bw.RunWorkerAsync(text);
                    timer.Start();
                    autostart = true;
                }
            }
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            SendLog("SERVER STOPED TIME WORKED IN SEC " + timer_time);
            savetofilelog();
        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String;
            try
            {
                var Bot = new Telegram.Bot.TelegramBotClient(key);
                await Bot.SetWebhookAsync("");
                int offset = 0;
                while (true)
                {
                    Update[] updates;
                    decimal conAttempts = 1;
                    decimal allwaitingseconds = 0;
                    while (true)
                    {
                        try
                        {
                            updates = await Bot.GetUpdatesAsync(offset);
                        }
                        catch (Exception ex)
                        {
                            allwaitingseconds += (conAttempts / 10);
                            SendLog("No connection for the last " + allwaitingseconds + " seconds, attempt = " + conAttempts++);
                            Thread.Sleep((int)conAttempts * 100);
                            continue;
                        }
                        conAttempts = 1;
                        break;
                    }
                    foreach (var update in updates)
                    {
                        Person p = null;
                        if (update.Message.From.Username != null)
                        {
                            p = new Person(update.Message.From.Id, update.Message.From.Username, constr);
                        }
                        else
                        {
                            p = new Person(update.Message.From.Id, update.Message.From.FirstName + " " + update.Message.From.LastName, constr);
                        }
                        var keyboardStandart = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                        {
                            Keyboard = new[] {
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton("Задание"),
                                                },
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton("Очки"),
                                                    new Telegram.Bot.Types.KeyboardButton("ТОП-10"),
                                                    new Telegram.Bot.Types.KeyboardButton("Помощь")
                                                },
                                            },
                            OneTimeKeyboard = false,
                            ResizeKeyboard = true
                        };
                        var keyboardStandartFail = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                        {
                            Keyboard = new[] {
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton("Задание")
                                                },
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton("Очки"),
                                                    new Telegram.Bot.Types.KeyboardButton("ТОП-10"),
                                                    new Telegram.Bot.Types.KeyboardButton("Помощь")
                                                },
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton("Ошибка в последнем вопросе?")
                                                },
                                            },
                            OneTimeKeyboard = false,
                            ResizeKeyboard = true
                        };
                        var keyboardSerial = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                        {
                            Keyboard = new[] {
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton(p.VSerial[0]),
                                                    new Telegram.Bot.Types.KeyboardButton(p.VSerial[1]),
                                                    new Telegram.Bot.Types.KeyboardButton(p.VSerial[2]),
                                                    new Telegram.Bot.Types.KeyboardButton(p.VSerial[3])
                                                },
                                            },
                            OneTimeKeyboard = false,
                            ResizeKeyboard = true
                        };
                        var keyboardSeason = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup();
                        if (p.C_serial == "SGU")
                        {
                            keyboardSeason = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                            {
                                Keyboard = new[] {
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton("1"),
                                                    new Telegram.Bot.Types.KeyboardButton("2")
                                                },
                                            },
                                OneTimeKeyboard = false,
                                ResizeKeyboard = true
                            };
                        }
                        else
                        {
                            keyboardSeason = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                            {
                                Keyboard = new[] {
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Season[0]),
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Season[1]),
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Season[2]),
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Season[3])
                                                },
                                            },
                                OneTimeKeyboard = false,
                                ResizeKeyboard = true
                            };
                        }
                        var keyboardSeries = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                        {
                            Keyboard = new[] {
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Series[0])
                                                },
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Series[1])
                                                },
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Series[2])
                                                },
                                                new[]
                                                {
                                                    new Telegram.Bot.Types.KeyboardButton(p.V_Series[3])
                                                },
                                            },
                            OneTimeKeyboard = false,
                            ResizeKeyboard = true
                        };
                        var message = update.Message;
                        bool recognise = false;
                        if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                        {
                            #region stand_0
                            if (p.stand == 0)
                            {
                                if (message.Text == "/start")
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    if (p.task.Contains("http"))
                                    {
                                        await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), "Из какого сериала или фильмов эта картинка?", false, 0, keyboardSerial);
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Из какого сериала или фильмов эта фраза? \n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSerial);
                                    }
                                }
                                if (message.Text == p.VSerial[0] || message.Text == p.VSerial[1] || message.Text == p.VSerial[2] || message.Text == p.VSerial[3])
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    if (message.Text == p.C_serial)
                                    {
                                        if (p.C_serial == "Movies")
                                        {
                                            p.stand += 2;
                                            p.SaveToSQl();
                                            SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "Correct" + " STAND " + p.stand + " TASK_ID " + p.taskId);
                                            if (p.task.Contains("http"))
                                            {
                                                await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), p.C_serial + ", верно! Какой фильм?", false, 0, keyboardSeries);
                                            }
                                            else
                                            {
                                                await Bot.SendTextMessageAsync(message.Chat.Id, p.C_serial + ", верно! Какой фильм? \n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSeries);
                                            }
                                        }
                                        else
                                        {
                                            p.stand++;
                                            p.SaveToSQl();
                                            SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "Correct" + " STAND " + p.stand + " TASK_ID " + p.taskId);
                                            if (p.task.Contains("http"))
                                            {
                                                await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), p.C_serial + ", верно! Какой сезон?", false, 0, keyboardSeason);
                                            }
                                            else
                                            {
                                                await Bot.SendTextMessageAsync(message.Chat.Id, p.C_serial + ", верно! Какой сезон? \n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSeason);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        p.stand = 3;
                                        p.TaskWrong();
                                        SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "InCorrect" + " SCORE " + p.Score + " TASK_ID " + p.taskId);
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Неверно! Запросите новое задание!", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandartFail);
                                    }
                                }
                            }
                            #endregion
                            #region stand_1
                            if (p.stand == 1)
                            {
                                if (message.Text == "/start")
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    if (p.task.Contains("http"))
                                    {
                                        await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), p.C_serial + ", верно! Какой сезон?", false, 0, keyboardSeason);
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id, p.C_serial + ", верно! Какой сезон?\n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSeason);
                                    }
                                }
                                if (p.C_serial == "SGU")
                                {
                                    if (message.Text == "1" || message.Text == "2")
                                    {
                                        p.Ask = false;
                                        recognise = true;
                                        if (message.Text == p.C_season)
                                        {
                                            p.stand++;
                                            p.SaveToSQl();
                                            SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "Correct" + " STAND " + p.stand + " TASK_ID " + p.taskId);
                                            if (p.task.Contains("http"))
                                            {
                                                await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), p.C_season + ", верно! Какая серия?", false, 0, keyboardSeries);
                                            }
                                            else
                                            {
                                                await Bot.SendTextMessageAsync(message.Chat.Id, p.C_season + ", верно! Какая серия?\n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSeries);
                                            }
                                        }
                                        else
                                        {
                                            p.stand = 3;
                                            p.TaskWrong();
                                            SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "InCorrect" + " SCORE " + p.Score + " TASK_ID " + p.taskId);
                                            await Bot.SendTextMessageAsync(message.Chat.Id, "Неверно! Запросите новое задание!", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandartFail);
                                        }
                                    }
                                }
                                else
                                {
                                    if (message.Text == p.V_Season[0] || message.Text == p.V_Season[1] || message.Text == p.V_Season[2] || message.Text == p.V_Season[3])
                                    {
                                        p.Ask = false;
                                        recognise = true;
                                        if (message.Text == p.C_season)
                                        {
                                            p.stand++;
                                            p.SaveToSQl();
                                            SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "Correct" + " STAND " + p.stand + " TASK_ID " + p.taskId);
                                            if (p.task.Contains("http"))
                                            {
                                                await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), p.C_season + ", верно! Какая серия?", false, 0, keyboardSeries);
                                            }
                                            else
                                            {
                                                await Bot.SendTextMessageAsync(message.Chat.Id, p.C_season + ", верно! Какая серия?\n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSeries);
                                            }
                                        }
                                        else
                                        {
                                            p.stand = 3;
                                            p.TaskWrong();
                                            SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "InCorrect" + " SCORE " + p.Score + " TASK_ID " + p.taskId);
                                            await Bot.SendTextMessageAsync(message.Chat.Id, "Неверно! Запросите новое задание!", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandartFail);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region stand_2
                            if (p.stand == 2)
                            {
                                if (message.Text == "/start")
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    if (p.C_serial == "Movies")
                                    {
                                        if (p.task.Contains("http"))
                                        {
                                            await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), p.C_serial + ", верно! Какой фильм?", false, 0, keyboardSeries);
                                        }
                                        else
                                        {
                                            await Bot.SendTextMessageAsync(message.Chat.Id, p.C_serial + ", верно! Какой фильм?\n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSeries);
                                        }
                                    }
                                    else
                                    {
                                        if (p.task.Contains("http"))
                                        {
                                            await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), p.C_season + ", верно! Какая серия?", false, 0, keyboardSeries);
                                        }
                                        else
                                        {
                                            await Bot.SendTextMessageAsync(message.Chat.Id, p.C_season + ", верно! Какая серия?\n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSeries);
                                        }
                                    }
                                }
                                if (message.Text == p.V_Series[0] || message.Text == p.V_Series[1] || message.Text == p.V_Series[2] || message.Text == p.V_Series[3])
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    if (message.Text == p.C_series)
                                    {
                                        p.stand++;
                                        p.TaskDone();
                                        SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "Correct" + " SCORE " + p.Score + " TASK_ID " + p.taskId);
                                        await Bot.SendTextMessageAsync(message.Chat.Id, p.C_series + ", верно! Вы молодец! Запрашивайте новое задание :)", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                                    }
                                    else
                                    {
                                        p.stand = 3;
                                        p.TaskWrong();
                                        SendLog(message.Text + " FROM " + p.UserName + " ITIS " + "InCorrect" + " SCORE " + p.Score + " TASK_ID " + p.taskId);
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Неверно! Запросите новое задание!", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandartFail);
                                    }
                                }
                            }
                            #endregion
                            #region stand_3
                            if (p.stand == 3)
                            {
                                if (message.Text == "Задание")
                                {
                                    recognise = true;
                                    p.Ask = false;
                                    p.stand = 0;
                                    p.GetTask();
                                    SendLog(message.Text + " TO " + p.UserName + " TASK_ID " + p.taskId);
                                    if (p.task.Contains("http"))
                                    {
                                        await Bot.SendPhotoAsync(message.Chat.Id, new FileToSend(p.task), "Из какого сериала или фильмов эта картинка?", false, 0, keyboardSerial);
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Из какого сериала или фильмов эта фраза?\n\n" + p.task, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardSerial);
                                    }
                                }
                                if (message.Text == "Ошибка в последнем вопросе?" && !p.done)
                                {
                                    recognise = true;
                                    p.Ask = true;
                                    p.SaveToSQl();
                                    SendLog("SEND_INSTRUCTION_TO_WRONG_MSG_TO " + p.UserName + " TASK_ID " + p.taskId + " ASK_UNIT " + p.Ask.ToString());
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Отправьте сообщение с описанием проблемы на столько широко как сможете, так же тут можно оставить свой отзыв.", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                                }
                                if (message.Text == "Помощь")
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    SendLog(message.Text + " TO " + p.UserName);
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Просто отвечайте на вопросы и зарабатывайте очки. Верный ответ +1, неверный -1 балл.\nДля управления чатом используй кнопки внизу.", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                                }
                                if (message.Text == "ТОП-10")
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    SendLog(message.Text + " TO " + p.UserName);
                                    await Bot.SendTextMessageAsync(message.Chat.Id, getTopList(), Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                                }
                                if (message.Text == "Очки")
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    SendLog(message.Text + " TO " + p.UserName + " SCORE " + p.Score);
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Кол-во очков: " + p.Score, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                                }
                                if (message.Text == "/start")
                                {
                                    p.Ask = false;
                                    recognise = true;
                                    if (!p.amnew)
                                    {
                                        SendLog("NEW_OLD_USER " + p.Id + " NAME " + p.UserName);
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "С Возвращением!\nИспользуйте клавиатуру для работы с чатом", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                                    }
                                    else
                                    {
                                        SendLog("NEW_USER " + p.Id + " NAME " + p.UserName);
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать!\nИспользуйте клавиатуру для работы с чатом", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                                    }

                                }
                            }
                            #endregion
                            if (message.Text != "" && !recognise && !p.Ask)
                            {
                                SendLog(message.Text + " SOME_TEXT_WRONG " + " FROM " + p.UserName);
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Я не могу ответить на это сообщение, не кликайте на кнопку по нескольку раз, не дождавшись ответа и не пишите с клавиатуры до требования бота!");
                            }
                            else if (message.Text != "" && p.Ask && !recognise)
                            {
                                p.Ask = false;
                                SendLog(message.Text + " RECEIVING_MSG_FROM_USER " + p.UserName);
                                AskToSQL(message.Text, p);
                                p.SaveToSQl();
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Ваше сообщение принято для обработки, со временем изменения вступят в силу", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardStandart);
                            }
                        }
                        else
                        {
                            SendLog("SOME_NOT_TEXT_WRONG " + " FROM " + p.UserName);
                            await Bot.SendTextMessageAsync(message.Chat.Id, "Я могу отвечать только на текстовые сообщения!");
                        }
                        offset = update.Id + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(DateTime.Now.ToString("dd.MM.yy hh.mm.ss") + " errorlog.txt", DateTime.Now + " -- " + ex.Message + "\n"
                    + ex.TargetSite.Name + "\n" + ex.StackTrace + "\n" + ex.HelpLink);
                Application.Current.Shutdown();
            }

        }
        private void AskToSQL(string ask, Person per)
        {
            using (var con = new SqlConnection(constr))
            {
                var ds = new DataSet();
                var a = new SqlDataAdapter();
                using (SqlCommand command = new SqlCommand("INSERT INTO dbo.AskToWrong (user_id, Message, task_id) VALUES (@user_id, @Message, @task_id);", con))
                {
                    command.Parameters.AddWithValue("@user_id", per.Id);
                    command.Parameters.AddWithValue("@Message", ask);
                    command.Parameters.AddWithValue("@task_id", per.taskId);

                    con.Open();
                    int result = command.ExecuteNonQuery();
                    if (result < 0)
                    {
                        throw new Exception("Error in command new user");
                    }
                    con.Close();
                }
            }
        }
        private void SendLog(string log)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                LogBox.Text = DateTime.Now + " -- " + log + Environment.NewLine + LogBox.Text;
            }));
        }
        private void savetofilelog()
        {
            if (LogBox.Text != "")
            {
                bool newfile = true;
                string textlog = "";
                if (File.Exists("log.txt"))
                {
                    newfile = false;
                    textlog = File.ReadAllText("log.txt");
                    File.WriteAllText("log.txt", "");
                }
                else
                {
                    newfile = true;
                    File.WriteAllText("log.txt", "");
                }
                using (var stream = File.CreateText("log.txt"))
                {
                            stream.Write(LogBox.Text);
                            LogBox.Clear();
                        if (!newfile) stream.Write(textlog);
                }
            }
        }
        private string getTopList()
        {
            string res = "ТОП ПОЛЬЗОВАТЕЛЕЙ: ";
            using (var con = new SqlConnection(constr))
            {
                var list = new List<Top>();
                using (var command = new SqlCommand("SELECT TOP 10 Username, Score FROM dbo.Users ORDER BY Score DESC;", con))
                {
                    con.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add(new Top
                            {
                                Score = reader.GetInt32(1),
                                Username = reader.GetString(0)
                            });
                    }
                }
                if (list.Count == 0)
                {
                    throw new Exception("More than one user in table");
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Username.Split(' ').Length == 1)
                        {
                            res += "\n" + (i + 1) + ": @" + list[i].Username + ": " + list[i].Score;
                        }
                        else
                        {
                            res += "\n" + (i + 1) + ": " + list[i].Username + ": " + list[i].Score;
                        }
                    }
                }

            }
            return res;
        }
        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (timer_time == 1)
                SendLog("SERVER START");
            SecondsOnline.Text = timer_time.ToString();
            timer_time++;
            if (timer_time % 60 == 0) savetofilelog();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            var text = @KeyText.Text;
            if (text != "" && bw.IsBusy != true && autostart == false)
            {
                bw.RunWorkerAsync(text);
                timer.Start();
                SendLog("SERVER START");
            }
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LoadButton_OnClickButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddRowButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
