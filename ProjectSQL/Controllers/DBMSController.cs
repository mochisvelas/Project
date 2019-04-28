using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace ProjectSQL.Controllers {

    public class DBMSController : Controller {

        // Reserved words dictionary
        private static Dictionary<string, List<string>> reservedWords = new Dictionary<string, List<string>>() {
            { "SELECT", new List<string>(){ "SELECT" } },
            { "FROM", new List<string>(){ "FROM" } },
            { "DELETE", new List<string>(){ "DELETE" } },
            { "WHERE", new List<string>(){ "WHERE" } },
            { "CREATE TABLE", new List<string>(){ "CREATE TABLE" } },
            { "DROP TABLE", new List<string>(){ "DROP TABLE" } },
            { "INSERT INTO", new List<string>(){ "INSERT INTO" } },
            { "VALUES", new List<string>(){ "VALUES" } },
            { "GO", new List<string>(){ "GO" } }
        };

        // Return the view to load the reserved words
        [HttpGet]
        public ActionResult LoadReservedWords() {
            return View();
        }

        // Try to read the file and save the data in the dictionary
        [HttpPost]
        public ActionResult LoadFile(HttpPostedFileBase reservedWords) {
            if (ValidateFile(reservedWords)) {
                ViewBag.Message = "success";
            } else {
                ViewBag.Message = "Ocurrio un error a la hora de cargar el archivo. Verifique que sea el correcto.";
            }
            return View("LoadReservedWords");
        }

        // Download the dictionarie as json
        [HttpPost]
        public FileResult DownloadDictionary() {
            return DownloadFile("Dictionary");
        }

        /// <summary>Validate and save the data in each file in the directories.</summary>
        /// <param name="file">The file with the reserved words.</param>
        /// <returns>A boolean with true if is succes.</returns>
        private bool ValidateFile(HttpPostedFileBase file) {
            bool value = false;
            if (file != null) {
                if (CheckExtension(file)) {
                    string path = StoreFile(file);
                    try {
                        if (LoadData(path)) {
                            value = true;
                        }
                    } catch (Exception) {
                        return false;
                    }
                }
            }
            return value;
        }

        /// <summary>Check the extension of the file.</summary>
        /// <param name="file">The file to check the extension</param>
        /// <returns>A boolean with true if is succes.</returns>
        private bool CheckExtension(HttpPostedFileBase file) {
            if (".csv".Equals(Path.GetExtension(file.FileName), StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        /// <summary>Store the file in the server</summary>
        /// <param name="file">The file to store.</param>
        /// <returns>The path of the file.</returns>
        private string StoreFile(HttpPostedFileBase file) {
            string name = Path.GetFileName(file.FileName);
            string path = Path.Combine(Server.MapPath("~/App_Data/"), name);
            file.SaveAs(path);
            return path;
        }

        /// <summary>Try to save the data in the dictionary.</summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A boolean with true if is succes.</returns>
        private bool LoadData(string path) {
            bool value = false;
            StreamReader reader = new StreamReader(path);
            reader.ReadLine();
            Dictionary<string, List<string>> newWords = new Dictionary<string, List<string>>();
            string line;
            while ((line = reader.ReadLine()) != null) {
                string[] items = line.Split(',');
                if (SaveData(items, ref newWords)) {
                    value = true;
                    reservedWords = newWords;
                } else {
                    reader.Close();
                    return false;
                }
            }
            reader.Close();
            System.IO.File.Delete(path);
            return value;
        }

        /// <summary>Create and object and save in the dictionary.</summary>
        /// <param name="items">The items to save.</param>
        /// <param name="dictionary">The dictionary to save the words</param>
        private bool SaveData(string[] items, ref Dictionary<string, List<string>> dictionary) {
            try {
                if (dictionary.ContainsKey(items[0])) {
                    List<string> words = dictionary[items[0]];
                    words.Add(items[1]);
                    dictionary[items[0]] = words;
                } else {
                    dictionary.Add(items[0], new List<string>() { items[1] });
                }
                return true;
            } catch (Exception) {
                dictionary.Clear();
                return false;
            }
        }


        /// <summary>Create the file and download them.</summary>
        /// <param name="name">The name of the file.</param>
        /// <returns>The file to download.</returns>
        private FileResult DownloadFile(string name)
        {
            string fileName = name + ".json";
            string path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
            if (name.Equals("Dictionary")) {
                System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(reservedWords, Formatting.Indented));
            }
            FileContentResult file = File(System.IO.File.ReadAllBytes(path), "application/octet-stream", fileName);
            System.IO.File.Delete(path);
            return file;
        }

    }

}
