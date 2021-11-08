using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace HTML_Editor.Controllers
{
    
    public class RunController : Controller
    {
        private const int MaxLenID = 8;//Max Lenght For UniqeID
        private const int FSizeLimitInBytes = 52 * 428* 800;//Max File Size Definition
        string ConnectionSTR = "";
        public string Text = "";
        public static string URL_ID_Session;
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Index(string BTNcommand,string DropDown, Models.HtmlTxtModel ModelControls)//always update on refresh
        {
            ViewBag.WarningText = "HTML Session ID: " + URL_ID_Session;
            DeleteTempFile(URL_ID_Session);
            switch (BTNcommand)
            {
                case "SaveBTN":
                    int Result = WriteFileToServer(ModelControls.TxtEditorText);
                    if (Result == -2)//File Size Too Large
                    {
                        ViewBag.WarningText = "[ERROR] File Size Is Too Large!";
                        ViewBag.WarningText2 = "Maximum Lenght Allowed Is 50MB";
                        break;
                    }
                    else
                    {
                        ViewBag.WarningText2 = "Last Save Time: " + DateTime.Now.ToString("HH:mm:ss");
                        RouteData.Values["URLGen"] = URL_ID_Session;
                        return RedirectToAction("Index", "Run");
                    }  
                case "RunBTN":
                    WriteTempFileToServer(ModelControls.TxtEditorText);
                    //load The file into View
                    ModelControls.OpenHTMLFIle = GetFileMapPathForIFrame(URL_ID_Session,true);
                    break;
                case "Show_Hide_Settings":
                    ModelControls.ShowHideVal = !ModelControls.ShowHideVal;
                    break;
                case "CloseDropDown":
                    ModelControls.ShowHideVal = false;
                    break;
                case "EnterHTMLURL":
                        ModelControls.ShowURLTextBox = !ModelControls.ShowURLTextBox;
                    break;
                case "GenerateURL":
                    URL_ID_Session = GenerateRandomFileID();
                    RouteData.Values["URLGen"] = URL_ID_Session;
                    return RedirectToAction("Index", "Run");//Refresh so we can update COntrol for new ID
                case "ShareURL":
                    if (System.IO.File.Exists(GetFileMapPath(URL_ID_Session)) == true)
                    {
                        ViewBag.WarningText = "Copy The Bellow URL To Share This Project:";
                        ViewBag.WarningText2 = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + "/" + URL_ID_Session;
                        ModelControls.OpenHTMLFIle = GetFileMapPathForIFrame(URL_ID_Session, false);
                    }
                    else//make sure they cant share a non save project
                    {
                        ViewBag.WarningText = "The Project Is Not Saved!";
                        ViewBag.WarningText2 = "Press The 'SAVE' Button First Before Sharing A Project";
                    }
                    break;
                case "EditBTN":
                    int Result2 = WriteCopyFileToServer(ModelControls.TxtEditorText);//Everything is handled in this function...
                    if(Result2 == -2)
                    {
                        ViewBag.WarningText = "[ERROR] File Size Is Too Large!";
                        ViewBag.WarningText2 = "Maximum Lenght Allowed Is 50MB";
                        break;
                    }
                    else return RedirectToAction("Index", "Run");//Just Refresh so the ID updates to the new one
                case "RevertBTN":
                    URL_ID_Session = ModelControls.ShowOriginalFileID;
                    RouteData.Values["URLGen"] = ModelControls.ShowOriginalFileID;
                    return RedirectToAction("Index", "Run");//Refresh so we can update COntrol for new ID
                case "NextFileBTN":
                    URL_ID_Session = ModelControls.ShowNextFileID;
                    RouteData.Values["URLGen"] = ModelControls.ShowNextFileID;
                    return RedirectToAction("Index", "Run");//Refresh so we can update COntrol for new ID
            }

            return View("Index", ModelControls);
        }
        [HttpGet]
        public ActionResult Index(string BTNcommand,Models.HtmlTxtModel ModelControls)
        {
            if (ModelControls.TxtEditorText == null)//the Editor has not yet been accessed, meaning it has //[JUST LOADED]\\
            {
                ModelControls.TxtEditorText = "Input Html Code Here";//solves refresh page on losing text
            }
            if (RouteData.Values["URLGen"] == null) RouteData.Values["URLGen"] = URL_ID_Session;
            if (String.Equals(RouteData.Values["URLGen"].ToString(),URL_ID_Session))//Default session
            {
                
                ViewBag.WarningText = "Your Edit ID: ["+ RouteData.Values["URLGen"]+"] If You Would Like To Start A Different Project";
                ViewBag.WarningText2 = "Go to 'Settings > Generate NEW URL";
                //load The file into View
                if (System.IO.File.Exists(GetFileMapPath(URL_ID_Session).Replace(".html", "-TEMP.html")) == true)//This Temp File, Dont show Edit option
                {
                    ModelControls.TxtEditorText = GetFiledata(URL_ID_Session);
                    ModelControls.OpenHTMLFIle = GetFileMapPathForIFrame(URL_ID_Session);
                }
                else if (System.IO.File.Exists(GetFileMapPath(URL_ID_Session)) == true)//User is working on Saved project
                {
                    //load The file into View
                    EnableFileEditView(ModelControls);
                }
            }
            else //User entered Custom URL ID
            {
                if (RouteData.Values["URLGen"].ToString().Length > MaxLenID | RouteData.Values["URLGen"].ToString().Length < MaxLenID)//Max Lenght for the Uniqe ID i chose to be 8chars long
                {
                    ViewBag.ErrorText1 = "[ERROR] Invalid URL ID, Incorrect Lenght!";
                    ViewBag.WarningText = "ID: " + RouteData.Values["URLGen"] + " Is Invalid!";
                    ViewBag.WarningText2 = "Go to 'Settings > Generate NEW URL";
                    ModelControls.ShowErrorBox = true;//Display the custom Error fields
                    ModelControls.ShowMainBodyField = false;
                }
                else//Custom URL/ID of a Saved HTML has been enter, we need to load it
                {
                    int userCount;string ConnectionSTR = ConfigurationManager.ConnectionStrings["myConnectionT"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(ConnectionSTR))
                    {
                        SqlCommand cmd = new SqlCommand("SELECT COUNT(*) from UniqueIDTable WHERE HtmFileID = '" + RouteData.Values["URLGen"] + "';");
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Connection = connection;
                        connection.Open();
                        userCount = (int)cmd.ExecuteScalar();
                        connection.Close();
                    }
                    if (userCount > 0)//The Data Exists Load It
                    {
                        EnableFileEditView(ModelControls);
                    }//data Does not exist
                    else
                    {
                        ViewBag.ErrorText1 = "[ERROR] Invalid URL ID, The Data Entered Does Not Exist!";
                        ViewBag.WarningText = "ID' :" + RouteData.Values["URLGen"] + "Is Invalid!";
                        ViewBag.WarningText2 = "Go to 'Settings > Generate NEW URL To Start A New Session";
                        ModelControls.ShowErrorBox = true;//Display the custom Error fields
                        ModelControls.ShowMainBodyField = false;
                    }
                    
                }
            }
            return View("Index", ModelControls);
        }

        /// /---------------------Quick Handlers---------------------
        public void EnableFileEditView(Models.HtmlTxtModel ModelControls)
        {
            string ConnectionSTR = ConfigurationManager.ConnectionStrings["myConnectionT"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(ConnectionSTR))
            {
                SqlCommand cmd = new SqlCommand("SELECT * from UniqueIDTable WHERE HtmFileID = @HtmFileID");
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = connection;
                connection.Open();
                cmd.Parameters.AddWithValue("@HtmFileID", RouteData.Values["URLGen"]);
                using (SqlDataReader oReader = cmd.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        ModelControls.ShowOriginalFileID = oReader["BeforeEditChildID"].ToString();//Store the Original file so user can go back to it
                        ModelControls.ShowNextFileID = oReader["AfterEditChildID"].ToString();//Store the Original file so user can go to next File if any
                    }
                }
                connection.Close();
            }

            ModelControls.ShowEditOption = true;//show Extra Edit options
            ViewBag.WarningText = "LOADED EDIT ID: [" + RouteData.Values["URLGen"] + "] If You Would Like To Start A New Project";
            ViewBag.WarningText2 = "Go to 'Settings > Generate NEW URL";
            URL_ID_Session = RouteData.Values["URLGen"].ToString();
            //load The file into View
            ModelControls.TxtEditorText = GetFiledata(URL_ID_Session);
            ModelControls.OpenHTMLFIle = GetFileMapPathForIFrame(URL_ID_Session);
            
        }
        /// <summary>
        /// /---------------------Functions---------------------
        /// </summary>

        /// <summary>
        /// /---------------------Functions---------------------------------------[WriteFileToServer]-----------------------
        /// </summary>
        public int WriteFileToServer(string TextToWrite)
            {
            FileStream Fstream = new FileStream(GetFileMapPath(URL_ID_Session), FileMode.Create);
            StreamWriter streanW = new StreamWriter(Fstream);
            using (var writer = new StreamWriter(Fstream))
            {
                writer.AutoFlush = true;//so the writer updates the BaseStream
                foreach (char Chars in TextToWrite)
                {
                    writer.Write(Chars);
                    if (writer.BaseStream.Length > FSizeLimitInBytes)//File Size is now going above the Size Limit we need to Cancel the operation
                    {
                        writer.Flush();
                        writer.Close();
                        Fstream.Close();
                        return -2;
                    }
                }
            }
            Fstream.Close();
            //store this HTML file to database
            ConnectionSTR = ConfigurationManager.ConnectionStrings["myConnectionT"].ConnectionString;
            int userCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionSTR))
            {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) from UniqueIDTable WHERE HtmFileID = '" + RouteData.Values["URLGen"] + "';");
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = connection;
                connection.Open();
                userCount = (int)cmd.ExecuteScalar();
                if (userCount > 0)//allready exist just update the time edited
                {
                    cmd.CommandText = "UPDATE UniqueIDTable SET LastEditTime = @LastEditTime Where HtmFileID = @HtmFileID";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@HtmFileID", URL_ID_Session);
                    cmd.Parameters.AddWithValue("@LastEditTIme", DateTime.Now.ToString("HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
                else//Creat new Item as it doesnt exist
                {
                    cmd.CommandText= "INSERT INTO UniqueIDTable (HtmFileID, SaveTime,BeforeEditChildID,AfterEditChildID,LastEditTime,ISOriginal) VALUES "+
                                     "(@HtmFileID, @SaveTime, @BeforeEditChildID,@AfterEditChildID,@LastEditTime,@ISOriginal)";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@HtmFileID", URL_ID_Session);
                    cmd.Parameters.AddWithValue("@SaveTime", DateTime.Now.ToString("HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@BeforeEditChildID", "noneVALX");
                    cmd.Parameters.AddWithValue("@AfterEditChildID", "noneVALX");
                    cmd.Parameters.AddWithValue("@LastEditTime", "");
                    cmd.Parameters.AddWithValue("@ISOriginal", "1");//Original First Saved edit
                    cmd.ExecuteNonQuery();
                    
                }
                connection.Close();;
            }
            return 1;
        }
        /// <summary>
        /// /---------------------Functions---------------------------------------[WriteCopyFileToServer]-----------------------
        /// </summary>
        public int WriteCopyFileToServer(string TextToWrite)
        {
            //First Re-Generate a NEW ID for te Child Sesion:
            string New_SesionID = GenerateRandomFileID();
            //
            //We Are not going to Re-Save The Original as the user can do that manually...
            //But if we want we can just do: "WriteFileToServer(TextToWrite);" It Will Write the current Sesion ID
            //
            FileStream Fstream = new FileStream(GetFileMapPath(New_SesionID), FileMode.Create);
            StreamWriter streanW = new StreamWriter(Fstream);
            using (var writer = new StreamWriter(Fstream))
            {
                writer.AutoFlush = true;//so the writer updates the BaseStream
                foreach (char Chars in TextToWrite)
                {
                    writer.Write(Chars);
                    if (writer.BaseStream.Length > FSizeLimitInBytes)//File Size is now going above the Size Limit we need to Cancel the operation
                    {
                        writer.Flush();
                        writer.Close();
                        Fstream.Close();
                        return -2;
                    }
                }
            }
            Fstream.Close();
            //store this NEW HTML file to database
            ConnectionSTR = ConfigurationManager.ConnectionStrings["myConnectionT"].ConnectionString;
            int userCount = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionSTR))
            {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) from UniqueIDTable WHERE HtmFileID = '" + RouteData.Values["URLGen"] + "';");
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = connection;
                connection.Open();
                userCount = (int)cmd.ExecuteScalar();
                if (userCount > 0)//allready exist just update the Perent HTML Edit so we can trece back
                {
                    cmd.CommandText = "UPDATE UniqueIDTable SET AfterEditChildID = @AfterEditChildID001 Where HtmFileID = @HtmFileID001";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@HtmFileID001", URL_ID_Session);
                    cmd.Parameters.AddWithValue("@AfterEditChildID001", New_SesionID);//Make sure to write The New Generate ID
                    cmd.ExecuteNonQuery();
                }
                else{ }//Crytical Error as this Data should exist!!!
                //Now To Add the New Edit ID to the database
                cmd.CommandText = "INSERT INTO UniqueIDTable (HtmFileID, SaveTime,BeforeEditChildID,AfterEditChildID,LastEditTime,ISOriginal) VALUES " +
                                  "(@HtmFileID, @SaveTime, @BeforeEditChildID,@AfterEditChildID,@LastEditTime,@ISOriginal)";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = connection;
                cmd.Parameters.AddWithValue("@HtmFileID", New_SesionID);
                cmd.Parameters.AddWithValue("@SaveTime", DateTime.Now.ToString("HH:mm:ss"));
                cmd.Parameters.AddWithValue("@BeforeEditChildID", URL_ID_Session);
                cmd.Parameters.AddWithValue("@AfterEditChildID", "noneVALX");
                cmd.Parameters.AddWithValue("@LastEditTime", "");
                cmd.Parameters.AddWithValue("@ISOriginal", "0");//Make sure we State its not the original Edit
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            //Just Update The Sesion ID/s so we can refresh after...
            URL_ID_Session = New_SesionID;
            RouteData.Values["URLGen"] = New_SesionID;
            return 1;
        }
        public void WriteTempFileToServer(string TextToWrite)
        {
            FileStream fs = new FileStream(GetFileMapPath(URL_ID_Session).Replace(".html", "-TEMP.html"), FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(TextToWrite);
            sw.Close();
            fs.Close();
        }
        public void DeleteTempFile(string HtmlID)
        {
            string physicalPath = GetFileMapPath(HtmlID).Replace(".html", "-TEMP.html");
            if (System.IO.File.Exists(physicalPath) == true) System.IO.File.Delete(physicalPath);
        }
        private static Random random = new Random();
        public static string GenerateRandomFileID()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string rand = new string(Enumerable.Repeat(chars, MaxLenID).Select(s => s[random.Next(s.Length)]).ToArray());
            string physicalPath = GetFileMapPath(rand);
            if (System.IO.File.Exists(physicalPath) == true) return GenerateRandomFileID();//if by some chance the random string already exists.
            return rand;
        }
        public static string GetFiledata(string HtmlID)
        {
            string physicalPath = GetFileMapPath(HtmlID);
            if (System.IO.File.Exists(physicalPath) == true) return System.IO.File.ReadAllText(physicalPath);
            return null;
        }
        public static string GetFileMapPath(string HtmlID)
        {
            return System.Web.Hosting.HostingEnvironment.MapPath("~/HtmlFiles/" + "Edit-" + HtmlID + ".html");
        }
        public static string GetFileMapPathForIFrame(string HtmlID,bool IsTemo=false)
        {
            if(IsTemo==true) return "HtmlFiles/" + "Edit-" + HtmlID + "-TEMP.html";
            return "HtmlFiles/" + "Edit-" + HtmlID + ".html";
        }
        
        /*
        [ChildActionOnly]
        public ActionResult GetHtmlPage(string path)
        {
            return new FilePathResult(path, "text/html");
        }*/
    }
}