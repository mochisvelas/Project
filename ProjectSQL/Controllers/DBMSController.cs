using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            ViewBag.Message = "null";
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

        // Reset the dictionary to the default values
        [HttpPost]
        public ActionResult ResetDictionary() {
            try {
                DefaultDictionary();
                ViewBag.Message = "success";
            } catch (Exception) {
                ViewBag.Message = "Lo sentimos no se pudo cargar el diccionario a sus valores por defecto.";
            }
            return View("LoadReservedWords");
        }

        // Add a new word to the dictionarie
        [HttpPost]
        public ActionResult AddNewWord(string option, string newWord) {
            if(AddWord(option, newWord)) {
                ViewBag.Message = "success";
            } else {
                ViewBag.Message = "Ocurrio un error. La palabra ya esta guardada en el diccionario.";
            }
            return View("LoadReservedWords");
        }
        
        // Return the view for the sql console
        [HttpGet]
        public ActionResult SQLCode() {
            ViewBag.Message = "null";
            return View();
        }

        // Return a list of the words to highlight
        [HttpGet]
        public JsonResult MatchWords(string text) {
            if (!string.IsNullOrEmpty(text)) {
                string[] words = text.ToUpper().Split(' ', '\n');
                List<string> list = new List<string>();
                if(FindWords(words, ref list)) {
                    return Json(new { words = list }, JsonRequestBehavior.AllowGet);
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        //Get the action to do with the database.
        [HttpPost]
        public ActionResult SQLCode(string text) {
            if(string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) {
                ViewBag.Message = "No pudimos realizar ninguna acción.";
            } else {
                ViewBag.Message = CheckCommand(text.Trim());
            }
            return View();
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
                if (dictionary.ContainsKey(items[0].ToUpper())) {
                    List<string> words = dictionary[items[0].ToUpper()];
                    words.Add(items[1].ToUpper());
                    dictionary[items[0].ToUpper()] = words;
                } else {
                    dictionary.Add(items[0].ToUpper(), new List<string>() { items[1].ToUpper() });
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
        private FileResult DownloadFile(string name) {
            string fileName = name + ".json";
            string path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
            if (name.Equals("Dictionary")) {
                System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(reservedWords, Formatting.Indented));
            }
            FileContentResult file = File(System.IO.File.ReadAllBytes(path), "application/octet-stream", fileName);
            System.IO.File.Delete(path);
            return file;
        }

        /// <summary>Load the default values in the dictionarie</summary>
        private void DefaultDictionary() {
            reservedWords = new Dictionary<string, List<string>>() {
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
        }

        /// <summary>Add the new word in the dictionary if dosn't exist.</summary>
        /// <param name="option">The command to check.</param>
        /// <param name="word">The reserved word to add.</param>
        /// <returns>True if the word has been added.</returns>
        private bool AddWord(string option, string word) {
            if (reservedWords.ContainsKey(option)) {
                if (!InDictionary(word)) {
                    reservedWords[option].Add(word.ToUpper());
                    return true;
                }
            }
            return false;
        }

        /// <summary>Check if the word is in the dictionary.</summary>
        /// <param name="word">The word to check.</param>
        /// <returns>True if the word is in the dictionary.</returns>
        private bool InDictionary(string word) {
            foreach(KeyValuePair<string, List<string>> words in reservedWords) {
                if (words.Value.Contains(word.ToUpper())) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Found the reserved words in the text.</summary>
        /// <param name="words">The text to check.</param>
        /// <param name="list">The list to save the words to highlight.</param>
        /// <returns>True if find words</returns>
        private bool FindWords(string[] words, ref List<string> list) {
            bool value = false;
            foreach(KeyValuePair<string, List<string>> commnad in reservedWords) {
                foreach(string reservedWord in commnad.Value) {
                    List<string> reservedWords = new List<string>(reservedWord.Split(' '));
                    for(int i = 0; i < words.Length; i++) {
                        foreach(string word in reservedWords) {
                            if (words[i].Equals(word)) {
                                if (!list.Contains(words[i])) {
                                    list.Add(words[i]);
                                }
                                value = true;
                            }
                        }
                    }
                }
            }
            return value;
        }


        private string CheckCommand(string text) {
            string message = string.Empty;
            string command = NormalizeText(text);
            CheckCreateTable(command.ToUpper(), ref message);
            return message;
        }

        /// <summary>Check if the syntax for create a table is correct.</summary>
        /// <param name="command">The command to check the syntax.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the syntax is correct.</returns>
        private bool CheckCreateTable(string command, ref string message) {
            bool value = false;
            List<string> words = reservedWords["CREATE TABLE"];
            string pattern = string.Empty;
            foreach(string word in words) {
                pattern = word + " [a-zA-Z0-9]+ {";
                Match match = Regex.Match(command, pattern);
                if (!match.Success) {
                    message = "El nombre de la tabla solo puede ser una palabra y debe de ser seguido por {";
                } else {
                    if (command[command.Length - 1].Equals('}')) {
                        Match attributes = Regex.Match(command, @"\{(.*?)\}");
                        if (attributes.Success) {
                            string normalizedAttributes = attributes.Value;
                            string aux = normalizedAttributes.Replace("{", "");
                            normalizedAttributes = aux.Replace("}", "");
                            List<string> atts = new List<string>(normalizedAttributes.Split(','));
                            if(atts.Any(x => x.Contains("INT PRIMARY KEY"))) {
                                foreach(string attribute in atts) {
                                    try {
                                        if(attribute.Contains("INT") || attribute.Contains("VARCHAR(100)") || attribute.Contains("DATETIME")) {
                                        } else {
                                            message = "Los unicos tipos de datos de las columnas pueden ser int, varchar(100) y datetime";
                                            break;
                                        }
                                    } catch (Exception) {
                                        message = "Ocurrio un error al momento de ejecutar el proceso. Intentalo mas tarde.";
                                        break;
                                    }
                                }
                            } else {
                                message = "Debes de agregar una columna de tipo int primary key";
                                break;
                            }       
                        }
                    } else {
                        message = "El comando debe de terminar con }";
                        break;
                    }
                }
            }
            return value;
        }

        /// <summary>Normalize the text in one line.</summary>
        /// <param name="text">The text to normalize.</param>
        /// <returns>The text</returns>
        private string NormalizeText(string text) {
            List<string> words = new List<string>(text.Split(' ', '\n'));
            words.RemoveAll(x => x == "" || x == "\n" || x == "\r");
            string newText = string.Join(" ", words.ToArray());
            newText = newText.Replace("\r", "");
            return newText;
        }

    }

}
