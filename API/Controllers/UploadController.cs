using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using CSVUploader.Resources.Helpers;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace CSVUploader.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        SqlConnection con;
        string sqlconn;
        IConfiguration _configuration;
        public UploadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // POST api/<UploadController>
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Post()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Files");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    DataTable tblcsv = new DataTable();
                    string[] ReadCSV = System.IO.File.ReadAllText(dbPath).Split('\n');
                    //spliting row after new line
                    for (int i = 0; i < ReadCSV.Length; i++)
                    {
                        string csvRow = ReadCSV[i];
                        if (!string.IsNullOrEmpty(csvRow))
                        {
                            if (i != 0)
                                tblcsv.Rows.Add();
                            int count = 0;
                            foreach (string FileRec in csvRow.Split(','))
                            {
                                if (i == 0)
                                {
                                    tblcsv.Columns.Add(FileRec);
                                }
                                else
                                {
                                    tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                                    count++;
                                }
                            }
                            //Adding each row into datatable
                        }
                    }
                    //Calling insert Functions
                    InsertCSVRecords(Path.GetFileNameWithoutExtension(fullPath), tblcsv);
                    return Ok("Table created Successfully!!!");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        private void connection()
        {
            sqlconn = this._configuration.GetConnectionString("DefaultConnection");
            //sqlconn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            con = new SqlConnection(sqlconn);
        }
        public void InsertCSVRecords(string fileName, DataTable csvdt)
        {
            try
            {
                connection();
                //creating Table
                string createQuery = CreateTABLE(fileName, csvdt);
                //creating object of SqlBulkCopy
                SqlBulkCopy objbulk = new SqlBulkCopy(con);
                //assigning Destination table name
                objbulk.DestinationTableName = fileName;
                //Mapping Table column
                foreach (DataColumn column in csvdt.Columns)
                {
                    objbulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    var columnName = column.ColumnName.Replace("\r", "").ToLower();
                    if (columnName == "password")
                    {
                        foreach (DataRow row in csvdt.Rows)
                        {
                            row.SetField(column.ColumnName, PasswordHelper.GenerateHash(row.Field<string>(column.ColumnName), "mySalt"));
                        }
                    }
                }
                con.Open();
                //Create Table
                SqlCommand cmd = new SqlCommand(createQuery, con);
                cmd.ExecuteNonQuery();
                //Bulk Insert Values
                objbulk.WriteToServer(csvdt);
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static string CreateTABLE(string tableName, DataTable table)
        {
            string sqlsc;
            sqlsc = "CREATE TABLE " + tableName + "(";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
            return sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
        }
    }
}