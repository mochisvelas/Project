using DataStructures;
using Newtonsoft.Json;
using ProjectSQL.Models;
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

        // Dictionarie for the tables
        private static Dictionary<string, Table> tables = new Dictionary<string, Table>();

        // List of the querys used.
        private static List<string> querys = new List<string>();

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

        // Return the view of the tables
        [HttpGet]
        public ActionResult Tables() {
            if(tables.Count == 0) {
                ViewBag.AreTables = "no";
            } else {
                ViewBag.AreTables = "yes";
                ViewBag.Tables = TablesName();
            }
            return View();
        }

        // Return the columns of a table
        public JsonResult TableColumns(string name) {
            if (name.Equals("undefined")) {
                return Json(null);
            } else {
                List<List<string>> list = Columns(name);
                return Json(new { names = list[0], types = list[1] }, JsonRequestBehavior.AllowGet);
            }
        }

        // Return the view for the querys
        [HttpGet]
        public ActionResult Querys() {
            if (querys.Count == 0) {
                ViewBag.AreQuerys = "no";
            } else {
                ViewBag.AreQuerys = "yes";
                ViewBag.Querys = querys;
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
        
        /// <summary>Check if the command is ok.</summary>
        /// <param name="text">The text to check.</param>
        /// <returns>The message to send back.</returns>
        private string CheckCommand(string text) {
            string message = string.Empty;
            string command = NormalizeText(text);
            if(CheckCreateTable(command.ToUpper(), ref message)) {
                querys.Add(text);
                return message;
            } else {
                message = string.Empty;
                if(CheckInsert(command.ToUpper(), ref message)) {
                    querys.Add(text);
                    return message;
                } else {
                    message = "No se pudo realizar la accion.";
                    return message;
                }
            }
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
        
        /// <summary>Check if the syntax for create a table is correct.</summary>
        /// <param name="command">The command to check the syntax.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the syntax is correct.</returns>
        private bool CheckCreateTable(string command, ref string message) {
            List<string> words = reservedWords["CREATE TABLE"];
            return CheckTablePattern(command, ref message, words);
        }
        
        /// <summary>Check if is a pattern match</summary>
        /// <param name="command">The command to check the pattern</param>
        /// <param name="message">The message to send back.</param>
        /// <param name="words">The list of reserved words.</param>
        /// <returns>True if is a pattern.</returns>
        private bool CheckTablePattern(string command, ref string message, List<string> words) {
            bool value = false;
            string pattern = string.Empty;
            foreach (string word in words) {
                pattern = word + " [a-zA-Z0-9]+ {";
                Match match = Regex.Match(command, pattern);
                if (!match.Success) {
                    message = "El nombre de la tabla solo puede ser una palabra y debe de ser seguido por {";
                    value = false;
                } else {
                    if (command[command.Length - 1].Equals('}')) {
                        List<string> w = new List<string>(match.Value.Split(' '));
                        string tableName = w[w.Count - 2];
                        bool nameReserved = false;
                        foreach(KeyValuePair<string, List<string>> reservedword in reservedWords) {
                            foreach(string _w_ in reservedword.Value) {
                                if (tableName.Equals(_w_) || tableName.Equals("INT") || tableName.Equals("VARCHAR(100)") || tableName.Equals("DATETIME")) {
                                    nameReserved = true;
                                }
                            }
                        }
                        if (nameReserved) {
                            message = "El nombre no puede ser una palabra reservada";
                            value = false;
                        } else {
                            value = GetContent(command, tableName, ref message);
                        }
                    } else {
                        message = "El comando debe de terminar con }";
                        value = false;
                    }
                }
            }
            return value;
        }

        /// <summary>Get the value inside brackets.</summary>
        /// <param name="command">The command to get the content.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the content is find.</returns>
        private bool GetContent(string command, string name, ref string message) {
            bool value = false;
            if(!tables.ContainsKey(name)) {
                Match attributes = Regex.Match(command, @"\{(.*?)\}");
                if (attributes.Success) {
                    string normalizedAttributes = NormalizeAttributes(attributes);
                    List<string> atts = new List<string>(normalizedAttributes.Split(','));
                    if (atts.Any(x => x.Contains("INT PRIMARY KEY"))) {
                        value = GetColumns(atts, name, ref message);
                    } else {
                        message = "Debes de agregar una columna de tipo int primary key";
                        value = false;
                    }
                } else {
                    message = "El comando debe de terminar con }";
                    value = false;
                }
            } else {
                message = "Ya existe una tabla con ese nombre.";
                value = false;
            }
            return value;
        }

        /// <summary>Get the data for each column</summary>
        /// <param name="attributes">The list of attributes for each column.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the structure of the data is ok.</returns>
        private bool GetColumns(List<string> attributes, string name, ref string message) {
            bool value = false;
            List<KeyValuePair<string, string>> columns = new List<KeyValuePair<string, string>>();
            foreach (string attribute in attributes) {
                if (!attribute.Equals(" ")) {
                    try {
                        string att = attribute.Trim();
                        if (attribute.Contains("INT") || attribute.Contains("VARCHAR(100)") || attribute.Contains("DATETIME")) {
                            if (attribute.Contains("PRIMARY KEY")) {
                                value = MakeTable("primary", attribute, ref columns, ref message);
                            } else {
                                value = MakeTable("field", attribute, ref columns, ref message);
                            }
                        } else {
                            message = "Los unicos tipos de datos de las columnas pueden ser int, varchar(100) y datetime";
                            value = false;
                        }
                    } catch (Exception) {
                        message = "Ocurrio un error al momento de ejecutar el proceso. Intentalo mas tarde.";
                        value = false;
                    }
                }
            }
            if (value) {
                Table table = new Table() { columns = columns, data = new BPlusTree<List<KeyValuePair<string, string>>, int>(4, 4) };
                tables.Add(name, table);
                CreateBTreeFile(name);
                CreateTableFile(name);
            }
            return value;
        }

        /// <summary>Remove the brackets in the attributes.</summary>
        /// <param name="attributes">The attributes to normalaize.</param>
        /// <returns>A string with the new attributes.</returns>
        private string NormalizeAttributes(Match attributes) {
            string normalizedAttributes = attributes.Value;
            string aux = normalizedAttributes.Replace("{", "");
            normalizedAttributes = aux.Replace("}", "");
            return normalizedAttributes;
        }
        
        /// <summary>Check what type of column is going to add.</summary>
        /// <param name="type">The type of the column</param>
        /// <param name="attribute">The data for the column.</param>
        /// <param name="columns">The list to save the columns.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the column is created.</returns>
        private bool MakeTable(string type, string attribute, ref List<KeyValuePair<string, string>> columns, ref string message) {
            bool value = false;
            List<string> helper = new List<string>(attribute.Trim().Split(' '));
            if (type.Equals("primary")) {
                if (helper.Count > 4) {
                    message = "Los nombres de las columnas solo pueden ser de una palabra";
                    value = false;
                } else if(helper.Count == 4) {
                    string name = helper[0];
                    value = AddColumn(name, helper, ref columns, ref message);
                } else {
                    message = "La columna debe de poseer algun tipo";
                    value = false;
                }
            } else {
                if (helper.Count > 2) {
                    message = "Los nombres de las columnas solo pueden ser de una palabra";
                    value = false;
                } else if(helper.Count == 2) {
                    string name = helper[0];
                    value = AddColumn(name, helper, ref columns, ref message);
                } else {
                    message = "La columna debe de poseer algun tipo";
                    value = false;
                }
            }
            return value;
        }

        /// <summary>Add a new column to the table.</summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="words">The type of the column.</param>
        /// <param name="columns">The list to save the columns.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the column is added.</returns>
        private bool AddColumn(string name, List<string> words, ref List<KeyValuePair<string, string>> columns, ref string message) {
            bool value = false;
            bool sameName = false;
            foreach(KeyValuePair<string, string> item in columns) {
                if (name.Equals(item.Key)) {
                    sameName = true;
                }
            }
            if (sameName) {
                value = false;
                message = "Los nombres de las columnas no pueden estar repetidos";
            } else {
                if (NameIsReserved(name)) {
                    message = "Los nombres de las columnas no pueden ser palabras reservadas";
                    value = false;
                } else {
                    words.RemoveAt(0);
                    KeyValuePair<string, string> column = new KeyValuePair<string, string>(name, string.Join(" ", words.ToArray()));
                    columns.Add(column);
                    message = "success";
                    value = true;
                }
            }
            return value;
        }

        /// <summary>Check if the name is a reserved word.</summary>
        /// <param name="name">The name to check</param>
        /// <returns>True if is a reserved word.</returns>
        private bool NameIsReserved(string name) {
            if (name.Equals("INT") || name.Equals("VARCHAR(100)") || name.Equals("DATETIME") || name.Equals("PRIMARY") || name.Equals("KEY"))
                return true;
            return false;
        }

        /// <summary>Create a file with the elements of the table.</summary>
        /// <param name="name">The name of the table.</param>
        private void CreateBTreeFile(string name) {
            string directory = @"C:\microSQL\arbolesb";
            string fileName = name.Trim() + ".arbolb";
            string text = string.Empty;
            if (!tables[name].data.IsEmpty()) {
                text = JsonConvert.SerializeObject(tables[name].data.ToList(), Formatting.Indented);
            }
            CreateFile(directory, fileName, text);
        }

        /// <summary>Create a file with the columns of the table.</summary>
        /// <param name="name">The name of the table</param>
        private void CreateTableFile(string name) {
            string directory = @"C:\microSQL\tablas";
            string fileName = name.Trim() + ".tabla";
            string text = string.Empty;
            foreach(KeyValuePair<string, string> column in tables[name].columns) {
                text += "Nombre: " + column.Key + ", Tipo: " + column.Value + "\n";
            }
            CreateFile(directory, fileName, text);
        }

        /// <summary>Create a file specified.</summary>
        /// <param name="directory">The directory to the file.</param>
        /// <param name="file">The name of the file.</param>
        /// <param name="text">The content of the file.</param>
        private void CreateFile(string directory, string file, string text) {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            System.IO.File.WriteAllText(Path.Combine(directory, file), text);
        }

        /// <summary>Save the name of all the tables in a list.</summary>
        /// <returns>A list with the tables name.</returns>
        private List<string> TablesName() {
            List<string> names = new List<string>();
            foreach(KeyValuePair<string, Table> table in tables) {
                names.Add(table.Key);
            }
            return names;
        }

        /// <summary>Get the name and the type of each column in a table.</summary>
        /// <param name="name">The name of the table</param>
        /// <returns>The list with the names and the types</returns>
        private List<List<string>> Columns(string name) {
            List<string> names = new List<string>();
            List<string> types = new List<string>();
            List<KeyValuePair<string, string>> columns = tables[name].columns;
            foreach(KeyValuePair<string, string> column in columns) {
                names.Add(column.Key);
                types.Add(column.Value);
            }
            List<List<string>> list = new List<List<string>>();
            list.Add(names);
            list.Add(types);
            return list;
        }

        /// <summary>Check if the syntax for insert values is correct.</summary>
        /// <param name="command">The command to check the syntax.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the syntax is correct.</returns>
        private bool CheckInsert(string command, ref string message) {
            List<string> words = reservedWords["INSERT INTO"];
            return CheckInsertPattern(command, ref message, words);
        }

        /// <summary>Check if is a pattern match</summary>
        /// <param name="command">The command to check the pattern</param>
        /// <param name="message">The message to send back.</param>
        /// <param name="words">The list of reserved words.</param>
        /// <returns>True if is a pattern.</returns>
        private bool CheckInsertPattern(string command, ref string message, List<string> words) {
            bool value = false;
            string pattern = string.Empty;
            foreach (string word in words) {
                pattern = word + " [a-zA-Z0-9]+ {";
                Match match = Regex.Match(command, pattern);
                if (!match.Success) {
                    message = "El nombre de la tabla solo puede ser una palabra y debe de ser seguido por {";
                    value = false;
                } else {
                    string newPattern = "} VALUES {";
                    Match matchValues = Regex.Match(command, newPattern);
                    if (!matchValues.Success) {
                        message = "El comando debe de contener la instruccion VALUES y debe de ser seguido por {";
                        value = false;
                    } else {
                        if (command[command.Length - 1].Equals('}')) {
                            List<string> w = new List<string>(match.Value.Split(' '));
                            string tableName = w[w.Count - 2];
                            value = GetColumns(command, tableName, ref message);
                        } else {
                            message = "El comando debe de terminar con }";
                            value = false;
                        }
                    }
                }
            }
            return value;
        }

        /// <summary>Get the data for each column</summary>
        /// <param name="command">The command to get the data.</param>
        /// <param name="name">The name of the table.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the structure of the data is ok.</returns>
        private bool GetColumns(string command, string name, ref string message) {
            bool value = false;
            if (tables.ContainsKey(name)) {
                Match attributes = Regex.Match(command, @"\{(.*?)\}");
                if (attributes.Success) {
                    string normalizedAttributes = NormalizeAttributes(attributes);
                    List<string> atts = new List<string>(normalizedAttributes.Split(','));
                    if(tables[name].columns.Count == atts.Count) {
                        bool areDifferent = false;
                        int index = 0;
                        foreach (KeyValuePair<string, string> column in tables[name].columns) {
                            if (!column.Key.Equals(atts[index].Trim()))
                                areDifferent = true;
                            index++;
                        }
                        if (areDifferent) {
                            message = "Las columnas ingresadas no son las correctas";
                            value = false;
                        } else {
                            Match data = Regex.Match(command, @"VALUES \{(.*?)\}");
                            if (data.Success) {
                                GetData(data.Value, name, ref message);
                            } else {
                                message = "No se encontro la instruccion VALUES";
                                value = false;
                            }
                        }
                    } else {
                        message = "Las columnas ingresadas no son las correctas";
                        value = false;
                    }
                } else {
                    message = "No se han encotrado los datos de la tabla";
                    value = false;
                }
            } else {
                message = "No hay ninguna tabla con ese nombre.";
                value = false;
            }
            return value;
        }

        /// <summary>Get the data for each column</summary>
        /// <param name="command">The command to get the data.</param>
        /// <param name="name">The name of the table.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if the structure of the data is ok.</returns>
        private bool GetData(string command, string name, ref string message) {
            bool value = false;
            Match attributes = Regex.Match(command, @"\{(.*?)\}");
            if (attributes.Success) {
                string normalizedAttributes = NormalizeAttributes(attributes);
                List<string> atts = new List<string>(normalizedAttributes.Split(','));
                KeyValuePair<string, string> data = new KeyValuePair<string, string>();
                int index = 0;
                foreach(string attribute in atts) {
                    try {
                        if(!attribute.Equals(" ")) {
                            string a = attribute.Trim();
                            string type = tables[name].columns[index].Value;
                            if (type.Contains("INT")) {
                                InsertInt(data, a, type, ref message);
                            } else {
                                //InsertString();
                            }
                            index++;
                        }
                    } catch(Exception) {
                        message = "Los datos a ingresar no son los correctos";
                        value = false;
                    }
                }
            } else {
                message = "No se han encotrado los datos ha insertar";
                value = false;
            }
            return value;
        }

        /// <summary>Insert a int in the new tree</summary>
        /// <param name="data">The node for the tree.</param>
        /// <param name="attribute">The data of the field.</param>
        /// <param name="type">The type of the data.</param>
        /// <param name="message">The message to send back.</param>
        /// <returns>True if it is added.</returns>
        private bool InsertInt(KeyValuePair<string, string> data, string attribute, string type, ref string message) {
            bool value = false;
            if(int.TryParse(attribute, out int n)) {
                if(type.Contains("PRIMARY KEY")) {
                    KeyValuePair<string, string> newValue = new KeyValuePair<string, string>(attribute, data.Value);
                    data = newValue;
                } else {
                    KeyValuePair<string, string> newValue = new KeyValuePair<string, string>(attribute, data.Value);
                }
            } else {
                message = "Hay campos numericos en los que estas tratando de ingresar letras";
                value = false;
            }
            return value;
        }

    }

}
