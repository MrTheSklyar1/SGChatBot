using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ChatBot
{
    public class Person
    {
        public bool amnew = false;
        private string constr = "";
        public int Id = -1;//
        public string LastSer = "";
        public string LastSeas = "";
        public string task;//
        public string C_season;//
        public string[] V_Season = new string[4];
        public string C_series;//
        public string C_serial;//
        public string[] V_Series = new string[4];
        public string[] VSerial = { "SG1", "SGA", "SGU", "Movies" };
        public int taskId = 0;//
        public bool done = false;//
        public bool Ask = false;
        public int AskI = 0;
        public int Score = 0;//
        public int stand = 3;
        public string UserName = "";
        public Person(int id, string usern, string constring)
        {
            constr = constring;
            Id = id;
            UserName = usern;
            getinfo();
        }
        private void getinfo()
        {
            using (var con = new SqlConnection(constr))
            {
                var list = new List<User>();
                using (var command = new SqlCommand("SELECT t2.Task_id, t2.done, t1.Score, t2.Stand, t1.Username, t2.LastItemsSer, t2.LastItemsSeas, t2.Ask FROM dbo.Users t1 join dbo.UserQuestionInfo t2 on t1.User_ID = t2.User_ID WHERE t1.User_ID = " + Id + ";", con))
                {
                    con.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add(new User
                            {
                                User_ID = Id,
                                Task_id = reader.GetInt32(0),
                                done = reader.GetInt32(1),
                                Score = reader.GetInt32(2),
                                Stand = reader.GetInt32(3),
                                Username = reader.GetString(4),
                                LastSeries = reader.GetString(5),
                                lastSeason = reader.GetString(6),
                                ask = reader.GetInt32(7)
                            });
                    }
                }
                if (list.Count == 0)
                {
                    con.Close();
                    NewUser();
                }
                else if (list.Count == 1)
                {
                    taskId = list[0].Task_id;
                    done = (list[0].done == 0) ? false : true;
                    Score = list[0].Score;
                    stand = list[0].Stand;
                    LastSeas = list[0].lastSeason;
                    LastSer = list[0].LastSeries;
                    Ask = (list[0].ask == 0) ? false : true;
                    if (UserName != list[0].Username)
                    {
                        setNewUserName();
                    }
                    GetTask(true);
                }
                else
                {
                    throw new Exception("More than one user in table");
                }
            }
        }
        public void setNewUserName()
        {
            using (var con = new SqlConnection(constr))
            {
                var ds = new DataSet();
                var a = new SqlDataAdapter();
                using (SqlCommand command = new SqlCommand("UPDATE dbo.Users SET Username='" + UserName + "' WHERE User_ID=" + Id + ";", con))
                {
                    con.Open();
                    int result = command.ExecuteNonQuery();
                    if (result < 0)
                    {
                        throw new Exception("Error in command save true");
                    }
                    con.Close();
                }
            }
        }
        public void GetTask(bool old_user = false)
        {
            using (var con = new SqlConnection(constr))
            {
                var list = new List<Task>();
                Task currentTask;
                if (old_user && !done)
                {
                    amnew = false;
                    using (var command = new SqlCommand("SELECT Task_id, Task_text, Serial, Season, Series FROM dbo.Tasks WHERE Task_id=" + taskId + ";", con))
                    {
                        con.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                list.Add(new Task
                                {
                                    Task_id = reader.GetInt32(0),
                                    Task_text = reader.GetString(1),
                                    Serial = reader.GetString(2),
                                    Season = reader.GetString(3),
                                    Series = reader.GetString(4)
                                });
                        }
                        con.Close();
                    }
                    if (list.Count != 1)
                    {
                        throw new Exception("More than 1 data from tasks db");
                    }
                    else
                    {
                        currentTask = list[0];
                    }
                    taskId = currentTask.Task_id;
                    task = currentTask.Task_text;
                    C_serial = currentTask.Serial;
                    C_season = currentTask.Season;
                    C_series = currentTask.Series;
                    V_Season = LastSeas.Split(',');
                    V_Series = LastSer.Split('|');
                }
                else
                {
                    amnew = true;
                    int count = 0;
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.tasks;", con))
                    {
                        con.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                count = reader.GetInt32(0);
                        }
                        con.Close();
                    }
                    Random r = new Random();
                    int randt = r.Next(count) + 1;
                    using (var command = new SqlCommand("SELECT Task_id, Task_text, Serial, Season, Series FROM dbo.Tasks WHERE Task_id=" + randt + ";", con))
                    {
                        con.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                list.Add(new Task
                                {
                                    Task_id = reader.GetInt32(0),
                                    Task_text = reader.GetString(1),
                                    Serial = reader.GetString(2),
                                    Season = reader.GetString(3),
                                    Series = reader.GetString(4)
                                });
                        }
                        con.Close();
                    }
                    if (list.Count != 1)
                    {
                        throw new Exception("More than 1 data from tasks db");
                    }
                    else
                    {
                        currentTask = list[0];
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        V_Season[i] = "";
                        V_Series[i] = "";
                    }
                    taskId = currentTask.Task_id;
                    task = currentTask.Task_text;
                    C_serial = currentTask.Serial;
                    C_season = currentTask.Season;
                    C_series = currentTask.Series;
                    var dblist = new List<DBSeries>();
                    using (var command = new SqlCommand("SELECT Seasons, Series FROM dbo.Serials WHERE Serial='" + C_serial + "' AND Season=" + Convert.ToInt32(C_season) + ";", con))
                    {
                        con.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                dblist.Add(new DBSeries
                                {
                                    num = reader.GetInt32(0),
                                    series = reader.GetString(1),
                                });
                        }
                        con.Close();
                    }
                    DBSeries db = new DBSeries();
                    if (dblist.Count != 1)
                    {
                        throw new Exception("More than 1 data from series db");
                    }
                    else
                    {
                        db = dblist[0];
                    }
                    bool movedone = false;
                    bool first = true;
                    int iterator = 0;
                    while (movedone != true)
                    {
                        Random myr = new Random();
                        int n = myr.Next(4);
                        if (V_Season[n] == "")
                        {
                            if (first)
                            {
                                V_Season[n] = C_season;
                                first = false;
                            }
                            else
                            {
                                Random randSeas = new Random();
                                while (V_Season[n] == "")
                                {
                                    int numSeas = randSeas.Next(db.num) + 1;
                                    if (numSeas != Convert.ToInt32(C_season))
                                    {
                                        if (V_Season[0] != numSeas.ToString() && V_Season[1] != numSeas.ToString() && V_Season[2] != numSeas.ToString() && V_Season[3] != numSeas.ToString())
                                            V_Season[n] = numSeas.ToString();
                                    }
                                }
                            }
                            iterator++;
                        }
                        if (V_Season[0] != "" && V_Season[1] != "" && V_Season[2] != "" && V_Season[3] != "") movedone = true;
                    }
                    movedone = false;
                    first = true;
                    iterator = 0;
                    string[] dbseriesvar = db.series.Split('|');
                    while (movedone != true)
                    {
                        Random myr = new Random();
                        int n = myr.Next(4);
                        if (V_Series[n] == "")
                        {
                            if (first)
                            {
                                V_Series[n] = C_series;
                                first = false;
                            }
                            else
                            {
                                Random randSer = new Random();
                                while (V_Series[n] == "")
                                {
                                    int numSer = randSer.Next(dbseriesvar.Length);
                                    if (dbseriesvar[numSer] != C_series)
                                    {
                                        if (V_Series[0] != dbseriesvar[numSer] && V_Series[1] != dbseriesvar[numSer] && V_Series[2] != dbseriesvar[numSer] && V_Series[3] != dbseriesvar[numSer])
                                            V_Series[n] = dbseriesvar[numSer];
                                    }
                                }
                            }
                            iterator++;
                        }
                        if (V_Series[0] != "" && V_Series[1] != "" && V_Series[2] != "" && V_Series[3] != "") movedone = true;
                    }
                    string temp1 = "";
                    string temp2 = "";
                    for (int i = 0; i < 4; i++)
                    {
                        if (i == 3)
                        {
                            temp1 = temp1 + V_Series[i];
                            temp2 = temp2 + V_Season[i];
                        }
                        else
                        {
                            temp1 = temp1 + V_Series[i] + "|";
                            temp2 = temp2 + V_Season[i] + ",";
                        }
                    }
                    LastSeas = temp2;
                    LastSer = temp1;
                }
                list.Clear();
            }
            SaveToSQl();
        }
        public void SaveToSQl(bool done = false)
        {
            AskI = Ask ? 1 : 0;
            if (done)
            {
                using (var con = new SqlConnection(constr))
                {
                    if (LastSer.Contains("'"))
                    {
                        LastSer = LastSer.Replace("'", "''");
                    }
                    using (SqlCommand command = new SqlCommand("UPDATE dbo.UserQuestionInfo SET done=1, Stand=" + stand + ", LastItemsSer='" + LastSer + "', LastItemsSeas='" + LastSeas + "', Ask=" + AskI + " WHERE User_ID=" + Id + ";", con))
                    {
                        con.Open();
                        int result = command.ExecuteNonQuery();
                        if (result < 0)
                        {
                            throw new Exception("Error in command save true");
                        }
                        con.Close();
                    }
                    using (SqlCommand command = new SqlCommand("UPDATE dbo.Users SET Score=" + Score + " WHERE User_ID=" + Id + ";", con))
                    {
                        con.Open();
                        int result = command.ExecuteNonQuery();
                        if (result < 0)
                        {
                            throw new Exception("Error in command save true");
                        }
                        con.Close();
                    }
                    if (LastSer.Contains("''"))
                    {
                        LastSer = LastSer.Replace("''", "'");
                    }
                }
            }
            else
            {
                using (var con = new SqlConnection(constr))
                {
                    if (LastSer.Contains("'"))
                    {
                        LastSer = LastSer.Replace("'", "''");
                    }
                    using (SqlCommand command = new SqlCommand("UPDATE dbo.UserQuestionInfo SET done=0, Stand=" + stand + ", Task_id=" + taskId + ", LastItemsSer='" + LastSer + "', LastItemsSeas='" + LastSeas + "', Ask=" + AskI + " WHERE User_ID=" + Id + ";", con))
                    {
                        con.Open();
                        int result = command.ExecuteNonQuery();
                        if (result < 0)
                        {
                            throw new Exception("Error in command save true");
                        }
                        con.Close();
                    }
                    using (SqlCommand command = new SqlCommand("UPDATE dbo.Users SET Score=" + Score + " WHERE User_ID=" + Id + ";", con))
                    {
                        con.Open();
                        int result = command.ExecuteNonQuery();
                        if (result < 0)
                        {
                            throw new Exception("Error in command save true");
                        }
                        con.Close();
                    }
                    if (LastSer.Contains("''"))
                    {
                        LastSer = LastSer.Replace("''", "'");
                    }
                }
            }
        }
        public void NewUser()
        {
            GetTask();
            using (var con = new SqlConnection(constr))
            {
                using (SqlCommand command = new SqlCommand("INSERT INTO dbo.Users (User_ID, Username, Score) VALUES (@User_ID,@Username,@Score);", con))
                {
                    command.Parameters.AddWithValue("@User_ID", Id);
                    command.Parameters.AddWithValue("@Score", Score);
                    command.Parameters.AddWithValue("@Username", UserName);

                    con.Open();
                    int result = command.ExecuteNonQuery();
                    if (result < 0)
                    {
                        throw new Exception("Error in command new user");
                    }
                    con.Close();
                }
                using (SqlCommand command = new SqlCommand("INSERT INTO dbo.UserQuestionInfo (User_ID, Task_id, done, Stand, Ask, LastItemsSer, LastItemsSeas) VALUES (@User_ID, @Task_id, @done, @Stand,@Ask, @LastItemsSer, @LastItemsSeas);", con))
                {
                    command.Parameters.AddWithValue("@User_ID", Id);
                    command.Parameters.AddWithValue("@Task_id", taskId);
                    command.Parameters.AddWithValue("@done", done);
                    command.Parameters.AddWithValue("@Stand", stand);
                    command.Parameters.AddWithValue("@LastItemsSer", LastSer);
                    command.Parameters.AddWithValue("@LastItemsSeas", LastSeas);
                    command.Parameters.AddWithValue("@Ask", AskI);
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
        public void TaskDone()
        {
            Score++;
            SaveToSQl(true);
        }
        public void TaskWrong()
        {
            Score--;
            SaveToSQl();
        }
    }
    public class Task
    {
        public int Task_id { get; set; }
        public string Task_text { get; set; }
        public string Serial { get; set; }
        public string Season { get; set; }
        public string Series { get; set; }
    }
    public class User
    {
        public int User_ID { get; set; }
        public int Task_id { get; set; }
        public int done { get; set; }
        public int Score { get; set; }
        public int Stand { get; set; }
        public string Username { get; set; }
        public string LastSeries { get; set; }
        public string lastSeason { get; set; }
        public int ask { get; set; }
    }
    public class Top
    {
        public int Score { get; set; }
        public string Username { get; set; }
    }
    public class DBSeries
    {
        public int num { get; set; }
        public string series { get; set; }
    }
}
